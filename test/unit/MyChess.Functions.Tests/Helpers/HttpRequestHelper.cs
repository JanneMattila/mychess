using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MyChess.Functions.Tests.Helpers;

public static class HttpRequestHelper
{
    public static HttpRequestData Create(string method = "GET",
        string? query = null,
        HttpHeadersCollection? headers = null,
        object? body = null)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
        serviceCollection.AddScoped<JsonObjectSerializer>();
        // Ref: https://github.com/Azure/azure-functions-host/issues/5469
        serviceCollection.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
        {
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            jsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;

            // override the default value
            jsonSerializerOptions.PropertyNameCaseInsensitive = false;
        });
        serviceCollection.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var context = new Mock<FunctionContext>();
        context.SetupProperty(c => c.InstanceServices, serviceProvider);

        var request = new FakeHttpRequestData(
            context.Object,
            method,
            $"http://localhost/{query}",
            headers,
            body);
        return request;
    }
}
