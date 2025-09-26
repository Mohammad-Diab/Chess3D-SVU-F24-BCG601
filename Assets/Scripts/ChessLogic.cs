using Chess;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ChessLogic
{

    private static ChessBoard _board;
    public static bool IsWhiteTurn => (_board?.Turn ?? PieceColor.White) == PieceColor.White;

    public static bool IsGameOver => _board?.IsEndGame ?? true;

    public static bool IsBlackKingChecked => _board?.BlackKingChecked ?? false;
    public static bool IsWhiteKingChecked => _board?.WhiteKingChecked ?? false;

    public static string BlackKingPosition => _board?.BlackKing.ToString().ToLower() ?? "e8";
    public static string WhiteKingPosition => _board?.WhiteKing.ToString().ToLower() ?? "e1";

    public static void StartNewGame()
    {
        _board = new ChessBoard()
        {
            AutoEndgameRules = AutoEndgameRules.All
        };
    }

    public static List<Move> GetAllowedMoves(string fromSquare)
    {
        try
        {
            return _board.Moves(new Position(fromSquare), false, true).ToList();
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return new List<Move>();
        }
    }

    public static Piece GetPieceAtTile(string square)
    {
        try
        {
            return _board[square];
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return null;
        }
    }

    public static bool MakeMove(Move move)
    {
        try
        {
            bool result = _board.Move(move);
            return result;
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return false;
        }
    }

    public static void Surrender()
    {
        try
        {
            _board.Resign(IsWhiteTurn ? PieceColor.White : PieceColor.Black);
        }
        catch (Exception ex)
        {
            var message = ex.Message;
        }
    }

    public static EndGameInfo GetEndGameInfo()
    {
        try
        {
            var result = _board.EndGame;
            return result;
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return null;
        }
    }

    public static Piece[] GetCapturedPieces(PieceColor color)
    {
        try
        {
            var result = color == PieceColor.White ? _board.CapturedWhite : _board.CapturedBlack;
            return result;
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return new Piece[0];
        }

    }

    public static (bool isEnded, string result) GetGameState()
    {
        if (_board.IsEndGame)
            return (true, _board.EndGame.ToString());
        return (false, "Playing");
    }

    public static bool IsCheck()
    {
        return _board.BlackKingChecked || _board.WhiteKingChecked;
    }

    public static string GetFen()
    {
        return _board.ToFen();
    }
}