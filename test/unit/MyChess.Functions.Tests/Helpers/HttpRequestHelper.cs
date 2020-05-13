using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace MyChess.Functions.Tests.Helpers
{
    public static class HttpRequestHelper
    {
        public static HttpRequest Create(string method, Dictionary<string, StringValues> query = null, string body = null)
        {
            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.Method).Returns(method);

            if (query != null)
            {
                reqMock.Setup(req => req.Query).Returns(new QueryCollection(query));
            }

            if (body != null)
            {
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream);
                writer.Write(body);
                writer.Flush();
                stream.Position = 0;
                reqMock.Setup(req => req.Body).Returns(stream);
            }

            return reqMock.Object;
        }
    }
}
