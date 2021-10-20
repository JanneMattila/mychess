namespace MyChess.Client.Shared
{
    public class AppState
    {
        private bool _isLoading = false;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;

                OnLoadingChange?.Invoke();
            }
        }

        public event Action OnLoadingChange;
    }
}
