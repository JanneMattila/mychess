using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;
using MyChess.Interfaces;

namespace MyChess.Functions;

public class SettingsFunction
{
    private readonly ILogger<SettingsFunction> _log;
    private readonly ISettingsHandler _settingsHandler;
    private readonly ISecurityValidator _securityValidator;

    public SettingsFunction(
        ILogger<SettingsFunction> log,
        ISettingsHandler settingsHandler,
        ISecurityValidator securityValidator)
    {
        _log = log;
        _settingsHandler = settingsHandler;
        _securityValidator = securityValidator;
    }

    [Function("Settings")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "users/me/settings")] HttpRequestData req)
    {
        using var _ = _log.FuncSettingsScope();
        _log.FuncSettingsStarted();

        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        if (!principal.HasPermission(PermissionConstants.UserReadWrite))
        {
            _log.FuncSettingsUserDoesNotHavePermission(principal.Identity?.Name, PermissionConstants.UserReadWrite);
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        _log.FuncSettingsProcessingMethod(req.Method);
        return req.Method switch
        {
            "GET" => await Get(req, authenticatedUser),
            "POST" => await PostAsync(req, authenticatedUser),
            _ => req.CreateResponse(HttpStatusCode.NotImplemented)
        };
    }

    private async Task<HttpResponseData> Get(HttpRequestData req, AuthenticatedUser authenticatedUser)
    {
        _log.FuncMeFetchMe();
        var settings = await _settingsHandler.GetSettingsAsync(authenticatedUser);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(settings);
        return response;
    }

    private async Task<HttpResponseData> PostAsync(HttpRequestData req, AuthenticatedUser authenticatedUser)
    {
        _log.FuncGamesCreateNewGame();
        var userSettings = await JsonSerializer.DeserializeAsync<UserSettings>(req.Body);
        var result = await _settingsHandler.UpdateSettingsAsync(authenticatedUser, userSettings);
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

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(problemDetail);
            response.StatusCode = (HttpStatusCode)problemDetail.Status;
            return response;
        }
    }
}
