using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
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

        [Function("Games")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "games/{id?}")] HttpRequestData req,
            //[SignalR(HubName = "GameHub")] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            string id)
        {
            using var _ = _log.FuncGamesScope();
            _log.FuncGamesStarted();

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
            {
                _log.FuncGamesUserDoesNotHavePermission(principal.Identity?.Name, PermissionConstants.GamesReadWrite);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var authenticatedUser = principal.ToAuthenticatedUser();


            var state = "";
            var value = HttpUtility.ParseQueryString(req.Url.Query).Get("state");
            if (string.IsNullOrEmpty(value) == false)
            {
                state = value;
            }

            _log.FuncGamesProcessingMethod(req.Method);
            return req.Method switch
            {
                "GET" => await GetAsync(req, authenticatedUser, id, state /*, signalRGroupActions*/),
                "POST" => await PostAsync(req, authenticatedUser, id),
                "DELETE" => await DeleteAsync(req, authenticatedUser, id),
                _ => req.CreateResponse(HttpStatusCode.NotImplemented)
            };
        }

        private async Task<HttpResponseData> GetAsync(HttpRequestData req, AuthenticatedUser authenticatedUser, string id, string state /*, IAsyncCollector<SignalRGroupAction> signalRGroupActions*/)
        {
            if (string.IsNullOrEmpty(id))
            {
                _log.FuncGamesFetchAllGames();
                var games = await _gamesHandler.GetGamesAsync(authenticatedUser, state);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(games);
                return response;
            }
            else
            {
                _log.FuncGamesFetchSingleGame(id);
                var game = await _gamesHandler.GetGameAsync(authenticatedUser, id, state);
                if (game == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }
                //await signalRGroupActions.AddAsync(
                //    new SignalRGroupAction
                //    {
                //        UserId = authenticatedUser.UserIdentifier,
                //        GroupName = id,
                //        Action = GroupAction.Add
                //    });
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(game);
                return response;
            }
        }

        private async Task<HttpResponseData> PostAsync(HttpRequestData req, AuthenticatedUser authenticatedUser, string id)
        {
            _log.FuncGamesCreateNewGame();
            var gameToCreate = await JsonSerializer.DeserializeAsync<MyChessGame>(req.Body);
            var result = await _gamesHandler.CreateGameAsync(authenticatedUser, gameToCreate);
            if (result.Game != null)
            {
                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add(HeaderNames.Location, $"/api/games/{result.Game.ID}");
                await response.WriteAsJsonAsync(result.Game);
                return response;
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

                var response = req.CreateResponse((HttpStatusCode)problemDetail.Status);
                await response.WriteAsJsonAsync(problemDetail);
                return response;
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        private async Task<HttpResponseData> DeleteAsync(HttpRequestData req, AuthenticatedUser authenticatedUser, string id)
        {
            _log.FuncGamesDeleteGame(id);
            var result = await _gamesHandler.DeleteGameAsync(authenticatedUser, id);
            if (result == null)
            {
                return req.CreateResponse(HttpStatusCode.OK);
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

                var response = req.CreateResponse((HttpStatusCode)problemDetail.Status);
                await response.WriteAsJsonAsync(problemDetail);
                return response;
            }
        }
    }
}
