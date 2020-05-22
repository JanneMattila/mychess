using System.Threading.Tasks;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public interface IMeHandler
    {
        Task<Player> LoginAsync(AuthenticatedUser authenticatedUser);
    }
}
