using System;

namespace MyChess;

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

    // Must match 'int ChessMove.CompareTo(object obj)' implicitly implemented member 'int IComparable.CompareTo(object? obj)'
#nullable disable
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
#nullable enable

    public override string ToString()
    {
        char horizontalFrom = (char)((int)'A' + this.From.Column);
        char verticalFrom = (char)((int)'1' + (7 - this.From.Row));
        char horizontalTo = (char)((int)'A' + this.To.Column);
        char verticalTo = (char)((int)'1' + (7 - this.To.Row));

        return string.Format("{0}{1}{2}{3}", horizontalFrom, verticalFrom, horizontalTo, verticalTo);
    }
}
