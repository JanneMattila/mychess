using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Functions.Tests.Helpers;
using MyChess.Functions.Tests.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Functions.Tests
{
    public class GamesFunctionTests
    {
        private readonly GamesFunction _gamesFunction;
        private readonly SecurityValidatorStub _securityValidatorStub;
        private readonly GamesHandlerStub _gamesHandlerStub;

        public GamesFunctionTests()
        {
            _gamesHandlerStub = new GamesHandlerStub();
            _securityValidatorStub = new SecurityValidatorStub();
            _gamesFunction = new GamesFunction(NullLogger<GamesFunction>.Instance, _gamesHandlerStub, _securityValidatorStub);
        }

        [Fact]
        public async Task No_ClaimsPrincipal_Test()
        {
            // Arrange
            var expected = HttpStatusCode.Unauthorized;
            var req = HttpRequestHelper.Create();

            // Act
            var actual = await _gamesFunction.Run(req, null);

            // Assert
            Assert.Equal(expected, actual.StatusCode);
        }

        [Fact]
        public async Task No_Required_Permission_Test()
        {
            // Arrange
            var expected = HttpStatusCode.Unauthorized;
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var req = HttpRequestHelper.Create();

            // Act
            var actual = await _gamesFunction.Run(req, null);

            // Assert
            Assert.Equal(expected, actual.StatusCode);
        }

        [Fact]
        public async Task Fetch_Games_Test()
        {
            // Arrange
            var expectedGames = 2;

            _gamesHandlerStub.Games.Add(new MyChessGame());
            _gamesHandlerStub.Games.Add(new MyChessGame());

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "Games.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create(query: "state=WaitingForYou");

            // Act
            var actual = await _gamesFunction.Run(req, /*SignalRHelper.Create(),*/ string.Empty);

            // Assert
            actual.Body.Position = 0;
            var list = await JsonSerializer.DeserializeAsync<List<MyChessGame>>(actual.Body);
            Assert.Equal(expectedGames, list?.Count);
        }

        [Fact]
        public async Task Fetch_Single_Game_Test()
        {
            // Arrange
            var expected = HttpStatusCode.OK;
            var expectedGameID = "abc";

            _gamesHandlerStub.SingleGame = new MyChessGame()
            {
                ID = "abc"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "Games.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create(query: "state=Archive");

            // Act
            var actual = await _gamesFunction.Run(req, /*SignalRHelper.Create(),*/ "abc");

            // Assert
            Assert.Equal(expected, actual.StatusCode);
            actual.Body.Position = 0;
            var body = await JsonSerializer.DeserializeAsync<MyChessGame>(actual.Body);
            Assert.Equal(expectedGameID, body?.ID);
        }

        [Fact]
        public async Task Create_Game_Test()
        {
            // Arrange
            var expected = HttpStatusCode.Created;
            var expectedGameID = "abc";
            var game = new MyChessGame()
            {
                Name = "great game",
                State = "Normal",
                Updated = DateTimeOffset.UtcNow
            };
            game.Players.White.ID = "p1";
            game.Players.Black.ID = "p2";
            game.Moves.Add(new MyChessGameMove()
            {
                Move = "A2A3",
                Comment = "Cool move",
                Start = DateTimeOffset.UtcNow.AddMinutes(-1),
                End = DateTimeOffset.UtcNow
            });

            _gamesHandlerStub.SingleGame = new MyChessGame()
            {
                ID = "abc"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "Games.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("POST", body: game, query: "");

            // Act
            var actual = await _gamesFunction.Run(req, /*SignalRHelper.Create(),*/ "abc");

            // Assert
            Assert.Equal(expected, actual.StatusCode);
            actual.Body.Position = 0;
            var body = await JsonSerializer.DeserializeAsync<MyChessGame>(actual.Body);
            Assert.Equal(expectedGameID, body?.ID);
        }

        [Fact]
        public async Task Create_Game_Fails_Due_Invalid_Opponent_Test()
        {
            // Arrange
            var expectedError = "1234";
            var game = new MyChessGame()
            {
                Name = "great game",
                State = "Normal",
                Updated = DateTimeOffset.UtcNow
            };
            game.Players.White.ID = "p1";
            game.Players.Black.ID = "p3";
            game.Moves.Add(new MyChessGameMove()
            {
                Move = "A2A3",
                Comment = "Cool move",
                Start = DateTimeOffset.UtcNow.AddMinutes(-1),
                End = DateTimeOffset.UtcNow
            });

            _gamesHandlerStub.Error = new HandlerError()
            {
                Instance = "some text/1234"
            };

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "Games.ReadWrite"));
            _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

            var req = HttpRequestHelper.Create("POST", body: game, query: "");

            // Act
            var actual = await _gamesFunction.Run(req, /*SignalRHelper.Create(),*/ "abc");

            // Assert
            actual.Body.Position = 0;
            var actualError = await JsonSerializer.DeserializeAsync<ProblemDetails>(actual.Body);
            Assert.EndsWith(expectedError, actualError?.Instance);
        }
    }
}
