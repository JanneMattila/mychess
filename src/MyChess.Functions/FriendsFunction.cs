using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;
using MyChess.Interfaces;

namespace MyChess.Functions;

public class FriendsFunction
{
    private readonly ILogger<FriendsFunction> _log;
    private readonly IFriendsHandler _friendsHandler;
    private readonly ISecurityValidator _securityValidator;

    public FriendsFunction(
        ILogger<FriendsFunction> log,
        IFriendsHandler friendHandler,
        ISecurityValidator securityValidator)
    {
        _log = log;
        _friendsHandler = friendHandler;
        _securityValidator = securityValidator;
    }

    [Function("Friends")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "users/me/friends/{id?}")] HttpRequestData req,
        string id)
    {
        using var _ = _log.FuncFriendsScope();
        _log.FuncFriendsStarted();

        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        if (!principal.HasPermission(PermissionConstants.UserReadWrite))
        {
            _log.FuncFriendsUserDoesNotHavePermission(principal.Identity?.Name, PermissionConstants.UserReadWrite);
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        _log.FuncFriendsProcessingMethod(req.Method);
        return req.Method switch
        {
            "GET" => await Get(req, authenticatedUser, id),
            "POST" => await PostAsync(req, authenticatedUser, id),
            "DELETE" => Delete(req, authenticatedUser, id),
            _ => req.CreateResponse(HttpStatusCode.NotImplemented)
        };
    }

    private async Task<HttpResponseData> Get(HttpRequestData req, AuthenticatedUser authenticatedUser, string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            _log.FuncFriendsFetchAllFriends();
            var friends = await _friendsHandler.GetFriendsAsync(authenticatedUser);
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(friends);
            return response;
        }
        else
        {
            _log.FuncFriendsFetchSingleFriend(id);
            var friend = await _friendsHandler.GetFriendAsync(authenticatedUser, id);
            if (friend == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(friend);
            return response;
        }
    }

    private async Task<HttpResponseData> PostAsync(HttpRequestData req, AuthenticatedUser authenticatedUser, string id)
    {
        _log.FuncFriendsAddNewFriend();
        var friendToAdd = await JsonSerializer.DeserializeAsync<User>(req.Body);
        ArgumentNullException.ThrowIfNull(friendToAdd);

        var result = await _friendsHandler.AddNewFriend(authenticatedUser, friendToAdd);
        if (result.Friend != null)
        {
            var response = req.CreateResponse();
            response.Headers.Add(HeaderNames.Location, $"/api/friend/{result.Friend.ID}");
            await response.WriteAsJsonAsync(result.Friend);
            response.StatusCode = HttpStatusCode.Created;
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

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(problemDetail);
            response.StatusCode = (HttpStatusCode)problemDetail.Status;
            return response;
        }
        else
        {
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    private HttpResponseData Delete(HttpRequestData req, AuthenticatedUser authenticatedUser, string id)
    {
        return req.CreateResponse(HttpStatusCode.OK);
    }
}
