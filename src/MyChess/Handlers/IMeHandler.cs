using System.Threading.Tasks;
using MyChess.Interfaces;

namespace MyChess.Handlers
{
    public interface IMeHandler
    {
        Task<string> LoginAsync(AuthenticatedUser authenticatedUser);
    }
}
