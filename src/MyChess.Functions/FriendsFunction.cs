using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;
using MyChess.Interfaces;

namespace MyChess.Functions
{
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

        [FunctionName("Friends")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "users/me/friends/{id?}")] HttpRequest req,
            string id)
        {
            using var _ = _log.FuncFriendsScope();
            _log.FuncFriendsStarted();

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.UserReadWrite))
            {
                _log.FuncFriendsUserDoesNotHavePermission(principal.Identity.Name, PermissionConstants.UserReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            _log.FuncFriendsProcessingMethod(req.Method);
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
                _log.FuncFriendsFetchAllFriends();
                var friends = await _friendsHandler.GetFriendsAsync(authenticatedUser);
                return new OkObjectResult(friends);
            }
            else
            {
                _log.FuncFriendsFetchSingleFriend(id);
                var friend = await _friendsHandler.GetFriendAsync(authenticatedUser, id);
                if (friend == null)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(friend);
            }
        }

        private async Task<IActionResult> PostAsync(AuthenticatedUser authenticatedUser, HttpRequest req, string id)
        {
            _log.FuncFriendsAddNewFriend();
            var friendToAdd = await JsonSerializer.DeserializeAsync<User>(req.Body);
            var result = await _friendsHandler.AddNewFriend(authenticatedUser, friendToAdd);
            if (result.Friend != null)
            {
                return new CreatedResult($"/api/friend/{result.Friend.ID}", result.Friend);
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
