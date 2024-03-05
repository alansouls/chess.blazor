using ChessBlazor.Models;

namespace ChessBlazor.Logic;

public class GameServer
{
    private readonly Dictionary<int, GameRoom> _roomsById;
    private readonly Dictionary<int, GameManager> _gameManagers;

    public GameServer()
    {
        _roomsById = new Dictionary<int, GameRoom>();
        _gameManagers = new Dictionary<int, GameManager>();
    }

    public GameManager? GetGameManager(int roomId, bool isWhite, string password)
    {
        var gameRoom = _roomsById.GetValueOrDefault(roomId);

        if (gameRoom is null)
            return null;

        if (isWhite && gameRoom.WhitePassword != password)
            return null;

        if (!isWhite && gameRoom.BlackPassword != password)
            return null;

        return _gameManagers.GetValueOrDefault(roomId);
    }

    public JoinResult JoinGame(int gameId, string? preferenceColor = null, string? password = null)
    {
        var room = _roomsById.GetValueOrDefault(gameId);

        if (room is null)
        {
            room = new GameRoom(gameId, null, null);
            _gameManagers.Add(gameId, new GameManager());
        }

        if (preferenceColor is null or "white" && room.WhitePassword is null)
        {
            var whitePassword = GeneratePassword();
            room = room with { WhitePassword = whitePassword };

            _roomsById[gameId] = room;

            return new JoinResult(true, false, false, whitePassword);
        }

        if (preferenceColor == "white")
        {
            return new JoinResult(false, false, true, null);
        }

        if (preferenceColor is null or "black" && room.BlackPassword is null)
        {
            var blackPassword = GeneratePassword();
            room = room with { BlackPassword = blackPassword };

            _roomsById[gameId] = room;

            return new JoinResult(false, true, false, blackPassword);
        }

        return new JoinResult(false, false, true, null);
    }

    private static string GeneratePassword()
    {
        return new string(Enumerable.Repeat(1, 20)
            .Select(s => (char)Random.Shared.Next((int)'a', (int)'z')).ToArray());
    }
}