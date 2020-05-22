using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions.Tests.Stubs
{
    public class MeHandlerStub : IMeHandler
    {
        public Player User { get; set; } = new Player();

        public async Task<Player> LoginAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(User);
        }
    }
}
