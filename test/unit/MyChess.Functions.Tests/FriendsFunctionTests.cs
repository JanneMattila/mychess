using System;
using System.Collections.Generic;
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
    public class FriendsFunctionTests
    {
        private readonly FriendsFunction _friendsFunction;
        private readonly SecurityValidatorStub _securityValidatorStub;
        private readonly FriendsHandlerStub _friendsHandlerStub;

        public FriendsFunctionTests()
        {
            _friendsHandlerStub = new FriendsHandlerStub();
            _securityValidatorStub = new SecurityValidatorStub();
            _friendsFunction = new FriendsFunction(NullLogger<FriendsFunction>.Instance, _friendsHandlerStub, _securityValidatorStub);
        }

        [Fact]
        public async Task No_ClaimsPrincipal_Test()
        {
            // Arrange
            var expected = typeof(UnauthorizedResult);

            // Act
            var actual = await _friendsFunction.Run(null, null);

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
            var actual = await _friendsFunction.Run(null, null);

            // Assert
            Assert.IsType(expected, actual);
        }

        [Fact]
        public async Task Fetch_Players_Test()
        {
            // Arrange
            var expected = typeof(OkObjectResult);
            var expectedFriends = 2;
            
            _friendsHandlerStub.Friends.Add(new Player());
            _friendsHandlerStub.Friends.Add(new Player());
            
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("GET");

            // Act
            var actual = await _friendsFunction.Run(req, null);

            // Assert
            Assert.IsType(expected, actual);
            var body = actual as OkObjectResult;
            var list = body?.Value as List<Player>;
            Assert.Equal(expectedFriends, list?.Count);
        }

        [Fact]
        public async Task Fetch_Single_friend_Test()
        {
            // Arrange
            var expected = typeof(OkObjectResult);
            var expectedPlayerID = "abc";

            _friendsHandlerStub.SingleFriend = new Player()
            {
                ID = "abc"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("GET");

            // Act
            var actual = await _friendsFunction.Run(req, "abc");

            // Assert
            Assert.IsType(expected, actual);
            var body = actual as OkObjectResult;
            var actualPlayer = body?.Value as Player;
            Assert.Equal(expectedPlayerID, actualPlayer?.ID);
        }

        [Fact]
        public async Task Add_Friend_Test()
        {
            // Arrange
            var expected = typeof(CreatedResult);
            var expectedPlayerID = "abc";
            var friend = new Player()
            {
                ID = "abc",
                Name = "John Doe"
            };

            _friendsHandlerStub.SingleFriend = new Player()
            {
                ID = "abc"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("POST", body: friend);

            // Act
            var actual = await _friendsFunction.Run(req, "abc");

            // Assert
            Assert.IsType(expected, actual);
            var body = actual as CreatedResult;
            var actualPlayer = body?.Value as Player;
            Assert.Equal(expectedPlayerID, actualPlayer?.ID);
        }

        [Fact]
        public async Task Add_Friend_Fails_Due_Invalid_Player_Test()
        {
            // Arrange
            var expected = typeof(ObjectResult);
            var expectedError = "1234";
            var friend = new Player()
            {
                ID = "abc",
                Name = "John Doe"
            };

            _friendsHandlerStub.Error = new Models.HandlerError()
            {
                Instance = "some text/1234"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("POST", body: friend);

            // Act
            var actual = await _friendsFunction.Run(req, "abc");

            // Assert
            Assert.IsType(expected, actual);
            var body = actual as ObjectResult;
            var actualError = body?.Value as ProblemDetails;
            Assert.EndsWith(expectedError, actualError?.Instance);
        }
    }
}
