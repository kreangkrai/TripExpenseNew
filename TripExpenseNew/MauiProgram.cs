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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddTransient<IAuthen, AuthenService>();
            builder.Services.AddTransient<IBorrowerLog, BorrowerLogService>();
            builder.Services.AddTransient<IBorrower, BorrowerService>();
            builder.Services.AddTransient<ICar, CarService>();
            builder.Services.AddTransient<ICompany, CompanyService>();
            builder.Services.AddTransient<IEmployee, EmployeeService>();
            builder.Services.AddTransient<ILastTrip, LastTripService>();
            builder.Services.AddTransient<ILocationCustomer, LocationCustomerService>();
            builder.Services.AddTransient<IOther, OtherService>();
            builder.Services.AddTransient<IPassengerCompany, PassengerCompanyService>();
            builder.Services.AddTransient<IPassengerPersonal, PassengerPersonalService>();
            builder.Services.AddTransient<IPersonal, PersonalService>();
            builder.Services.AddTransient<ITracking, TrackingService>();
            builder.Services.AddTransient<IVersion, VersionService>();

            builder.Services.AddTransient<MainPage>();
            return builder.Build();
        }
    }
}
