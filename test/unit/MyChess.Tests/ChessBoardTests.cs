using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MyChess.Tests;

public class ChessBoardTests
{
    private readonly ChessBoard _board;

    public ChessBoardTests()
    {
        _board = new ChessBoard(NullLogger<ChessBoard>.Instance);
        _board.Initialize();
    }

    [Fact]
    public void AvailableMoves_In_Start_Test()
    {
        // Arrange
        var expected = 20;

        // Act
        var actual = _board.GetAllAvailableMoves().Length;

        // Assert
        Assert.Equal(expected, actual);
    }
}
