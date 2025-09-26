using Chess;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TileControl : MonoBehaviour
{

    [Header("Materials")]
    private Material _normalMaterial;
    private Renderer _rend;
    public GameController _gameController;
    public static GameController GameControllerInstance;


    public Material hoverMaterial;

    [SerializeField] private PromotionUI promotionUI;

    void Start()
    {
        _rend = GetComponent<Renderer>();
        _normalMaterial = _rend.material;
    }

    void Awake()
    {
        lock ("gameController")
        {
            if (GameControllerInstance == null)
            {
                GameControllerInstance = _gameController;
            }
        }
        DontDestroyOnLoad(GameControllerInstance);
    }


    public void OnTileEnter()
    {
        hoverMaterial.color = _rend.material.color;
        _rend.material = hoverMaterial;
    }

    public void OnTileExit()
    {
        _rend.material = _normalMaterial;
    }

    public void OnTileClick()
    {
        GameControllerInstance.ResetItemsColor();
        if (GameControllerInstance.SelectedPeice == null)
        {
            var piece = GameControllerInstance.GetPieceAtTile(name.ToLower());
            if (piece == null) return;
            else if (piece.Color == PieceColor.White && !GameControllerInstance.IsWhiteTurn())
            {
                GameControllerInstance.ShowMessage("It's Black Turn");
                return;
            }
            else if (piece.Color == PieceColor.Black && GameControllerInstance.IsWhiteTurn())
            {
                GameControllerInstance.ShowMessage("It's White Turn");
                return;
            }

            GameControllerInstance.AllowedMoves = GameControllerInstance.GetAllowedMoves(name.ToLower());
            if (GameControllerInstance.AllowedMoves.Count == 0)
            {
                GameControllerInstance.ShowMessage("No Possible Moves");
                return;
            }
            GameControllerInstance.SelectedPeice = name.ToLower();
            foreach (var move in GameControllerInstance.AllowedMoves)
            {
                var tileMaterial = GameControllerInstance.GetTile(move.NewPosition.ToString()).material;
                if (move.IsCastling || move.IsPromotion)
                {
                    tileMaterial.SetColor("_BaseColor", Color.purple);
                }
                else if (move.IsEnPassant || move.CapturedPiece != null)
                {
                    tileMaterial.SetColor("_BaseColor", Color.tomato);
                }
                else
                {
                    tileMaterial.SetColor("_BaseColor", Color.royalBlue);
                }
            }
        }
        else
        {
            var moves = GameControllerInstance.AllowedMoves.FindAll(x => x.NewPosition.ToString() == name.ToLower() && x.OriginalPosition.ToString() == GameControllerInstance.SelectedPeice);
            if (moves.Count > 0)
            {
                if (moves[0].IsPromotion)
                {
                    promotionUI.Show(moves[0].Piece.Color, selectedType =>
                    {
                        var move = moves.Find(x => (x.Parameter as MovePromotion).PromotionType == selectedType);
                        MovePiece(move, selectedType);
                    });
                }
                else
                {
                    MovePiece(moves[0]);
                }
            }
            else
            {
                GameControllerInstance.ShowMessage("Invalid Move");
            }

            GameControllerInstance.SelectedPeice = null;
            GameControllerInstance.AllowedMoves = null;
        }
    }

    private void MovePiece(Move move, PromotionType? promotionType = null)
    {
        if (GameControllerInstance.MakeMove(move))
        {
            var origialPos = GameControllerInstance.GetTile(move.OriginalPosition.ToString()).transform.position;
            var newPos = GameControllerInstance.GetTile(move.NewPosition.ToString()).transform.position;
            var piece = GameControllerInstance.GetPieceByPos(origialPos.x, origialPos.z);
            if (move.IsCastling)
            {
                var rookPiece = GameControllerInstance.GetPiece($"{(move.Piece.Color == PieceColor.White ? "White" : "Black")}_rook_{((move.Parameter as MoveCastle).CastleType == CastleType.King ? "H" : "A")}");
                float zPos = newPos.z;
                if ((move.Parameter as MoveCastle).CastleType == CastleType.King) zPos -= 1;
                else zPos += 1;

                StartCoroutine(AnimatePiece(
                    rookPiece.transform,
                    new Vector3(newPos.x, piece.transform.position.y, zPos)));

            }
            if (move.IsCheck)
            {
                var kingPiece = move.Piece.Color == PieceColor.White ? GameControllerInstance.GetPiece("black_king") : GameControllerInstance.GetPiece("white_king");
                var kingTile = GameControllerInstance.GetTileByPos(kingPiece.transform.position.x, kingPiece.transform.position.z);
                kingTile.material.SetColor("_BaseColor", Color.darkRed);
            }
            if (move.CapturedPiece != null)
            {
                var capOrigialPos = GameControllerInstance.GetTile(move.NewPosition.ToString()).transform.position;
                if (move.Parameter is MoveEnPassant moveEnPassant)
                {
                    var tile = GameControllerInstance.GetTile(moveEnPassant.CapturedPawnPosition.ToString());
                    capOrigialPos = tile.transform.position;
                }

                var capPiece = GameControllerInstance.GetPieceByPos(capOrigialPos.x, capOrigialPos.z);
                int capturedPiecesCount = GameControllerInstance.GetCapturedPieces(move.CapturedPiece.Color).Length;

                _gameController.RemovePiece(capPiece.name);

                StartCoroutine(AnimatePiece(
                    capPiece.transform,
                    new Vector3(
                    (capturedPiecesCount > 8 ? capturedPiecesCount - 8 : capturedPiecesCount) - 1,
                    capPiece.transform.position.y - 0.1f,
                    (capturedPiecesCount > 8 ? move.CapturedPiece.Color == PieceColor.White ? 1 : -1 : 0) + (move.CapturedPiece.Color == PieceColor.White ? 8 : -1))));
            }
            if (move.IsPromotion && promotionType.HasValue)
            {
                var pieceType = promotionType.Value switch
                {
                    PromotionType.ToQueen => "Queen",
                    PromotionType.ToRook => "Rook",
                    PromotionType.ToBishop => "Bishop",
                    PromotionType.ToKnight => "Knight",
                    _ => "Queen",
                };
                var colorStr = move.Piece.Color == PieceColor.White ? "White" : "Black";

                var pieceLetter = promotionType.Value switch
                {
                    PromotionType.ToQueen => "",
                    PromotionType.ToRook => "_A",
                    PromotionType.ToBishop => "_C",
                    PromotionType.ToKnight => "_B",
                    _ => "",
                };

                var newPieceName =  $"{colorStr}_{pieceType}_{piece.name[^1]}";
                var promotedPiece = GameControllerInstance.GetPromotedPiece(move.Piece.Color == PieceColor.White, promotionType.Value, newPieceName);

                // var promotedPiece = Instantiate(promotedPieceRef, promotedPieceRef.transform.parent);
                // promotedPiece.gameObject.SetActive(true);
                // promotedPiece.name = newPieceName;

                StartCoroutine(AnimatePiece(
                promotedPiece.transform,
                new Vector3(piece.transform.position.x, piece.transform.position.y, piece.transform.position.z)));

                piece.gameObject.SetActive(false);
                GameControllerInstance.RemovePiece(piece.name);
                GameControllerInstance.AddPiece(newPieceName.ToLower(), promotedPiece);
                piece = promotedPiece;
            }

            StartCoroutine(AnimatePiece(
                piece.transform,
                new Vector3(newPos.x, piece.transform.position.y, newPos.z)));
        }
    }

    public IEnumerator AnimatePiece(Transform piece, Vector3 targetPos, float duration = 0.33f, float jumpHeight = 1f)
    {
        Vector3 startPos = piece.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(startPos, targetPos, t);
            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            piece.position = new Vector3(horizontal.x, startPos.y + yOffset, horizontal.z);

            elapsed += Time.deltaTime;
            yield return null;
        }
        piece.position = targetPos;
    }

}
