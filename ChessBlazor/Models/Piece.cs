namespace ChessBlazor.Models;

public record Piece(string Name, string Image, string Color, bool HasMoved = false);