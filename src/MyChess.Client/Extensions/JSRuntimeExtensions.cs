using Microsoft.JSInterop;

namespace MyChess.Client.Extensions;

public static class JSRuntimeExtensions
{
    public static LocalStorage GetLocalStorage(this IJSRuntime js)
    {
        ArgumentNullException.ThrowIfNull(js);
        return new LocalStorage(js);
    }
}
