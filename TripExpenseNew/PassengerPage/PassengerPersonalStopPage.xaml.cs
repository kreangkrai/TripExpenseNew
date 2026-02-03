using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Services;
using TripExpenseNew.DBService;
using TripExpenseNew.CustomPopup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using Plugin.LocalNotification;


#if IOS
using CoreLocation;
using UserNotifications;
using Microsoft.Maui.Maps;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;

#endif
namespace TripExpenseNew.PassengerPage;

public partial class PassengerPersonalStopPage : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IMileage Mileage;
    private DBInterface.IPersonal DB_Personal;
    private Interface.IPersonal _Personal;
    private ILastTrip LastTrip;
    private DBInterface.IActivePersonal ActivePersonal;
    private IPassengerPersonal PassengerPersonal;
    private IInternet Internet;
    private CancellationTokenSource cancellationTokenSource;
    private bool isTracking = true;
    Tuple<string, bool> loc = new Tuple<string, bool>("", false);
    Location g_location = null;
    bool IsCustomer = false;
    LastTripViewModel trip = new LastTripViewModel();
    string emp_id = "";
    CultureInfo cultureinfo = new CultureInfo("en-us");

    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
    DateTime now = DateTime.Now;
    TimePicker time_select = new TimePicker();
#if IOS
    private Platforms.iOS.LocationService locationService;
#elif ANDROID
    private Intent intent = new Intent();
#endif

    int mileage_start = 0;
    public PassengerPersonalStopPage(LastTripViewModel _trip)
    {
        InitializeComponent();
        Login = new LoginService();
        LocationCustomer = new LocationCustomerService();
        LocationOther = new LocationOtherService();
        Mileage = new MileageService();
        DB_Personal = new DBService.PersonalService();
        _Personal = new Services.PersonalService();
        LastTrip = new LastTripService();
        ActivePersonal = new ActivePersonalService();
        PassengerPersonal = new PassengerPersonalService();
        Internet = new InternetService();
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
            UpdateLocationDataAsync(data.Location);

        });

        Text_TripName.Text = _trip.trip;
        Text_Driver.Text = _trip.driver_name;
        mileage_start = _trip.mileage_start;
        emp_id = _trip.emp_id;
        trip = _trip;
        timePicker.Time = new TimeSpan(now.Hour,now.Minute,0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            var result = await this.ShowPopupAsync(new PolicyPopup());
            if (result != null)
            {
                await RequestPermissionsAsync();
            }
        }

        //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        //if (status != PermissionStatus.Granted)
        //{
        //    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        return;
        //    }
        //}

        //status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        //if (status != PermissionStatus.Granted)
        //{
        //    status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        return;
        //    }
        //}

        GetLocationCTL.Add(new LocationOtherModel()
        {
            location = "CTL(HQ)",
            latitude = 13.729175,
            longitude = 100.728538
        });
        GetLocationCTL.Add(new LocationOtherModel()
        {
            location = "CTL(RBO)",
            latitude = 12.718476,
            longitude = 101.162984
        });
        GetLocationCTL.Add(new LocationOtherModel()
        {
            location = "CTL(KBO)",
            latitude = 16.444429,
            longitude = 102.794939
        });


        LoginModel login = await Login.GetLogin(1);
        GetLocationCustomers = await LocationCustomer.GetByEmp(login.emp_id);
        GetLocationOthers = await LocationOther.GetByEmp(login.emp_id);

#if IOS
        try
        {
            locationService = new Platforms.iOS.LocationService(5);
        }
        catch (Exception ex)
        {
            //LocationLabel.Text = $"เกิดข้อผิดพลาดในการเริ่มต้น LocationService: {ex.Message}";
            Console.WriteLine($"LocationService Initialization Error: {ex}");
        }
#endif

        await GetLocation();
    }

    private async Task RequestPermissionsAsync()
    {
        // Location

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {

            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        }
        status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
        {
            return;
        }

        if (status != PermissionStatus.Granted)
        {

            await Permissions.RequestAsync<Permissions.LocationAlways>();

        }

        // Notification
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            await LocalNotificationCenter.Current.RequestNotificationPermission();
    }
    private async Task GetLocation()
    {
        try
        {
            if (isTracking)
            {
#if IOS
                // ตรวจสอบ Location Services ด้วย CLLocationManager
                if (!CLLocationManager.LocationServicesEnabled)
                {
                    //LocationLabel.Text = "Location Services ถูกปิด กรุณาเปิดใน Settings";
                    return;
                }
#else
                // สำหรับ Android และแพลตฟอร์มอื่นๆ อาจไม่สามารถตรวจสอบได้โดยตรง
                // ใช้ try-catch ใน Geolocation แทน
#endif

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        //LocationLabel.Text = "ไม่ได้รับอนุญาต กรุณาเปิดใช้บริการตำแหน่ง";
                        return;
                    }
                }

#if IOS || ANDROID
                status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationAlways>();
                    if (status != PermissionStatus.Granted)
                    {
                        //LocationLabel.Text = "ไม่ได้รับอนุญาต Background Location";
                        return;
                    }
                }
#endif


                cancellationTokenSource = new CancellationTokenSource();

#if IOS
                if (locationService == null)
                {
                    // LocationLabel.Text = "LocationService ไม่ได้เริ่มต้น";
                    return;
                }
                locationService.StartUpdatingLocation(async location =>
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UpdateLocationDataAsync(location));
                });
#elif ANDROID
                intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                intent.PutExtra("TrackingInterval", 5000);
                Platform.AppContext.StartForegroundService(intent);
#endif
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Crash in OnToggleTrackingClicked: {ex}");
        }
    }

    private void UpdateLocationDataAsync(Location location)
    {
        try
        {
            if (location != null)
            {

                FindLocationService findLocation = new FindLocationService();
                loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, location);

                g_location = location;
                IsCustomer = loc.Item2;
                if (IsCustomer) // Customer
                {
                    CustomerBtn.BackgroundColor = Color.FromArgb("#297CC0");
                    OtherBtn.BackgroundColor = Colors.LightGrey;
                }
                else
                {
                    CustomerBtn.BackgroundColor = Colors.LightGrey;
                    OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
                }
            }
            else
            {

            }

            #region STOP
#if IOS
            locationService?.StopUpdatingLocation();
            locationService = null;
#elif ANDROID
            intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            Platform.AppContext.StopService(intent);
#endif
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
        }
    }

    private void OtherBtn_Clicked(object sender, EventArgs e)
    {
        IsCustomer = false;
        CustomerBtn.BackgroundColor = Colors.LightGrey;
        OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
    }

    private void CustomerBtn_Clicked(object sender, EventArgs e)
    {
        IsCustomer = true;
        CustomerBtn.BackgroundColor = Color.FromArgb("#297CC0");
        OtherBtn.BackgroundColor = Colors.LightGrey;
    }

    private async void ConfirmBtn_Clicked(object sender, EventArgs e)
    {
        ConfirmBtn.IsEnabled = false;
        if (Text_Location.Text.Trim() != "")
        {
            bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
            if (internet)
            {

                var popup = new ProgressPopup();
                this.ShowPopup(popup);

                DateTime date = new DateTime(trip.trip_start.Year, trip.trip_start.Month, trip.trip_start.Day, time_select.Time.Hours, time_select.Time.Minutes, time_select.Time.Seconds);

                double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                string message = "";

                #region ADD PASSENGER

                PassengerPersonalModel passengerPersonal = new PassengerPersonalModel()
                {
                    date = date,
                    driver = trip.driver,
                    trip = trip.trip,
                    job_id = trip.job_id,
                    latitude = g_location.Latitude,
                    longitude = g_location.Longitude,
                    accuracy = g_location.Accuracy.HasValue ? g_location.Accuracy.Value : 10.0,
                    location = Text_Location.Text,
                    location_mode = IsCustomer ? "CUSTOMER" : "OTHER",
                    passenger = emp_id,
                    status = "STOP",
                    zipcode = zipcode
                };
                message = await PassengerPersonal.Insert(passengerPersonal);

                LastTripModel lastTrip_passenger = new LastTripModel()
                {
                    driver = trip.driver,
                    speed = 0,
                    emp_id = emp_id,
                    job_id = trip.job_id,
                    trip_start = trip.trip_start,
                    date = date,
                    distance = 0,
                    location = Text_Location.Text,
                    latitude = g_location.Latitude,
                    longitude = g_location.Longitude,
                    accuracy = g_location.Accuracy.HasValue ? g_location.Accuracy.Value : 10.0,
                    mileage_start = 0,
                    mileage_stop = 0,
                    mode = "PASSENGER PERSONAL",
                    status = false,
                    trip = trip.trip,
                    car_id = "",
                    borrower_id = ""
                };

                message = await LastTrip.UpdateByTrip(lastTrip_passenger);

                #endregion

                await popup.CloseAsync();
                await Shell.Current.GoToAsync("Home_Page");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", "Cann't connect to server", "OK");
                });
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", "Please input current location", "OK");
            });
        }
        ConfirmBtn.IsEnabled = true;
    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
#if IOS
        locationService?.StopUpdatingLocation();
        locationService = null;
#elif ANDROID
        intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
        Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("Home_Page");
        CancelBtn.IsEnabled = true;
    }

    private void timePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var timePicker = sender as TimePicker;
            var selectedTime = timePicker.Time;
            var currentTime = now.TimeOfDay;
            if (trip.trip_start.Date == now.Date)
            {
                if (selectedTime <= currentTime.Add(new TimeSpan(0, 3, 0)))
                {

                    time_select.Time = selectedTime;
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "Current time incorrect", "OK");
                    });
                }
            }
            else
            {
                time_select.Time = selectedTime;
            }
        }
    }
}