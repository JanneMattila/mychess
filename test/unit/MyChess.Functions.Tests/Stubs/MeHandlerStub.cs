using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions.Tests.Stubs
{
    public class MeHandlerStub : IMeHandler
    {
        public User User { get; set; } = new User();

        public async Task<User> LoginAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(User);
        }
    }
}
