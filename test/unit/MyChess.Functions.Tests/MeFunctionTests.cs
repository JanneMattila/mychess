using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Functions.Tests.Helpers;
using MyChess.Functions.Tests.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Functions.Tests
{
    public class MeFunctionTests
    {
        private readonly MeFunction _meFunction;
        private readonly SecurityValidatorStub _securityValidatorStub;
        private readonly MeHandlerStub _meHandlerStub;

        public MeFunctionTests()
        {
            _meHandlerStub = new MeHandlerStub();
            _securityValidatorStub = new SecurityValidatorStub();
            _meFunction = new MeFunction(NullLogger<MeFunction>.Instance, _meHandlerStub, _securityValidatorStub);
        }

        [Fact]
        public async Task No_ClaimsPrincipal_Test()
        {
            // Arrange
            var expected = typeof(UnauthorizedResult);

            // Act
            var actual = await _meFunction.Run(null);

            // Assert
            Assert.IsType(expected, actual);
        }

        [Fact]
        public async Task No_Required_Permission_Test()
        {
            // Arrange
            var expected = typeof(UnauthorizedResult);
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var actual = await _meFunction.Run(null);

            // Assert
            Assert.IsType(expected, actual);
        }

        [Fact]
        public async Task Fetch_Me_Test()
        {
            // Arrange
            var expected = typeof(OkObjectResult);
            var expectedUserID = "abc";

            _meHandlerStub.User.ID = "abc";
            
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("GET");

            // Act
            var actual = await _meFunction.Run(req);

            // Assert
            Assert.IsType(expected, actual);
            var body = actual as OkObjectResult;
            var actualUser = body?.Value as Player;
            Assert.Equal(expectedUserID, actualUser?.ID);
        }
    }
}
