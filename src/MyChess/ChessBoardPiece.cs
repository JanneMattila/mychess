namespace MyChess
{
    public class ChessBoardPiece
    {
        public PiecePlayer Player { get; private set; }

        public PieceRank Rank { get; private set; }

        public static ChessBoardPiece Empty
        {
            get
            {
                return new ChessBoardPiece(PiecePlayer.None, PieceRank.None);
            }
        }

        public ChessBoardPiece(PiecePlayer player, PieceRank rank)
        {
            Player = player;
            Rank = rank;
        }
    }
}
