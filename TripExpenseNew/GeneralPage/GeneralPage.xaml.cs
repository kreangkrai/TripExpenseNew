
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
using TripExpenseNew.CustomGeneralPopup;
using CommunityToolkit.Maui.Extensions;

#if IOS
using UserNotifications;
using CoreLocation;
#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif

namespace TripExpenseNew.GeneralPage;

public partial class GeneralPage : ContentPage
{
    private IInternet Internet;
    private bool isTracking = true;
    Location g_location = null;
#if IOS
    private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif
    public GeneralPage(IInternet _Internet)
    {
        InitializeComponent();
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

        await GetLocation();
    }
    private async void GeneralStart_Clicked(object sender, EventArgs e)
    {
        bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
        if (internet)
        {
            var result = await this.ShowPopupAsync(new GeneralStartPopup());

            if (result is GeneralPopupStartModel g)
            {
                if (g.location_name != null && g.location_name != "")
                {
                    g.location = g_location;
                    g.trip_start = DateTime.Now;
                    //await Navigation.PushAsync(new General(g));
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "Please input current location", "OK");
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
    }

    private async void GeneralCancel_Clicked(object sender, EventArgs e)
    {
#if IOS
        locationService?.StopUpdatingLocation();
#elif ANDROID
                    intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                    Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("///Initial_Page");
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
                    locationService = new Platforms.iOS.LocationService(5);
                    //return;
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
                    locationService = new Platforms.iOS.LocationService(5);
                    //return;
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
                    if (BindingContext is ButtonGeneralStart viewModel)
                    {
                        viewModel.ButtonGeneralStartText = "START";
                    }
                    else
                    {
                        GeneralStart.IsEnabled = true;
                        GeneralStart.TextColor = Colors.White;
                        GeneralStart.BackgroundColor = Color.FromArgb("#297CC0");
                        GeneralStart.Text = "START";
                    }
                });

                g_location = location;
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
                        GeneralStart.IsEnabled = false;
                        GeneralStart.TextColor = Colors.White;
                        GeneralStart.BackgroundColor = Colors.Grey;
                        GeneralStart.Text = "Processing..";
                    }
                });
            }

            #region STOP
#if IOS
            //locationService?.StopUpdatingLocation();
            //locationService = null;
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