using ChessBlazor.Models;

namespace ChessBlazor.Logic;

public class GameManager
{
    private bool _isPlaying;
    private readonly Piece?[][] _board;
    private readonly bool[][] _enPassantTargets;
    private readonly bool[][] _litSquares;
    private (int I, int J)? _activePosition;
    private (int I, int J) _positionToPromote;
    private readonly bool _isSimulated;

    public GameManager()
    {
        _isSimulated = false;
        _board = new Piece[8][];
        for (int i = 0; i < 8; ++i)
        {
            _board[i] = new Piece[8];
        }

        _litSquares = new bool[8][];
        for (int i = 0; i < 8; ++i)
        {
            _litSquares[i] = new bool[8];
        }

        _enPassantTargets = new bool[8][];
        for (int i = 0; i < 8; ++i)
        {
            _enPassantTargets[i] = new bool[8];
        }

        _board[0][0] = new Piece("Tower", "/images/pieces/tower-black.png", "black");
        _board[0][1] = new Piece("Knight", "/images/pieces/knight-black.png", "black");
        _board[0][2] = new Piece("Bishop", "/images/pieces/bishop-black.png", "black");
        _board[0][3] = new Piece("Queen", "/images/pieces/queen-black.png", "black");
        _board[0][4] = new Piece("King", "/images/pieces/king-black.png", "black");
        _board[0][5] = new Piece("Bishop", "/images/pieces/bishop-black.png", "black");
        _board[0][6] = new Piece("Knight", "/images/pieces/knight-black.png", "black");
        _board[0][7] = new Piece("Tower", "/images/pieces/tower-black.png", "black");
        for (int i = 0; i < 8; ++i)
        {
            _board[1][i] = new Piece("Pawn", "/images/pieces/pawn-black.png", "black");
        }

        for (int i = 0; i < 8; ++i)
        {
            _board[6][i] = new Piece("Pawn", "/images/pieces/pawn-white.png", "white");
        }

        _board[7][0] = new Piece("Tower", "/images/pieces/tower-white.png", "white");
        _board[7][1] = new Piece("Knight", "/images/pieces/knight-white.png", "white");
        _board[7][2] = new Piece("Bishop", "/images/pieces/bishop-white.png", "white");
        _board[7][3] = new Piece("Queen", "/images/pieces/queen-white.png", "white");
        _board[7][4] = new Piece("King", "/images/pieces/king-white.png", "white");
        _board[7][5] = new Piece("Bishop", "/images/pieces/bishop-white.png", "white");
        _board[7][6] = new Piece("Knight", "/images/pieces/knight-white.png", "white");
        _board[7][7] = new Piece("Tower", "/images/pieces/tower-white.png", "white");

        IsWhiteTurn = true;
        _isPlaying = false;
        _activePosition = null;
        IsPromoting = false;

        PiecesTakenByBlack = new List<Piece>();
        PiecesTakenByWhite = new List<Piece>();

        _positionToPromote = (0, 0);

        ColorPromoting = string.Empty;
    }

    private GameManager(Piece?[][] board)
    {
        _isSimulated = true;
        _board = board;

        _litSquares = new bool[8][];
        for (int i = 0; i < 8; ++i)
        {
            _litSquares[i] = new bool[8];
        }

        _enPassantTargets = new bool[8][];
        for (int i = 0; i < 8; ++i)
        {
            _enPassantTargets[i] = new bool[8];
        }

        IsWhiteTurn = true;
        _isPlaying = false;
        _activePosition = null;
        IsPromoting = false;

        PiecesTakenByBlack = new List<Piece>();
        PiecesTakenByWhite = new List<Piece>();

        _positionToPromote = (0, 0);

        ColorPromoting = string.Empty;
    }
    
    public bool IsWhiteTurn { get; private set; }
    
    public string? Winner { get; private set; }

    public List<Piece> PiecesTakenByWhite { get; private set; }
    public List<Piece> PiecesTakenByBlack { get; private set; }

    public bool IsPromoting { get; private set; }
    public string ColorPromoting { get; private set; }

    public Piece? GetPiece(int i, int j) => _board[i][j];

    public bool PositionIsValid((int I, int J) position) => position.I is >= 0 and < 8 && position.J is >= 0 and < 8;

    public List<(int I, int J)> GetSquaresToMove((int I, int J) position, Piece piece)
    {
        var result = new List<(int I, int J)>();

        if (piece.Name == "Pawn")
        {
            GetPawnSquares(position, piece, result);
        }
        else if (piece.Name == "Tower")
        {
            GetTowerSquares(position, piece, result);
        }
        else if (piece.Name == "Bishop")
        {
            GetBishopSquares(position, piece, result);
        }
        else if (piece.Name == "Knight")
        {
            GetKnightSquares(position, piece, result);
        }
        else if (piece.Name == "Queen")
        {
            GetQueenSquares(position, piece, result);
        }
        else
        {
            GetKingSquares(position, piece, result);
        }

        return result;
    }

    private void GetKingSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var directions = new[]
        {
            (1, 1),
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1)
        };
        foreach (var direction in directions)
        {
            result.AddRange(GetSquaresInDirection(position, piece, direction, true));
        }

        if (!IsInCheck(piece.Color))
        {
            result.AddRange(GetSquaresToLitForCastle(position, piece));
        }
    }

    private List<(int, int)> GetSquaresToLitForCastle((int I, int J) position, Piece piece)
    {
        var result = new List<(int, int)>();
        if (!piece.HasMoved)
        {
            var pieceMostLeft = GetMostLeftForCastle(position);

            var pieceMostRight = GetMostRightForCastle(position);

            if (pieceMostLeft is not null && !pieceMostLeft.HasMoved)
            {
                result.Add((position.I, position.J - 2));
            }

            if (pieceMostRight is not null && !pieceMostRight.HasMoved)
            {
                result.Add((position.I, position.J + 2));
            }
        }

        return result;
    }

    private Piece? GetMostLeftForCastle((int I, int J) position)
    {
        Piece? pieceMostLeft = null;
        int i = position.J - 1;
        while (i >= 0)
        {
            var checkPiece = GetPiece(position.I, i);
            if (i == 0)
            {
                pieceMostLeft = checkPiece;
            }
            else if (checkPiece is not null)
            {
                break;
            }

            i--;
        }

        return pieceMostLeft;
    }

    private Piece? GetMostRightForCastle((int I, int J) position)
    {
        Piece? pieceMostRight = null;
        int i = position.J + 1;
        while (i < 8)
        {
            var checkPiece = GetPiece(position.I, i);
            if (i == 7)
            {
                pieceMostRight = checkPiece;
            }
            else if (checkPiece is not null)
            {
                break;
            }

            i++;
        }

        return pieceMostRight;
    }

    private void GetQueenSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var directions = new[]
        {
            (1, 1),
            (-1, -1),
            (-1, 1),
            (1, -1),
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1)
        };
        foreach (var direction in directions)
        {
            result.AddRange(GetSquaresInDirection(position, piece, direction));
        }
    }

    private void GetBishopSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var directions = new[]
        {
            (1, 1),
            (-1, -1),
            (-1, 1),
            (1, -1)
        };
        foreach (var direction in directions)
        {
            result.AddRange(GetSquaresInDirection(position, piece, direction));
        }
    }

    private void GetTowerSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var directions = new[]
        {
            (1, 0),
            (0, 1),
            (-1, 0),
            (0, -1)
        };
        foreach (var direction in directions)
        {
            result.AddRange(GetSquaresInDirection(position, piece, direction));
        }
    }

    private List<(int I, int J)> GetSquaresInDirection((int I, int J) position, Piece piece, (int, int) direction,
        bool limitToOne = false)
    {
        var result = new List<(int I, int J)>();
        Piece? blockingPiece = null;
        var startingPosition = position;
        while (blockingPiece is null)
        {
            var newPosition = (startingPosition.I + direction.Item1, startingPosition.J + direction.Item2);
            if (!PositionIsValid(newPosition))
            {
                break;
            }

            blockingPiece = GetPiece(newPosition.Item1, newPosition.Item2);
            if (blockingPiece is null || blockingPiece.Color != piece.Color)
            {
                result.Add((newPosition));
            }

            startingPosition = newPosition;

            if (limitToOne)
                return result;
        }

        return result;
    }

    private void GetKnightSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var possiblePositions = new List<(int I, int J)>()
        {
            (position.I + 1, position.J + 2),
            (position.I + 1, position.J - 2),
            (position.I - 1, position.J + 2),
            (position.I - 1, position.J - 2),
            (position.I + 2, position.J + 1),
            (position.I + 2, position.J - 1),
            (position.I - 2, position.J + 1),
            (position.I - 2, position.J - 1),
        };

        foreach (var positionToMove in possiblePositions)
        {
            if (PositionIsValid(positionToMove))
            {
                var pieceToTake = GetPiece(positionToMove.Item1, positionToMove.Item2);
                if (pieceToTake is null || pieceToTake.Color != piece.Color)
                {
                    result.Add(positionToMove);
                }
            }
        }
    }

    private void GetPawnSquares((int I, int J) position, Piece piece, List<(int I, int J)> result)
    {
        var multiplier = piece.Color == "black" ? 1 : -1;
        var positionToAdd = (position.I + (multiplier * 1), position.J);
        Piece? blockingPiece = null;
        if (PositionIsValid(positionToAdd))
        {
            var pieceToTake = GetPiece(positionToAdd.Item1, positionToAdd.Item2);
            blockingPiece = pieceToTake;
            if (pieceToTake is null)
            {
                result.Add(positionToAdd);
            }
        }

        if (!piece.HasMoved && blockingPiece is null)
        {
            positionToAdd = (position.I + (multiplier * 2), position.J);
            if (PositionIsValid(positionToAdd))
            {
                var pieceToTake = GetPiece(positionToAdd.Item1, positionToAdd.Item2);
                if (pieceToTake is null)
                {
                    result.Add(positionToAdd);
                }
            }
        }

        var positionToTake = (position.I + (multiplier * 1), position.J + 1);
        if (PositionIsValid(positionToTake))
        {
            var canEnPassant = _enPassantTargets[position.I][position.J + 1];
            var pieceToTake = GetPiece(positionToTake.Item1, positionToTake.Item2);
            if (canEnPassant || pieceToTake is not null && pieceToTake.Color != piece.Color)
            {
                result.Add(positionToTake);
            }
        }

        positionToTake = (position.I + (multiplier * 1), position.J - 1);
        if (PositionIsValid(positionToTake))
        {
            var canEnPassant = _enPassantTargets[position.I][position.J - 1];
            var pieceToTake = GetPiece(positionToTake.Item1, positionToTake.Item2);
            if (canEnPassant || pieceToTake is not null && pieceToTake.Color != piece.Color)
            {
                result.Add(positionToTake);
            }
        }
    }

    public bool ClickSquare((int I, int J) position)
    {
        if (IsPromoting)
            return false;
        if (!_isPlaying)
        {
            var clickedPiece = GetPiece(position.I, position.J);

            if (clickedPiece is null)
                return false;

            if (IsWhiteTurn && clickedPiece.Color == "black")
                return false;
            if (!IsWhiteTurn && clickedPiece.Color == "white")
                return false;

            var squaresToMove = GetSquaresToMove(position, clickedPiece);

            squaresToMove = squaresToMove.Where(s => !ResultInCheck(position, s, IsWhiteTurn))
                .ToList();

            if (squaresToMove.Count == 0)
                return false;

            LightUpSquares(squaresToMove);

            _activePosition = position;

            _isPlaying = true;

            return true;
        }
        else
        {
            if (!_isSimulated && !SquareIsLit(position.I, position.J))
            {
                _isPlaying = false;
                UnLightUpSquares();
                ClickSquare(position);
            }
            else
            {
                var pieceToMove = _board[_activePosition!.Value.I][_activePosition.Value.J]!;
                var pieceToTake = CheckEnPassant(position, pieceToMove);
                ClearEnPassantTargets();

                if (pieceToMove.Name == "Pawn")
                {
                    if ((IsWhiteTurn && position.I == 0) ||
                        (!IsWhiteTurn && position.I == 7))
                    {
                        IsPromoting = true;
                        _positionToPromote = position;
                        ColorPromoting = IsWhiteTurn ? "white" : "black";
                    }
                }

                HandleCastle(position, pieceToMove, _activePosition.Value);
                pieceToMove = HandleFirstMove(position, pieceToMove);

                pieceToTake = pieceToTake ?? _board[position.I][position.J];

                if (pieceToTake is not null)
                {
                    if (IsWhiteTurn)
                    {
                        PiecesTakenByWhite.Add(pieceToTake);
                    }
                    else
                    {
                        PiecesTakenByBlack.Add(pieceToTake);
                    }
                }

                _board[position.I][position.J] = pieceToMove;
                _board[_activePosition!.Value.I][_activePosition.Value.J] = null;

                _isPlaying = false;
                UnLightUpSquares();
                _activePosition = null;
                IsWhiteTurn = !IsWhiteTurn;

                if (!_isSimulated &&
                    IsInCheck(IsWhiteTurn ? "white" : "black") &&
                    IsCheckMate(IsWhiteTurn ? "white" : "black"))
                {
                    Winner = IsWhiteTurn ? "black" : "white";
                }
            }

            return true;
        }
    }

    private Piece HandleFirstMove((int I, int J) position, Piece pieceToMove)
    {
        if (!pieceToMove.HasMoved)
        {
            pieceToMove = pieceToMove with { HasMoved = true };
            CheckEnableEnPassant(position, pieceToMove);
        }

        return pieceToMove;
    }

    private void CheckEnableEnPassant((int I, int J) position, Piece pieceToMove)
    {
        if (pieceToMove.Name == "Pawn")
        {
            if (Math.Abs(_activePosition!.Value.I - position.I) == 2)
            {
                _enPassantTargets[position.I][position.J] = true;
            }
        }
    }

    private Piece? CheckEnPassant((int I, int J) position, Piece pieceToMove)
    {
        if (pieceToMove.Name == "Pawn" && _enPassantTargets[_activePosition!.Value.I][position.J])
        {
            var pieceToTake = _board[_activePosition.Value.I][position.J];
            _board[_activePosition.Value.I][position.J] = null;
            return pieceToTake;
        }

        return null;
    }

    private void HandleCastle((int I, int J) position, Piece pieceToMove, (int I, int J) activePosition)
    {
        if (pieceToMove.Name == "King")
        {
            if (Math.Abs(position.J - activePosition.J) > 1)
            {
                if (position.J == 6)
                {
                    var tower = GetPiece(activePosition.I, 7)!;
                    tower = tower with { HasMoved = true };
                    _board[activePosition.I][7] = null;
                    _board[activePosition.I][5] = tower;
                }

                if (position.J == 2)
                {
                    var tower = GetPiece(activePosition.I, 0)!;
                    _board[activePosition.I][0] = null;
                    _board[activePosition.I][3] = tower;
                }
            }
        }
    }

    private void LightUpSquares(List<(int I, int J)> squaresToLightUp)
    {
        foreach (var squareToLightUp in squaresToLightUp)
        {
            _litSquares[squareToLightUp.I][squareToLightUp.J] = true;
        }
    }

    private void UnLightUpSquares()
    {
        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                _litSquares[i][j] = false;
            }
        }
    }

    private void ClearEnPassantTargets()
    {
        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                _enPassantTargets[i][j] = false;
            }
        }
    }

    public bool SquareIsLit(int i, int j) => _litSquares[i][j];

    public void PromoteTo(string name)
    {
        if (name != "Queen" && name != "Knight" && name != "Bishop" && name != "Tower")
            return;

        var pieceToPromote = _board[_positionToPromote.I][_positionToPromote.J]!;

        var image = $"/images/pieces/{name.ToLower()}-{ColorPromoting}.png";

        pieceToPromote = pieceToPromote with
        {
            Name = name,
            Image = image
        };

        _board[_positionToPromote.I][_positionToPromote.J] = pieceToPromote;
        IsPromoting = false;
    }

    private bool ResultInCheck((int I, int J) fromPosition, (int I, int J) toPosition, bool isWhiteTurn)
    {
        var simulated = new GameManager(CloneBoard());
        simulated._isPlaying = true;
        simulated._activePosition = fromPosition;
        simulated.IsWhiteTurn = isWhiteTurn;

        simulated.ClickSquare(toPosition);

        return simulated.IsInCheck(isWhiteTurn ? "white" : "black");
    }

    private bool IsInCheck(string color)
    {
        var kingPosition = FindKingPosition(color);

        var attackingColor = color == "white" ? "black" : "white";

        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                var piece = GetPiece(i, j);

                if (piece is not null && piece.Color == attackingColor)
                {
                    var squaresToMove = GetSquaresToMove((i, j), piece);

                    if (squaresToMove.Contains(kingPosition))
                        return true;
                }
            }
        }

        return false;
    }

    private bool IsCheckMate(string targetColor)
    {
        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                var piece = GetPiece(i, j);

                if (piece is not null && piece.Color == targetColor)
                {
                    var squaresToMove = GetSquaresToMove((i, j), piece);

                    squaresToMove = squaresToMove
                        .Where(s => !ResultInCheck((i, j), s, targetColor == "white"))
                        .ToList();

                    if (squaresToMove.Count > 0)
                        return false;
                }
            }
        }

        return true;
    }

    private Piece?[][] CloneBoard()
    {
        var result = new Piece?[8][];

        for (int i = 0; i < 8; ++i)
        {
            result[i] = new Piece?[8];
            for (int j = 0; j < 8; ++j)
            {
                result[i][j] = _board[i][j];
            }
        }

        return result;
    }

    private (int I, int J) FindKingPosition(string colorToFind)
    {
        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                var piece = _board[i][j];

                if (piece is not null && piece.Name == "King" && piece.Color == colorToFind)
                {
                    return (i, j);
                }
            }
        }

        throw new Exception("Where's the king?");
    }
}