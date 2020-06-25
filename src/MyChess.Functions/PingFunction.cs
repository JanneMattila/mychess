using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MyChess.Functions
{
    public class PingFunction
    {
        private readonly ISecurityValidator _securityValidator;

        public PingFunction(ISecurityValidator securityValidator)
        {
            _securityValidator = securityValidator;
        }

        [FunctionName("Ping")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequest req)
        {
            await _securityValidator.InitializeAsync();
            return new OkResult();
        }
    }
}
