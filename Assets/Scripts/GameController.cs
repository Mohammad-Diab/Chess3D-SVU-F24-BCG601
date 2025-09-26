using Chess;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    private GameObject lastHovered;
    private bool isGameOn = false;

    public GameObject newGameMenu;
    public Messaging _messaging;
    public CameraOrbitNewInput _camera;
    public GameObject chessBoardPanel;

    public GameObject tilesParent;
    public GameObject[] piecesParents;


    [SerializeField]
    private TMP_Text gameResult;

    public UIFader faderButton;
    public BoardFader boardFader;
    public ItemsDropper boardDropper;
    public ItemsDropper piecesDropper;
    public float delayBeforeBoard = 0.5f;

    private Dictionary<string, Renderer> _tiles;
    private Dictionary<string, Renderer> _pieces;
    private List<Renderer> _promotedPiecesList;
    private Dictionary<string, Vector3> _piecesDefaultPositions;

    public Mesh queen,
        rook,
        bishop,
        knight;

    public Material white,
        black;


    public string SelectedPeice { get; set; }
    public List<Move> AllowedMoves { get; set; }

    private Color blackTilesColor = new Color(0.251f, 0.251f, 0.251f);

    void Start()
    {
        chessBoardPanel.SetActive(false);
    }


    void Update()
    {
        if (!isGameOn) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject current = hit.collider.gameObject;

            if (current != lastHovered)
            {
                if (lastHovered != null)
                    lastHovered.SendMessage("OnTileExit", SendMessageOptions.DontRequireReceiver);

                current.SendMessage("OnTileEnter", SendMessageOptions.DontRequireReceiver);
                lastHovered = current;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                current.SendMessage("OnTileClick", SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            if (lastHovered != null)
            {
                lastHovered.SendMessage("OnTileExit", SendMessageOptions.DontRequireReceiver);
                lastHovered = null;
            }
        }
    }

    public void OnNewGameClicked()
    {
        StartNewGame();
        foreach (var item in _pieces)
        {
            item.Value.gameObject.SetActive(false);
        }
        foreach (var item in _tiles)
        {
            item.Value.gameObject.SetActive(false);
        }
        StartCoroutine(StartGameSequence());
        isGameOn = true;
    }

    public void OnSurenderClick()
    {
        ChessLogic.Surrender();
        GameOver();
    }

    public void GameOver()
    {
        isGameOn = false;
        faderButton.FadeIn();
        var gameInfo = ChessLogic.GetEndGameInfo();
        if (gameInfo != null)
        {
            faderButton.FadeIn();
            bool isWhiteWin = gameInfo.WonSide == PieceColor.White;
            switch (gameInfo.EndgameType)
            {
                case EndgameType.Checkmate:
                    if (isWhiteWin)
                        gameResult.text = "Checkmate! Black Wins!";
                    else
                        gameResult.text = "Checkmate! White Wins!";
                    break;

                case EndgameType.Stalemate:
                    gameResult.text = "Draw! Stalemate!";
                    break;

                case EndgameType.Resigned:
                    if (isWhiteWin)
                        gameResult.text = "Black Resigned! White Wins!";
                    else
                        gameResult.text = "White Resigned! Black Wins!";
                    break;

                case EndgameType.InsufficientMaterial:
                    gameResult.text = "Draw! Insufficient Material!";
                    break;
                case EndgameType.FiftyMoveRule:
                    gameResult.text = "Draw! Fifty Move Rule!";
                    break;
                case EndgameType.Repetition:
                    gameResult.text = "Draw! Threefold Repetition!";
                    break;
            }
        }
        
    }

    public void OnExitGameClicked()
    {
        Application.Quit();
    }

    private IEnumerator StartGameSequence()
    {
        _messaging.SetPanal(true);
        _messaging.FlipTurn(IsWhiteTurn());
        _camera.FlipCamera(0.3f, 5f);
        faderButton.FadeOut();

        ResetItemsColor();

        foreach (var item in _pieces)
        {
            var defPos = _piecesDefaultPositions[item.Key];
            item.Value.transform.position = new Vector3(defPos.x, defPos.y, defPos.z);
        }
        yield return new WaitForSeconds(delayBeforeBoard);
        chessBoardPanel.SetActive(true);

        boardFader.FadeIn();
        yield return new WaitForSeconds(delayBeforeBoard);

        boardDropper.StartDropping();
        yield return new WaitForSeconds(delayBeforeBoard * 6);

        piecesDropper.StartDropping();
    }

    public void ShowMessage(string message)
    {
        _messaging.ShowMessage(message);
    }

    public void StartNewGame()
    {
        ChessLogic.StartNewGame();
        if(_promotedPiecesList is not null)
        {
            foreach (var item in _promotedPiecesList)
            {
                Destroy(item);
            }
        }

        _promotedPiecesList = new List<Renderer>();
        _tiles = new Dictionary<string, Renderer>();
        var lcoalTiles = tilesParent.GetComponentsInChildren<Renderer>(true);
        foreach (var item in lcoalTiles)
        {
            _tiles.Add(item.name.ToLower(), item);
        }

        _pieces = new Dictionary<string, Renderer>();
        foreach (var piecesGroup in piecesParents)
        {
            var lcoalPieces = piecesGroup.GetComponentsInChildren<Renderer>(true);
            foreach (var piece in lcoalPieces)
            {
                _pieces.Add(piece.name.ToLower(), piece);
            }
        }

        if (_piecesDefaultPositions == null)
        {
            _piecesDefaultPositions = new Dictionary<string, Vector3>();
            foreach (var item in _pieces)
            {
                _piecesDefaultPositions.Add(item.Key, new Vector3(item.Value.transform.position.x, item.Value.transform.position.y, item.Value.transform.position.z));
            }
        }
    }

    public List<Move> GetAllowedMoves(string fromSquare)
    {
        return ChessLogic.GetAllowedMoves(fromSquare);
    }

    public Piece GetPieceAtTile(string square)
    {
        return ChessLogic.GetPieceAtTile(square);
    }

    public bool MakeMove(Move move)
    {
        if (ChessLogic.MakeMove(move))
        {
            _messaging.FlipTurn(ChessLogic.IsWhiteTurn);
            _camera.FlipCamera(0.3f, 5f);
            if (move.IsMate || ChessLogic.IsGameOver)
            {
                GameOver();
            }
            return true;
        }
        return false;
    }

    public Piece[] GetCapturedPieces(PieceColor color)
    {
        return ChessLogic.GetCapturedPieces(color);
    }

    public bool IsWhiteKingChecked()
    {
        return ChessLogic.IsWhiteKingChecked;
    }

    public string GetWhiteKingPosition()
    {
        return ChessLogic.WhiteKingPosition;
    }

    public bool IsBlackKingChecked()
    {
        return ChessLogic.IsBlackKingChecked;
    }

    public string GetBlackKingPosition()
    {
        return ChessLogic.BlackKingPosition;
    }

    public bool IsWhiteTurn()
    {
        return ChessLogic.IsWhiteTurn;
    }

    public Renderer GetTile(string key)
    {
        return _tiles[key.ToLower()];
    }

    public Renderer GetPiece(string key, bool isPromoted = false)
    {
        return _pieces[$"{(isPromoted ? "_p" : "")}{key.ToLower()}"];
    }

    public void AddPiece(string key, Renderer piece, bool isPromoted = true)
    {
        _pieces.Add($"{key.ToLower()}{(isPromoted ? "_p" : "")}", piece);
    }

    public void RemovePiece(string key)
    {
        _pieces.Remove(key.ToLower());
    }

    public Renderer GetPieceByPos(float x, float z)
    {
        foreach (var piece in _pieces)
        {
            if (piece.Value.transform.position.x == x && piece.Value.transform.position.z == z)
                if (piece.Value.gameObject.activeSelf)
                    return piece.Value;
        }
        return null;
    }

    public Renderer GetTileByPos(float x, float z)
    {
        foreach (var tile in _tiles)
        {
            if (tile.Value.transform.position.x == x && tile.Value.transform.position.z == z)
                return tile.Value;
        }
        return null;
    }

    public void ResetItemsColor()
    {
        foreach (var item in _tiles)
        {
            var tileName = item.Value.name.ToLower();
            if (IsBlackKingChecked())
            {
                var balckKingPos = GetTile(GetBlackKingPosition()).transform.position;
                if (balckKingPos.x == item.Value.transform.position.x && balckKingPos.z == item.Value.transform.position.z)
                {
                    item.Value.material.SetColor("_BaseColor", Color.darkRed);
                    continue;
                }
            }
            if (IsWhiteKingChecked())
            {
                var whiteKingPos = GetTile(GetWhiteKingPosition()).transform.position;
                if (whiteKingPos.x == item.Value.transform.position.x && whiteKingPos.z == item.Value.transform.position.z)
                {
                    item.Value.material.SetColor("_BaseColor", Color.darkRed);
                    continue;
                }
            }
            bool isWhite = (tileName[0] - 'a' + 1 + (tileName[1] - '0')) % 2 != 0;
            if (isWhite) item.Value.material.SetColor("_BaseColor", Color.white);
            else item.Value.material.SetColor("_BaseColor", blackTilesColor);
        }
    }

    public Renderer CreatePiece(string name, Mesh mesh, Material mat)
    {
        GameObject piece = new(name);

        MeshFilter mf = piece.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = piece.AddComponent<MeshRenderer>();
        mr.material = mat;

        mr.transform.localScale = new Vector3(8, 8, 8);

        return mr;
    }


    public Renderer GetPromotedPiece(bool isWhite, PromotionType promotionType, string name)
    {
        var mesh = promotionType switch
        {
            PromotionType.ToQueen => queen,
            PromotionType.ToRook => rook,
            PromotionType.ToBishop => bishop,
            PromotionType.ToKnight => knight,
            _ => queen,
        };
        var mat = isWhite ? white : black;
        var piece = CreatePiece(name, mesh, mat);
        _promotedPiecesList.Add(piece);
        return piece;
    }

}