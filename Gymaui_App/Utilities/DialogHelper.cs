namespace Gymaui_App.Utilities
{
    public static class DialogHelper
    {
        [Obsolete]
        public static async Task DisplayAlert(string title, string message, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }

        [Obsolete]
        public static async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
    }
}
