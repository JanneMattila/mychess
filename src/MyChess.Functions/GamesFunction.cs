using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Handlers;
using MyChess.Interfaces;
using MyChess.Internal;

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
                _log.FuncGamesUserDoesNotHavePermission(principal.Identity.Name, PermissionConstants.GamesReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            _log.FuncGamesProcessingMethod(req.Method);
            return req.Method switch
            {
                "GET" => await Get(authenticatedUser, id),
                "POST" => await PostAsync(authenticatedUser, req, id),
                "DELETE" => Delete(authenticatedUser, id),
                _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented)
            };
        }

        private async Task<IActionResult> Get(AuthenticatedUser authenticatedUser, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _log.FuncGamesFetchAllGames();
                var games = await _gamesHandler.GetGamesAsync(authenticatedUser);
                return new OkObjectResult(games);
            }
            else
            {
                _log.FuncGamesFetchSingleGame(id);
                var game = await _gamesHandler.GetGameAsync(authenticatedUser, id);
                if (game == null)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(game);
            }
        }

        private async Task<IActionResult> PostAsync(AuthenticatedUser authenticatedUser, HttpRequest req, string id)
        {
            _log.FuncGamesCreateNewGame();
            var gameToCreate = await JsonSerializer.DeserializeAsync<MyChessGame>(req.Body);
            var result = await _gamesHandler.CreateGameAsync(authenticatedUser, gameToCreate);
            if (result.Game != null)
            {
                return new CreatedResult($"/api/games/{result.Game.ID}", result.Game);
            }
            else if (result.Error != null)
            {
                var problemDetail = new ProblemDetails
                {
                    Detail = result.Error.Detail,
                    Instance = result.Error.Instance,
                    Status = result.Error.Status,
                    Title = result.Error.Title
                };

                return new ObjectResult(problemDetail)
                {
                    StatusCode = problemDetail.Status
                };
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        private IActionResult Delete(AuthenticatedUser authenticatedUser, string id)
        {
            return new OkResult();
        }
    }
}
