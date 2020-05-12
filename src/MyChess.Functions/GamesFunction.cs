using System.Net;
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
        private readonly ILogger<GamesFunction> _log;
        private readonly IGamesHandler _gamesHandler;
        private readonly ISecurityValidator _securityValidator;

        public GamesFunction(
            ILogger<GamesFunction> log,
            IGamesHandler gamesHandler,
            ISecurityValidator securityValidator)
        {
            _log = log;
            _gamesHandler = gamesHandler;
            _securityValidator = securityValidator;
        }

        [FunctionName("Games")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "games/{id?}")] HttpRequest req,
            string id)
        {
            using var _ = _log.FuncGamesScope();
            _log.FuncGamesStarted();

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
            {
                _log.LogWarning(LoggingEvents.FuncGamesUserDoesNotHavePermission,
                    "User {user} does not have permission {permission}", principal.Identity.Name, PermissionConstants.GamesReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            _log.LogInformation(LoggingEvents.FuncGamesProcessingMethod, 
                "Processing {method} request", req.Method);
            return req.Method switch
            {
                "GET" => await Get(_log, authenticatedUser, id),
                "POST" => Post(_log, authenticatedUser, req, id),
                "DELETE" => Delete(_log, authenticatedUser, id),
                _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented)
            };
        }

        private async Task<IActionResult> Get(ILogger log, AuthenticatedUser authenticatedUser, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                log.LogTrace(LoggingEvents.FuncGamesFetchAllGames, "Fetch all games");
                var games = await _gamesHandler.GetGamesAsync(authenticatedUser);
                return new OkObjectResult(games);
            }
            else
            {
                log.LogTrace(LoggingEvents.FuncGamesFetchSingleGame,
                    "Fetch single game {gameID}", id);
                var game = await _gamesHandler.GetGameAsync(authenticatedUser, id);
                if (game == null)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(game);
            }
        }

        private IActionResult Post(ILogger log, AuthenticatedUser authenticatedUser, HttpRequest req, string id)
        {
            return new OkObjectResult($"create game {id}");
        }

        private IActionResult Delete(ILogger log, AuthenticatedUser authenticatedUser, string id)
        {
            return new OkResult();
        }
    }
}
