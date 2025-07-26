using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Maps;
using TripExpenseNew.Interface;
using TripExpenseNew.Services;

namespace TripExpenseNew
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            
            builder
                .UseMauiApp<App>()
                .UseMauiMaps() // เพิ่มการใช้งาน Maps
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //builder.Services.AddTransient<IAuthen, AuthenService>();
            //builder.Services.AddTransient<MainPage>();
            //builder.Services.AddTransient<App>();
            //builder.Services.AddTransient<AppShell>();
            return builder.Build();
        }
    }
}
