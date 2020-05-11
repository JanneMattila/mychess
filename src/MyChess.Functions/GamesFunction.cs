using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions
{
    public class GamesFunction
    {
        private readonly GamesHandler _gamesHandler;
        private readonly ISecurityValidator _securityValidator;

        public GamesFunction(
            GamesHandler gamesHandler,
            ISecurityValidator securityValidator)
        {
            _gamesHandler = gamesHandler;
            _securityValidator = securityValidator;
        }

        [FunctionName("Games")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using var scope = log.BeginScope("Games");
            log.LogInformation(LoggingEvents.FuncGamesStarted, "Games function processing request.");

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req, log);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
            {
                log.LogWarning(LoggingEvents.FuncGamesUserDoesNotHavePermission,
                    "User {user} does not have permission {permission}", principal.Identity.Name, PermissionConstants.GamesReadWrite);
                return new UnauthorizedResult();
            }

            var user = await _gamesHandler.GetOrCreateUserAsync(principal.ToAuthenticatedUser());

            var games = new List<MyChessGame>();
            for (int i = 0; i < 5; i++)
            {
                var game = new MyChessGame()
                {
                    ID = Guid.NewGuid().ToString("D"),
                    Name = $"Game of name {i + 1}",
                    Updated = DateTimeOffset.UtcNow.AddHours(-i)
                };
                game.Players.Black.Name = $"User {i + 1}";
                game.Moves.Add(new MyChessGameMove()
                {
                    Comment = "👍 Lorem ipsum dolor sit amet ❤ 😊, consectetur adipiscing elit. Ut sed mollis neque. Maecenas molestie nibh id elit gravida, quis placerat magna tempor. Morbi posuere orci sapien, eget dictum ligula tempor ut. Vivamus nec massa dolor. Sed fermentum ex non nunc dapibus blandit. Vivamus sollicitudin, libero rhoncus faucibus ullamcorper, velit dui finibus neque, sed placerat sapien urna id nunc. Aliquam ac consectetur elit.",
                    Move = "A2A4",
                    Start = DateTimeOffset.UtcNow.AddHours(-i),
                    End = DateTimeOffset.UtcNow,
                });
                games.Add(game);
            }

            return new OkObjectResult(games);
        }
    }
}
