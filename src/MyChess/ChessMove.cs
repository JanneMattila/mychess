using System;

namespace MyChess
{
    public class ChessMove : IComparable
    {
        public PiecePlayer Player { get; private set; }

        public PieceRank Rank { get; private set; }

        public ChessBoardLocation From { get; private set; }

        public ChessBoardLocation To { get; private set; }

        public ChessSpecialMove SpecialMove { get; private set; }

        public ChessMove(PieceRank rank, PiecePlayer player, int horizontalLocationFrom, int verticalLocationFrom, int horizontalLocationTo, int verticalLocationTo)
        {
            Rank = rank;
            Player = player;
            From = new ChessBoardLocation(horizontalLocationFrom, verticalLocationFrom);
            To = new ChessBoardLocation(horizontalLocationTo, verticalLocationTo);
            SpecialMove = ChessSpecialMove.None;
        }

        public ChessMove(PieceRank rank, PiecePlayer player, int horizontalLocationFrom, int verticalLocationFrom, int horizontalLocationTo, int verticalLocationTo, ChessSpecialMove specialMove)
        {
            Rank = rank;
            Player = player;
            From = new ChessBoardLocation(horizontalLocationFrom, verticalLocationFrom);
            To = new ChessBoardLocation(horizontalLocationTo, verticalLocationTo);
            SpecialMove = specialMove;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is ChessMove otherMove)
            {
                int compare = this.From.CompareTo(this.From);
                if (compare == 0)
                {
                    compare = this.To.CompareTo(otherMove.To);
                    if (compare == 0)
                    {
                        compare = this.Player.CompareTo(otherMove.Player);
                        if (compare == 0)
                        {
                            return this.Rank.CompareTo(otherMove.Rank);
                        }
                    }
                }

                return compare;
            }
            else
            {
                throw new ArgumentException("Object is not a ChessMove");
            }
        }

        public override string ToString()
        {
            char horizontalFrom = (char)((int)'A' + this.From.HorizontalLocation);
            char verticalFrom = (char)((int)'1' + (7 - this.From.VerticalLocation));
            char horizontalTo = (char)((int)'A' + this.To.HorizontalLocation);
            char verticalTo = (char)((int)'1' + (7 - this.To.VerticalLocation));

            return string.Format("{0}{1}{2}{3}", horizontalFrom, verticalFrom, horizontalTo, verticalTo);
        }
    }
}
