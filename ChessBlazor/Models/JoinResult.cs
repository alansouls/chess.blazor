namespace ChessBlazor.Models;

public record JoinResult(bool JoinedAsWhite, bool JoinedAsBlack, bool NeedPassword, string? Password);