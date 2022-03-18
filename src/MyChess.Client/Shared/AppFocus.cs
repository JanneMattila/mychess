namespace MyChess.Client.Shared
{
    public static class AppFocus
    {
        public static async Task Focus()
        {
            if (OnFocus != null)
            {
                await OnFocus.Invoke();
            }
        }

        public static event Func<Task>? OnFocus;
    }
}
