using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;

namespace MyChess.Functions;

public class MeFunction
{
    private readonly ILogger<MeFunction> _log;
    private readonly IMeHandler _meHandler;
    private readonly ISecurityValidator _securityValidator;

    public MeFunction(
        ILogger<MeFunction> log,
        IMeHandler meHandler,
        ISecurityValidator securityValidator)
    {
        _log = log;
        _meHandler = meHandler;
        _securityValidator = securityValidator;
    }

    [Function("Me")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "users/me")] HttpRequestData req)
    {
        using var _ = _log.FuncMeScope();
        _log.FuncMeStarted();

        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        if (!principal.HasPermission(PermissionConstants.UserReadWrite))
        {
            _log.FuncMeUserDoesNotHavePermission(principal.Identity?.Name, PermissionConstants.UserReadWrite);
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        _log.FuncMeProcessingMethod(req.Method);
        return req.Method switch
        {
            "GET" => await Get(req, authenticatedUser),
            "POST" => PostAsync(req, authenticatedUser),
            "DELETE" => Delete(req, authenticatedUser),
            _ => req.CreateResponse(HttpStatusCode.NotImplemented)
        };
    }

    private async Task<HttpResponseData> Get(HttpRequestData req, AuthenticatedUser authenticatedUser)
    {
        _log.FuncMeFetchMe();
        var me = await _meHandler.LoginAsync(authenticatedUser);
        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(me);
        return response;
    }

    private HttpResponseData PostAsync(HttpRequestData req, AuthenticatedUser authenticatedUser)
    {
        return req.CreateResponse(HttpStatusCode.OK);
    }

    private HttpResponseData Delete(HttpRequestData req, AuthenticatedUser authenticatedUser)
    {
        return req.CreateResponse(HttpStatusCode.OK);
    }
}
