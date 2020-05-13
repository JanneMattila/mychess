using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Handlers;
using MyChess.Interfaces;
using MyChess.Tests.Handlers.Stubs;
using Xunit;

namespace MyChess.Tests.Handlers
{
    public class GamesHandlerTests
    {
        private readonly GamesHandler _gamesHandler;
        private readonly MyChessContextStub _context;

        public GamesHandlerTests()
        {
            _context = new MyChessContextStub();
            _gamesHandler = new GamesHandler(NullLogger<GamesHandler>.Instance, _context);
        }

        [Fact]
        public async Task New_User_No_Games()
        {
            // Arrange
            var expected = 0;
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            // Act
            var actual = await _gamesHandler.GetGamesAsync(user);

            // Assert
            Assert.Equal(expected, actual.Count);
        }
    }
}
