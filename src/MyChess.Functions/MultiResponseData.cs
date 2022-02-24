using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions;

public class MultiResponseData
{
    [SignalROutput(HubName = "GameHub")]
    public List<object> Notifications { get; set; } = new List<object>();

    public HttpResponseData Response { get; set; }
}
