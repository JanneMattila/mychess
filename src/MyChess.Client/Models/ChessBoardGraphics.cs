namespace MyChess.Client.Models
{
    public class ChessBoardGraphics
    {
        public bool MoveAvailable { get; set; }
        public string LastMove { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }
}
