using System;
using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions.Tests.Helpers;

public class FakeHttpResponseData : HttpResponseData
{
    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
    public override Stream Body { get; set; } = new MemoryStream();

    public override HttpCookies Cookies => throw new NotImplementedException();

    public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
    }
}
