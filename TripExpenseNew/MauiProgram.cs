using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Maps;
using Plugin.LocalNotification;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBService;
using TripExpenseNew.Interface;
using TripExpenseNew.PersonalPage;
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
                    fonts.AddFont("Rajdhani-Bold.ttf", "RajdhaniBold");
                    fonts.AddFont("Rajdhani-Light.ttf", "RajdhaniLight");
                    fonts.AddFont("Rajdhani-Medium.ttf", "RajdhaniMedium");
                    fonts.AddFont("Rajdhani-Regular.ttf", "RajdhaniRegular");
                    fonts.AddFont("Rajdhani-SemiBold.ttf", "RajdhaniSemibold");
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
            builder.Services.AddTransient<ILocationOther, LocationOtherService>();
            builder.Services.AddTransient<IOther, OtherService>();
            builder.Services.AddTransient<IPassengerCompany, PassengerCompanyService>();
            builder.Services.AddTransient<IPassengerPersonal, PassengerPersonalService>();
            builder.Services.AddTransient<Interface.IPersonal, Services.PersonalService>();
            builder.Services.AddTransient<ITracking, TrackingService>();
            builder.Services.AddTransient<IVersion, VersionService>();
            builder.Services.AddTransient<ICurrentTime, CurrentTimeService>();
            builder.Services.AddTransient<IInternet, InternetService>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Personal>();
            builder.Services.AddTransient<PersonalPage.PersonalPage>();
            builder.Services.AddTransient<ModePage>();
            builder.Services.AddTransient<Initial_Page>();
            builder.Services.AddTransient<Login_Page>();
            builder.Services.AddTransient<Home_Page>();

            builder.Services.AddSingleton<IServer, ServerService>();
            builder.Services.AddSingleton<ILogin, LoginService>();
            builder.Services.AddSingleton<IActivePersonal, ActivePersonalService>();
            builder.Services.AddSingleton<IActiveCompany, ActiveCompanyService>();
            builder.Services.AddSingleton<DBInterface.IPersonal, DBService.PersonalService>();
            builder.Services.AddSingleton<DBInterface.ICompany,DBService.CompanyService>();
            builder.Services.AddSingleton<DBInterface.IMileage, DBService.MileageService>();

            return builder.Build();
        }
    }
}
