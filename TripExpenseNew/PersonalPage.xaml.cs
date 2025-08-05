using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;

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
namespace TripExpenseNew;

public partial class PersonalPage : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private CancellationTokenSource cancellationTokenSource;
    private bool isTracking = false;
    Tuple<string, bool> loc = new Tuple<string, bool> ("",false);
    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
#if IOS
        private Platforms.iOS.LocationService locationService;

#endif
    public PersonalPage(ILocationCustomer _LocationCustomer, ILogin _Login, ILocationOther _LocationOther)
	{
		InitializeComponent();

        Login = _Login;
        LocationCustomer = _LocationCustomer;
        LocationOther = _LocationOther;
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
             UpdateLocationDataAsync(data.Location);
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
                return;
            }
        }

        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
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
                locationService = new Platforms.iOS.LocationService();
            }
            catch (Exception ex)
            {
                //LocationLabel.Text = $"เกิดข้อผิดพลาดในการเริ่มต้น LocationService: {ex.Message}";
                Console.WriteLine($"LocationService Initialization Error: {ex}");
            }
#endif

        await GetLocation();
    }
    private async void PersonalStart_Clicked(object sender, EventArgs e)
    {

        var result = await this.ShowPopupAsync(new PersonalStartPopup(loc.Item1,loc.Item2));

        if (result is PersonalPopupStartModel personal)
        {
            if (personal.location != null && personal.location != "" && personal.mileage != 0)
            {
                await Shell.Current.GoToAsync("Personal");
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

    private async void PersonalCancel_Clicked(object sender, EventArgs e)
    {
#if IOS
                    locationService?.StopUpdatingLocation();
#elif ANDROID
                    var intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("Home_Page");
    }

    private async Task GetLocation()
    {
        try
        {
            if (!isTracking)
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

                isTracking = true;
                //ToggleButton.Text = "หยุด";
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
                    var intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StartForegroundService(intent);
                    //await locationService.StartTrackingAsync(cancellationTokenSource.Token);
                    //await StartTrackingAsync(cancellationTokenSource.Token);
#endif
            }
            else
            {
                isTracking = false;
                cancellationTokenSource?.Cancel();

#if IOS
                    locationService?.StopUpdatingLocation();
#elif ANDROID
                    var intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
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
            Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}");
#if IOS
                    locationService?.StopUpdatingLocation();
#elif ANDROID
                    var intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
#endif
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PersonalStart.IsEnabled = true;
            });

            FindLocationService findLocation = new FindLocationService();
            loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, location);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
        }
    }
}