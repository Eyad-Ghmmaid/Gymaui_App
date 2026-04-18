namespace Gymaui_App.Services
{
    public enum AppThemeMode
    {
        Dark,
        Light
    }

    public class ThemeService
    {
        private const string ThemePreferenceKey = "app_theme";

        public AppThemeMode CurrentTheme { get; private set; }

        // Dark theme colors (current defaults)
        private static readonly Dictionary<string, string> DarkColors = new()
        {
            { "BackgroundDark", "#0D0D0D" },
            { "Surface", "#1A1A1A" },
            { "Surface2", "#242424" },
            { "TextPrimary", "#FFFFFF" },
            { "TextSecondary", "#8A8A8A" },
            { "PrimaryAccent", "#E8FF47" },
            { "Danger", "#FF4444" },
            { "Success", "#44FF88" },
            { "BorderColor", "#2A2A2A" },
            { "MidnightBlue", "#0D0D0D" },
            { "OffBlack", "#0D0D0D" },
        };

        // Light theme colors
        private static readonly Dictionary<string, string> LightColors = new()
        {
            { "BackgroundDark", "#F2F2F7" },
            { "Surface", "#FFFFFF" },
            { "Surface2", "#E8E8ED" },
            { "TextPrimary", "#1C1C1E" },
            { "TextSecondary", "#6E6E73" },
            { "PrimaryAccent", "#C8D800" },
            { "Danger", "#FF3B30" },
            { "Success", "#34C759" },
            { "BorderColor", "#D1D1D6" },
            { "MidnightBlue", "#F2F2F7" },
            { "OffBlack", "#F2F2F7" },
        };

        public ThemeService()
        {
            var saved = Preferences.Get(ThemePreferenceKey, "dark");
            CurrentTheme = saved == "light" ? AppThemeMode.Light : AppThemeMode.Dark;
        }

        public void SetTheme(AppThemeMode theme)
        {
            CurrentTheme = theme;
            Preferences.Set(ThemePreferenceKey, theme == AppThemeMode.Light ? "light" : "dark");
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            var colors = CurrentTheme == AppThemeMode.Light ? LightColors : DarkColors;
            var resources = Application.Current?.Resources;

            if (resources == null)
                return;

            foreach (var kvp in colors)
            {
                var color = Color.FromArgb(kvp.Value);

                // Update the Color resource
                if (resources.ContainsKey(kvp.Key))
                    resources[kvp.Key] = color;
                else
                    SetInMergedDictionaries(resources, kvp.Key, color);

                // Update the corresponding Brush resource
                var brushKey = kvp.Key + "Brush";
                var brush = new SolidColorBrush(color);
                if (resources.ContainsKey(brushKey))
                    resources[brushKey] = brush;
                else
                    SetInMergedDictionaries(resources, brushKey, brush);
            }

            // Also update legacy "Primary" to match PrimaryAccent
            var primaryColor = Color.FromArgb(colors["PrimaryAccent"]);
            if (resources.ContainsKey("Primary"))
                resources["Primary"] = primaryColor;
            else
                SetInMergedDictionaries(resources, "Primary", primaryColor);

            if (resources.ContainsKey("PrimaryBrush"))
                resources["PrimaryBrush"] = new SolidColorBrush(primaryColor);
            else
                SetInMergedDictionaries(resources, "PrimaryBrush", new SolidColorBrush(primaryColor));
        }

        private static void SetInMergedDictionaries(ResourceDictionary resources, string key, object value)
        {
            foreach (var dict in resources.MergedDictionaries)
            {
                if (dict.ContainsKey(key))
                {
                    dict[key] = value;
                    return;
                }
            }
        }

        public bool IsDarkMode => CurrentTheme == AppThemeMode.Dark;
    }
}
