using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;
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
            [SignalR(HubName = "GameHub")] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
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


            var state = "";
            if (req.Query.ContainsKey("state"))
            {
                state = req.Query["state"];
            }

            _log.FuncGamesProcessingMethod(req.Method);
            return req.Method switch
            {
                "GET" => await GetAsync(authenticatedUser, id, state, signalRGroupActions),
                "POST" => await PostAsync(authenticatedUser, req, id),
                "DELETE" => await DeleteAsync(authenticatedUser, id),
                _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented)
            };
        }

        private async Task<IActionResult> GetAsync(AuthenticatedUser authenticatedUser, string id, string state, IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            if (string.IsNullOrEmpty(id))
            {
                _log.FuncGamesFetchAllGames();
                var games = await _gamesHandler.GetGamesAsync(authenticatedUser, state);
                return new OkObjectResult(games);
            }
            else
            {
                _log.FuncGamesFetchSingleGame(id);
                var game = await _gamesHandler.GetGameAsync(authenticatedUser, id, state);
                if (game == null)
                {
                    return new NotFoundResult();
                }
                await signalRGroupActions.AddAsync(
                    new SignalRGroupAction
                    {
                        UserId = authenticatedUser.UserIdentifier,
                        GroupName = id,
                        Action = GroupAction.Add
                    });
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

        private async Task<IActionResult> DeleteAsync(AuthenticatedUser authenticatedUser, string id)
        {
            _log.FuncGamesDeleteGame(id);
            var result = await _gamesHandler.DeleteGameAsync(authenticatedUser, id);
            if (result == null)
            {
                return new OkResult();
            }
            else
            {
                var problemDetail = new ProblemDetails
                {
                    Detail = result.Detail,
                    Instance = result.Instance,
                    Status = result.Status,
                    Title = result.Title
                };

                return new ObjectResult(problemDetail)
                {
                    StatusCode = problemDetail.Status
                };
            }
        }
    }
}
