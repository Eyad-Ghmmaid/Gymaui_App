using Gymaui_App.Services;

namespace Gymaui_App
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            // Apply saved theme
            var themeService = serviceProvider.GetRequiredService<ThemeService>();
            themeService.ApplyTheme();

            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }

        [Obsolete]
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage ?? _serviceProvider.GetRequiredService<AppShell>());
        }
    }
}
