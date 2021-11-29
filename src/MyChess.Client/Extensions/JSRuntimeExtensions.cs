using Microsoft.JSInterop;

namespace MyChess.Client.Extensions;

public static class JSRuntimeExtensions
{
    public static LocalStorage GetLocalStorage(this IJSRuntime js)
    {
        ArgumentNullException.ThrowIfNull(js);
        return new LocalStorage(js);
    }

    public static async Task<bool> Confirm(this IJSRuntime js, string text)
    {
        ArgumentNullException.ThrowIfNull(js);
        return await js.InvokeAsync<bool>("confirm", text);
    }
}
