using Gymaui_App.Controls;

namespace Gymaui_App.Utilities
{
    /// <summary>
    /// Helper class to wire up CustomHeader events consistently across all pages
    /// </summary>
    public static class HeaderEventHelper
    {
        /// <summary>
        /// Sets up the header events for a content page
        /// </summary>
        public static void SetupHeaderEvents(ContentPage page)
        {
            var header = page.FindByName<CustomHeader>("CustomHeader");
            if (header != null)
            {
                header.SettingsClicked -= OnHeaderSettingsClicked;
                header.SearchTextChanged -= OnHeaderSearchTextChanged;
                header.SearchPressed -= OnHeaderSearchPressed;

                header.SettingsClicked += OnHeaderSettingsClicked;
                header.SearchTextChanged += OnHeaderSearchTextChanged;
                header.SearchPressed += OnHeaderSearchPressed;
            }
        }

        private static async void OnHeaderSettingsClicked(object sender, EventArgs e)
        {
            var page = GetCurrentPage();
            if (page == null) return;

            var action = await page.DisplayActionSheet(
                "Settings",
                "Cancel",
                null,
                "View Statistics",
                "My Training Plans",
                "Settings & Preferences");

            switch (action)
            {
                case "View Statistics":
                    await Shell.Current.GoToAsync("///stats");
                    break;
                case "My Training Plans":
                    await Shell.Current.GoToAsync("///plans");
                    break;
                case "Settings & Preferences":
                    await Shell.Current.GoToAsync("SettingsPage");
                    break;
            }
        }

        private static void OnHeaderSearchTextChanged(object sender, string searchText)
        {
            // You can use this to filter or perform live search
            System.Diagnostics.Debug.WriteLine($"Search text changed: {searchText}");
        }

        private static async void OnHeaderSearchPressed(object sender, string searchText)
        {
            var page = GetCurrentPage();
            if (page == null) return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await page.DisplayAlert("Search", "Please enter a search term.", "OK");
                return;
            }

            // Navigate to exercise search or implement search functionality
            await page.DisplayAlert("Search", $"Searching for: {searchText}", "OK");
        }

        private static ContentPage? GetCurrentPage()
        {
            if (Shell.Current?.CurrentPage is ContentPage page)
            {
                return page;
            }
            return null;
        }
    }
}
