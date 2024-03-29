﻿using System;

namespace MyChess;

public class ChessBoardLocation : IComparable
{
    public static readonly int OUTSIDE_BOARD = -1;

    public int Column { get; private set; }

    public int Row { get; private set; }

    public static ChessBoardLocation OutsideBoard
    {
        get
        {
            return new ChessBoardLocation(OUTSIDE_BOARD, OUTSIDE_BOARD);
        }
    }

    public ChessBoardLocation(int horizontalLocation, int verticalLocation)
    {
        Column = horizontalLocation;
        Row = verticalLocation;
    }

    // Must match 'int ChessBoardLocation.CompareTo(object obj)' implicitly implemented member 'int IComparable.CompareTo(object? obj)'
#nullable disable
    public int CompareTo(object obj)
    {
        if (obj == null)
        {
            return 1;
        }

        if (obj is ChessBoardLocation otherLocation)
        {
            int compare = Column.CompareTo(otherLocation.Column);
            if (compare == 0)
            {
                return Row.CompareTo(otherLocation.Row);
            }

            return compare;
        }
        else
        {
            throw new ArgumentException("Object is not a ChessBoardLocation");
        }
    }
#nullable enable
}
