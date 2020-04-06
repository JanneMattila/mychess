namespace MyChess
{
    public class ChessBoardChange
    {
        public ChessBoardLocation Location { get; private set; }

        public PieceSelection Selection { get; private set; }

        public ChessBoardChange(int horizontalLocation, int verticalLocation, PieceSelection pieceSelection)
        {
            Location = new ChessBoardLocation(horizontalLocation, verticalLocation);
            Selection = pieceSelection;
        }
    }
}
