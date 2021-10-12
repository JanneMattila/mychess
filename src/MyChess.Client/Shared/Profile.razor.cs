using Microsoft.AspNetCore.Authorization;

namespace MyChess.Client.Shared;

[Authorize]
public class ProfileBase : MyChessComponentBase
{
}
