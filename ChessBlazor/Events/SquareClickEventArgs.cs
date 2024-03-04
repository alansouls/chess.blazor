using Microsoft.AspNetCore.Components.Web;

namespace ChessBlazor.Events;

public class SquareClickEventArgs
{
    public required MouseEventArgs MouseEventArgs { get; init; }
    
    public required (int I, int J) Position { get; init; }
}