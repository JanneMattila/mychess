using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions.Tests.Helpers;

public class FakeHttpRequestData : HttpRequestData
{
    public override Stream Body { get; }

    public override HttpHeadersCollection Headers { get; }

    public override IReadOnlyCollection<IHttpCookie> Cookies => new List<IHttpCookie>();

    public override Uri Url { get; }

    public override IEnumerable<ClaimsIdentity> Identities => new List<ClaimsIdentity>();

    public override string Method { get; }

    public FakeHttpRequestData(
        FunctionContext functionContext,
        string method,
        string url,
        HttpHeadersCollection? headers = null,
        object? body = null) : base(functionContext)
    {
        Method = method;
        Url = new Uri(url);
        Headers = headers ?? new HttpHeadersCollection();

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            var buffer = Encoding.UTF8.GetBytes(json);

            Body = new MemoryStream(buffer);
        }
        else
        {
            Body = new MemoryStream();
        }
    }

    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }
}
