namespace MyChess.Client.Models;

public class ChessBoardDraw
{
    public List<ChessBoardGraphics> Pieces { get; set; } = new();

    public List<ChessBoardPosition> AvailableMoves { get; set; } = new();

    public List<ChessBoardGraphics> Highlights { get; set; } = new();

    public List<ChessBoardAnimation> Animations { get; set; } = new();
}
