namespace Gymaui_App.Services
{
    public interface INavigationService
    {
        event Action<int>? OnNavigateToTab;
        void NavigateToTab(int tabIndex);
    }

    public class NavigationService : INavigationService
    {
        public event Action<int>? OnNavigateToTab;

        public void NavigateToTab(int tabIndex)
        {
            OnNavigateToTab?.Invoke(tabIndex);
        }
    }
}
