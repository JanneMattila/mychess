namespace MyChess.Client.Models;

public class ChessBoardAnimation
{
    public ChessBoardPosition From { get; set; } = new();
    public ChessBoardPosition To { get; set; } = new();
    public string Data { get; set; } = string.Empty;
}
