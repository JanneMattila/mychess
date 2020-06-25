using System.Threading.Tasks;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public interface IMeHandler
    {
        Task<User> LoginAsync(AuthenticatedUser authenticatedUser);
    }
}
