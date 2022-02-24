using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend;
using MyChess.Backend.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions;

public class GamesMoveFunction
{
    private readonly IGamesHandler _gamesHandler;
    private readonly ISecurityValidator _securityValidator;

    public GamesMoveFunction(
        IGamesHandler gamesHandler,
        ISecurityValidator securityValidator)
    {
        _gamesHandler = gamesHandler;
        _securityValidator = securityValidator;
    }

    [Function("GamesMove")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "games/{id}/moves")] HttpRequestData req,
        //[SignalR(HubName = "GameHub")] IAsyncCollector<SignalRMessage> signalRMessages,
        string id,
        ILogger log)
    {
        using var scope = log.BeginScope("GamesMove");
        log.LogInformation(LoggingEvents.FuncGamesMoveStarted, "GamesMove function processing request.");

        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
        {
            log.LogWarning(LoggingEvents.FuncGamesMoveUserDoesNotHavePermission,
                "User {user} does not have permission {permission}", principal.Identity?.Name, PermissionConstants.GamesReadWrite);
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        log.LogInformation(LoggingEvents.FuncGamesMoveProcessingMethod,
            "Processing {method} request", req.Method);

        var moveToAdd = await JsonSerializer.DeserializeAsync<MyChessGameMove>(req.Body);
        var error = await _gamesHandler.AddMoveAsync(authenticatedUser, id, moveToAdd);
        if (error == null)
        {
            //var payload = JsonSerializer.Serialize(moveToAdd);
            //await signalRMessages.AddAsync(new SignalRMessage()
            //{
            //    GroupName = id,
            //    Target = "MoveUpdate",
            //    Arguments = new[] { id, payload }
            //});
            return req.CreateResponse(HttpStatusCode.OK);
        }
        else
        {
            var problemDetail = new ProblemDetails
            {
                Detail = error.Detail,
                Instance = error.Instance,
                Status = error.Status,
                Title = error.Title
            };

            var response = req.CreateResponse((HttpStatusCode)problemDetail.Status);
            await response.WriteAsJsonAsync(problemDetail);
            return response;
        }
    }
}
