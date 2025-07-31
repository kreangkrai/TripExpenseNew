using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Maps;
using Plugin.LocalNotification;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBService;
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
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddTransient<IConnectAPI, ConnectAPIService>();
            builder.Services.AddTransient<IAuthen, AuthenService>();

            builder.Services.AddTransient<IBorrowerLog, BorrowerLogService>();
            builder.Services.AddTransient<IBorrower, BorrowerService>();
            builder.Services.AddTransient<ICar, CarService>();
            builder.Services.AddTransient<Interface.ICompany, Services.CompanyService>();
            builder.Services.AddTransient<IEmployee, EmployeeService>();
            builder.Services.AddTransient<ILastTrip, LastTripService>();
            builder.Services.AddTransient<ILocationCustomer, LocationCustomerService>();
            builder.Services.AddTransient<IOther, OtherService>();
            builder.Services.AddTransient<IPassengerCompany, PassengerCompanyService>();
            builder.Services.AddTransient<IPassengerPersonal, PassengerPersonalService>();
            builder.Services.AddTransient<Interface.IPersonal, Services.PersonalService>();
            builder.Services.AddTransient<ITracking, TrackingService>();
            builder.Services.AddTransient<IVersion, VersionService>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Initial_Page>();
            builder.Services.AddTransient<Login_Page>();
            builder.Services.AddTransient<Home_Page>();

            builder.Services.AddSingleton<IServer, ServerService>();
            builder.Services.AddSingleton<ILogin, LoginService>();
            builder.Services.AddSingleton<DBInterface.IPersonal, DBService.PersonalService>();
            builder.Services.AddSingleton<DBInterface.ICompany,DBService.CompanyService>();

            return builder.Build();
        }
    }
}
