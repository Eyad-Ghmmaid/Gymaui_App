using Microsoft.Extensions.Logging;
using Gymaui_App.Services;
using Gymaui_App.Views;
using Gymaui_App.ViewModels;

namespace Gymaui_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
			builder.Logging.AddDebug();
#endif

            // Register services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // Register ViewModels as Singleton
            builder.Services.AddSingleton<ExerciseViewModel>();

            // Register Views as Transient (they will be created with their dependencies)
            builder.Services.AddTransient<StartPage>();
            builder.Services.AddTransient<ActiveWorkoutPage>();
            builder.Services.AddTransient<PlansPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<ExerciseListPage>();
            
            // Register modal/navigation pages
            builder.Services.AddTransient<AddExercisePage>();
            builder.Services.AddTransient<PlanEditorPage>();
            builder.Services.AddTransient<DayEditorPage>();
            builder.Services.AddTransient<ExerciseSetsPage>();
            builder.Services.AddTransient<CreatePlanPage>();

            // Register AppShell
            builder.Services.AddSingleton<AppShell>();

            var app = builder.Build();
            return app;
        }
    }
}

