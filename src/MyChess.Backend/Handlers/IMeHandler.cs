using System.Threading.Tasks;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Backend.Handlers
{
    public interface IMeHandler
    {
        Task<User> LoginAsync(AuthenticatedUser authenticatedUser);
    }
}
