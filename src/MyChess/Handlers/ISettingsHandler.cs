using System.Threading.Tasks;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public interface ISettingsHandler
    {
        Task<PlayerSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser);
        Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, PlayerSettings playerSettings);
    }
}
