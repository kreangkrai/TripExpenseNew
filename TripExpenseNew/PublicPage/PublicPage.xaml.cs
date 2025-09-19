using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;
using TripExpenseNew.CustomPopup;
using TripExpenseNew.ViewModels;
using TripExpenseNew.CustomPersonalPopup;
using TripExpenseNew.CustomPublicPopup;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if IOS
using UserNotifications;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS
using CoreLocation;
#endif
namespace TripExpenseNew.PublicPage;

public partial class PublicPage : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IMileage Mileage;
    private IInternet Internet;
    private bool isTracking = true;
    Tuple<string, bool> loc = new Tuple<string, bool>("", false);
    Location g_location = null;
    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif
    public PublicPage(ILocationCustomer _LocationCustomer, ILogin _Login, ILocationOther _LocationOther, IMileage _Mileage, IInternet _Internet)
    {
        InitializeComponent();
        Login = _Login;
        LocationCustomer = _LocationCustomer;
        LocationOther = _LocationOther;
        Mileage = _Mileage;
        Internet = _Internet;
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
            if (send != null)
            {
                UpdateLocationDataAsync(data.Location);
            }

        });
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
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK", "Cancel");
                    if (confirm || !confirm)
                    {
                        AppInfo.ShowSettingsUI();
                    }
                });

                return;
            }
        }

        //if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        //{
        //    await LocalNotificationCenter.Current.RequestNotificationPermission();
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
    private void UpdateLocationDataAsync(Location location)
    {
        try
        {
            if (location != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonPublicStart viewModel)
                    {
                        viewModel.ButtonPublicStartText = "START";
                    }
                    else
                    {
                        PublicStart.IsEnabled = true;
                        PublicStart.TextColor = Colors.White;
                        PublicStart.BackgroundColor = Color.FromArgb("#297CC0");
                        PublicStart.Text = "START";
                    }
                });


                FindLocationService findLocation = new FindLocationService();
                loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, location);

                g_location = location;
                //Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonPublicStart viewModel)
                    {
                        viewModel.ButtonPublicStartText = "Processing..";
                    }
                    else
                    {
                        PublicStart.IsEnabled = false;
                        PublicStart.TextColor = Colors.White;
                        PublicStart.BackgroundColor = Colors.Grey;
                        PublicStart.Text = "Processing..";
                    }
                });
            }

            #region STOP
#if IOS
            locationService?.StopUpdatingLocation();
            locationService = null;
//#elif ANDROID
            //intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            //Platform.AppContext.StopService(intent);
#endif
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
        }
    }

    private async void PublicStart_Clicked(object sender, EventArgs e)
    {
        PublicStart.IsEnabled = false;
        bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
        if (internet)
        {
            var result = await this.ShowPopupAsync(new PublicStartPopup(loc.Item1, loc.Item2));

            if (result is PublicPopupStartModel p)
            {
                if (p.location_name != null && p.location_name != "")
                {
                    p.location = g_location;
                    p.IsContinue = false;
                    p.trip_start = DateTime.Now;
                    p.job_id = p.job_id != null ? p.job_id : "";
                    await Navigation.PushAsync(new Public(p));
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "กรุณาใส่ข้อมูล", "ตกลง");
                    });
                }
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", "Cann't connect to server", "OK");
            });
        }
        PublicStart.IsEnabled = true;
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
                intent.PutExtra("TrackingInterval", 2000);
                Platform.AppContext.StartForegroundService(intent);
#endif
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Crash in OnToggleTrackingClicked: {ex}");
        }
    }

    private async void PublicCancel_Clicked(object sender, EventArgs e)
    {
        PublicCancel.IsEnabled = false;
#if IOS
        locationService?.StopUpdatingLocation();
#elif ANDROID
                    intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("Home_Page");
        PublicCancel.IsEnabled = true;
    }
}