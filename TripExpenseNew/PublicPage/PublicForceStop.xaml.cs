namespace TripExpenseNew.PublicPage;

using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Models;
using TripExpenseNew.Interface;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Services;
using TripExpenseNew.DBService;
using TripExpenseNew.CustomPopup;
using CommunityToolkit.Maui.Views;
using System.Globalization;

#if IOS
using CoreLocation;
using UserNotifications;
using Microsoft.Maui.Maps;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;

#endif

public partial class PublicForceStop : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IMileage Mileage;
    private DBInterface.IPublic DB_Public;
    private Interface.IPublic _Public;
    private ILastTrip LastTrip;
    private DBInterface.IActivePublic ActivePublic;
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

    public PublicForceStop(LastTripViewModel _trip)
    {
        InitializeComponent();
        Login = new LoginService();
        LocationCustomer = new LocationCustomerService();
        LocationOther = new LocationOtherService();
        Mileage = new MileageService();
        DB_Public = new DBService.PublicService();
        _Public = new Services.PublicService();
        LastTrip = new LastTripService();
        ActivePublic = new ActivePublicService();
        Internet = new InternetService();
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
            UpdateLocationDataAsync(data.Location);

        });

        Text_TripName.Text = _trip.trip;
        emp_id = _trip.emp_id;
        trip = _trip;
        timePicker.Time = new TimeSpan(17, 30, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }
        }

        status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }
        }

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
                    OtherBtn.BackgroundColor = Colors.Grey;
                }
                else
                {
                    CustomerBtn.BackgroundColor = Colors.Grey;
                    OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
                }


                Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}");
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
        CustomerBtn.BackgroundColor = Colors.Grey;
        OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
    }

    private void CustomerBtn_Clicked(object sender, EventArgs e)
    {
        IsCustomer = true;
        CustomerBtn.BackgroundColor = Color.FromArgb("#297CC0");
        OtherBtn.BackgroundColor = Colors.Grey;
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

                PublicModel data_public = new PublicModel();
                    string message = "Success";

                    await DB_Public.Delete(trip.trip);

                    data_public = new PublicModel()
                    {
                        passenger = emp_id,
                        date = date,
                        job_id = trip.job_id,
                        distance = trip.distance,
                        latitude = g_location.Latitude,
                        longitude = g_location.Longitude,
                        accuracy = g_location.Accuracy.HasValue ? g_location.Accuracy.Value : 10.0,
                        location = Text_Location.Text,
                        zipcode = zipcode,
                        location_mode = IsCustomer ? "CUSTOMER" : "OTHER",
                        speed = speed,
                        trip = trip.trip,
                        status = "STOP"
                    };
                    message = await _Public.Insert(data_public);

                    if (message == "Success")
                    {
                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = "",
                            speed = data_public.speed,
                            emp_id = data_public.passenger,
                            job_id = data_public.job_id,
                            trip_start = trip.trip_start,
                            date = data_public.date,
                            distance = data_public.distance,
                            location = data_public.location,
                            latitude = data_public.latitude,
                            longitude = data_public.longitude,
                            accuracy = data_public.accuracy,
                            mileage_start = 0,
                            mileage_stop = 0,
                            mode = "PUBLIC",
                            status = false,
                            trip = data_public.trip,
                            car_id = "",
                            borrower_id = ""
                        };

                        message = await LastTrip.UpdateByTrip(lastTrip);

                        int act = await ActivePublic.Delete(trip.trip);
                    }


                    #region Add Location

                    if (loc.Item1 != Text_Location.Text && Text_Location.Text != "CTL(HQ)" && Text_Location.Text != "CTL(KBO)" && Text_Location.Text != "CTL(RBO)") // Insert New Location
                    {
                        if (IsCustomer)
                        {
                            LocationCustomerModel locationCustomer = new LocationCustomerModel()
                            {
                                emp_id = emp_id,
                                latitude = g_location.Latitude,
                                longitude = g_location.Longitude,
                                location = Text_Location.Text,
                                location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                zipcode = zipcode,
                            };
                            await LocationCustomer.Insert(locationCustomer);
                        }
                        else
                        {
                            LocationOtherModel locationOther = new LocationOtherModel()
                            {
                                emp_id = emp_id,
                                latitude = g_location.Latitude,
                                longitude = g_location.Longitude,
                                location = Text_Location.Text,
                                location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                zipcode = zipcode,
                            };
                            await LocationOther.Insert(locationOther);
                        }
                    }
                    #endregion

                    #region Stop
#if IOS
                    locationService?.StopUpdatingLocation();
                    locationService = null; // รีเซ็ต locationService
#elif ANDROID
                intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                Platform.AppContext.StopService(intent);
#endif
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
                await DisplayAlert("", "กรุณาใส่ข้อมูล", "ตกลง");
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
                        await DisplayAlert("", "เวลาไม่ถูกต้อง", "ตกลง");
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