using Microsoft.AspNetCore.Authorization;
using MyChess.Client.Shared;

namespace MyChess.Client.Pages;

[Authorize]
public class SettingsBase : MyChessComponentBase
{
}
