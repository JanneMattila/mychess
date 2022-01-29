using System.Text.Json;
using Microsoft.JSInterop;

namespace MyChess.Client.Extensions;

public class LocalStorage
{
    private readonly IJSRuntime _js;

    public LocalStorage(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<T?> Get<T>(string key)
    {
        var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task Set(string key, object value)
    {
        var json = JsonSerializer.Serialize(value);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task Delete(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
