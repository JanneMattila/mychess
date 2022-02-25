using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Functions.Internal;
using MyChess.Interfaces;

namespace MyChess.Functions;

public class GamesMoveFunction
{
    private readonly ILogger<GamesMoveFunction> _log;
    private readonly IGamesHandler _gamesHandler;
    private readonly ISecurityValidator _securityValidator;

    public GamesMoveFunction(
        ILogger<GamesMoveFunction> log,
        IGamesHandler gamesHandler,
        ISecurityValidator securityValidator)
    {
        _log = log;
        _gamesHandler = gamesHandler;
        _securityValidator = securityValidator;
    }

    [Function("GamesMove")]
    //[SignalROutput(HubName = "GameHub")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "games/{id}/moves")] HttpRequestData req,
        // Use following references to migrate to isolated worker:
        // https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#multiple-output-bindings
        // https://github.com/aspnet/AzureSignalR-samples/blob/main/samples/DotnetIsolated-BidirectionChat/Functions.cs
        // https://github.com/Azure/azure-functions-dotnet-worker/issues/294
        //[Microsoft.Azure.Functions.Worker.(HubName = "GameHub")] IAsyncCollector<SignalRMessage> signalRMessages,
        string id)
    {
        using var _ = _log.FuncGamesMoveScope();

        _log.FuncGamesMoveStarted();

        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
        {
            _log.FuncGamesMoveUserDoesNotHavePermission(principal.Identity?.Name, PermissionConstants.GamesReadWrite);
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        _log.FuncGamesMoveProcessingMethod(req.Method);

        var moveToAdd = await JsonSerializer.DeserializeAsync<MyChessGameMove>(req.Body);
        ArgumentNullException.ThrowIfNull(moveToAdd);

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
