using Microsoft.Maui.Controls;


namespace Gymaui_App
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            MainPage = serviceProvider.GetRequiredService<AppShell>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage ?? _serviceProvider.GetRequiredService<AppShell>());
        }
    }
}
