using System.Net;
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

        [FunctionName("Me")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "delete", Route = "users/me")] HttpRequest req)
        {
            using var _ = _log.FuncMeScope();
            _log.FuncMeStarted();

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.UserReadWrite))
            {
                _log.FuncMeUserDoesNotHavePermission(principal.Identity.Name, PermissionConstants.UserReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            _log.FuncMeProcessingMethod(req.Method);
            return req.Method switch
            {
                "GET" => await Get(authenticatedUser),
                "POST" => PostAsync(authenticatedUser, req),
                "DELETE" => Delete(authenticatedUser),
                _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented)
            };
        }

        private async Task<IActionResult> Get(AuthenticatedUser authenticatedUser)
        {
            _log.FuncMeFetchMe();
            var Me = await _meHandler.LoginAsync(authenticatedUser);
            return new OkObjectResult(Me);
        }

        private IActionResult PostAsync(AuthenticatedUser authenticatedUser, HttpRequest req)
        {
            return new OkResult();
        }

        private IActionResult Delete(AuthenticatedUser authenticatedUser)
        {
            return new OkResult();
        }
    }
}
