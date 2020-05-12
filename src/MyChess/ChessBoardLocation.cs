using System;

namespace MyChess
{
    public class ChessBoardLocation : IComparable
    {
        public static readonly int OUTSIDE_BOARD = -1;

        public int HorizontalLocation { get; private set; }

        public int VerticalLocation { get; private set; }

        public static ChessBoardLocation OutsideBoard
        {
            get
            {
                return new ChessBoardLocation(OUTSIDE_BOARD, OUTSIDE_BOARD);
            }
        }

        public ChessBoardLocation(int horizontalLocation, int verticalLocation)
        {
            HorizontalLocation = horizontalLocation;
            VerticalLocation = verticalLocation;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is ChessBoardLocation otherLocation)
            {
                int compare = HorizontalLocation.CompareTo(otherLocation.HorizontalLocation);
                if (compare == 0)
                {
                    return VerticalLocation.CompareTo(otherLocation.VerticalLocation);
                }

                return compare;
            }
            else
            {
                throw new ArgumentException("Object is not a ChessBoardLocation");
            }
        }
    }
}
