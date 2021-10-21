using Microsoft.AspNetCore.Components;
using MyChess.Client.Shared;

namespace MyChess.Client.Pages;

public class PlayBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; }
}
