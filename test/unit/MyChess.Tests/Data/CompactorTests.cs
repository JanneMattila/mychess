using MyChess.Data;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Tests.Data
{
    public class CompactorTests
    {
        private readonly Compactor _compactor;

        public CompactorTests()
        {
            _compactor = new Compactor();
        }

        [Fact]
        public void Compress_Decompress_Test()
        {
            // Arrange
            var expected = new MyChessGame
            {
                ID = "abc"
            };
            expected.Players.Black.ID = "def";
            var buffer = _compactor.Compact(expected);

            // Act
            var actual = _compactor.Decompress(buffer);

            // Assert
            Assert.Equal(expected.ID, actual.ID);
            Assert.Equal(expected.Players.Black.ID, actual.Players.Black.ID);
        }
    }
}
