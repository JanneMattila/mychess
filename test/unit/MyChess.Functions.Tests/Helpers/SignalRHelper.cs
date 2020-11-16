using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Moq;

namespace MyChess.Functions.Tests.Helpers
{
    public static class SignalRHelper
    {
        public static IAsyncCollector<SignalRGroupAction> Create()
        {
            var reqMock = new Mock<IAsyncCollector<SignalRGroupAction>>();
            return reqMock.Object;
        }
    }
}
