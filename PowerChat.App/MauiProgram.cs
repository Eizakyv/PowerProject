using Microsoft.Extensions.Logging;
using PowerChat.App.Services;

namespace PowerChat.App
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
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FASolid");
                });

            // This line registers the ChatService as a singleton in the dependency injection container,
            // ensuring that there is only one instance of ChatService throughout the application's lifecycle.
            builder.Services.AddSingleton<ChatService>();

            // This line registers the MainPage as a transient service in the dependency injection container,
            // meaning that a new instance of MainPage will be created each time it is requested.
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
