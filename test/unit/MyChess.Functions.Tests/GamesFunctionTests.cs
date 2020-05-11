using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Functions.Tests.Stubs;
using Xunit;

namespace MyChess.Functions.Tests
{
    public class GamesFunctionTests
    {
        private readonly GamesFunction _gamesFunction;
        private readonly SecurityValidatorStub _securityValidatorStub;

        public GamesFunctionTests()
        {
            _securityValidatorStub = new SecurityValidatorStub();
            _gamesFunction = new GamesFunction(null, _securityValidatorStub);
        }

        [Fact]
        public async Task No_Claims_Test()
        {
            // Arrange
            var expected = typeof(UnauthorizedResult);

            // Act
            var actual = await _gamesFunction.Run(null, null, NullLogger.Instance);

            // Assert
            Assert.IsType(expected, actual);
        }
    }
}
