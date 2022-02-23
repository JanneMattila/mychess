namespace MyChess.Client.Shared
{
    public static class AppFocus
    {
        public static void Focus()
        {
            OnFocus?.Invoke();
        }

        public static event Func<Task> OnFocus;
    }
}
