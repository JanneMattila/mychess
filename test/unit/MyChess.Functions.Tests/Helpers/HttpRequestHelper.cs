using System.IO;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;

namespace MyChess.Functions.Tests.Helpers;

public static class HttpRequestHelper
{
    public static HttpRequestData Create(string method,
        string? query = null,
        HttpHeadersCollection? headers = null,
        object? body = null)
    {
        var reqMock = new Mock<HttpRequestData>();
        reqMock.Setup(req => req.Method).Returns(method);
        if (query != null)
        {
            reqMock.Setup(req => req.Url).Returns(new System.Uri($"http://localhost/{query}"));
        }

        if (headers != null)
        {
            reqMock.Setup(req => req.Headers).Returns(headers);
        }

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);

            // Note: Do not dispose these two since
            // they are read during the test execution.
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            reqMock.Setup(req => req.Body).Returns(stream);
        }

        return reqMock.Object;
    }
}
