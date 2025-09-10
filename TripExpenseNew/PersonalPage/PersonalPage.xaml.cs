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
namespace TripExpenseNew.PersonalPage;

public partial class PersonalPage : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IMileage Mileage;
    private IInternet Internet;
    private bool isTracking = true;
    Tuple<string, bool> loc = new Tuple<string, bool> ("",false);
    Location g_location = null;
    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif
    public PersonalPage(ILocationCustomer _LocationCustomer, ILogin _Login, ILocationOther _LocationOther, IMileage _Mileage, IInternet _Internet)
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
                    bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK","Cancel");
                    if (confirm || ! confirm)
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
    private async void PersonalStart_Clicked(object sender, EventArgs e)
    {
        PersonalStart.IsEnabled = false;
        bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
        if (internet)
        {
            MileageDBModel mileage = await Mileage.GetMileage(1);

            var result = await this.ShowPopupAsync(new PersonalStartPopup(loc.Item1, loc.Item2, mileage.mileage));

            if (result is PersonalPopupStartModel personal)
            {
                if (personal.location_name != null && personal.location_name != "" && personal.mileage != 0)
                {
                    //await Shell.Current.GoToAsync("Personal");
                    personal.location = g_location;
                    personal.IsContinue = false;
                    personal.trip_start = DateTime.Now;
                    personal.job_id = personal.job_id != null ? personal.job_id : "";
                    await Navigation.PushAsync(new Personal(personal));
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
        PersonalStart.IsEnabled = true;
    }

    private async void PersonalCancel_Clicked(object sender, EventArgs e)
    {
        PersonalCancel.IsEnabled = false;
#if IOS
        locationService?.StopUpdatingLocation();
#elif ANDROID
                    intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("Home_Page");
        PersonalCancel.IsEnabled = true;
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonPersonalStart viewModel)
                    {
                        viewModel.ButtonPersonalStartText = "START";
                    }
                    else
                    {
                        PersonalStart.IsEnabled = true;
                        PersonalStart.TextColor = Colors.White;
                        PersonalStart.BackgroundColor = Color.FromArgb("#297CC0");
                        PersonalStart.Text = "START";
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
                    if (BindingContext is ButtonPersonalStart viewModel)
                    {
                        viewModel.ButtonPersonalStartText = "Processing..";
                    }
                    else
                    {
                        PersonalStart.IsEnabled = false;
                        PersonalStart.TextColor = Colors.White;
                        PersonalStart.BackgroundColor = Colors.Grey;
                        PersonalStart.Text = "Processing..";
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
}