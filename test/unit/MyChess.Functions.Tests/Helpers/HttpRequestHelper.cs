using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace MyChess.Functions.Tests.Helpers
{
    public static class HttpRequestHelper
    {
        public static HttpRequest Create(string method, 
            Dictionary<string, StringValues> query = null,
            Dictionary<string, StringValues> headers = null,
            object body = null)
        {
            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.Method).Returns(method);
            if (query != null)
            {
                reqMock.Setup(req => req.Query).Returns(new QueryCollection(query));
            }

            if (headers != null)
            {
                reqMock.Setup(req => req.Headers).Returns(new HeaderDictionary(headers));
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
}
