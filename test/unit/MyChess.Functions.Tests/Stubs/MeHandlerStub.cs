using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions.Tests.Stubs
{
    public class MeHandlerStub : IMeHandler
    {
        public string UserID { get; set; } = string.Empty;

        public async Task<string> LoginAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(UserID);
        }
    }
}
