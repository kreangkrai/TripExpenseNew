using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Models;
using TripExpenseNew.Interface;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Services;
using TripExpenseNew.ViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using TripExpenseNew.CustomPopup;
using System.Globalization;
using TripExpenseNew.CustomPersonalPopup;
using TripExpenseNew.CustomGeneralPopup;
using CommunityToolkit.Maui.Extensions;
#if IOS
using CoreLocation;
using Microsoft.Maui.Maps;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using System.Reflection.Emit;
#endif


namespace TripExpenseNew.GeneralPage
{
    public partial class General : ContentPage
    {   
        private Location previousLocation = null;
        private Location g_location = null;
        private double totalDistance = 0;
        string emp_id = "EM001";
        GeneralPopupStartModel start = new GeneralPopupStartModel();
        bool isStart = false;
        DateTime trip_start = DateTime.MinValue;
        CultureInfo cultureinfo = new CultureInfo("en-us");
        List<ActivePersonalModel> act_general = new List<ActivePersonalModel>();
        private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
        int interval = 10;
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif

        GeneralPopupStartModel _start = new GeneralPopupStartModel()
        {
            trip_start = DateTime.Now,
            distance = 0,
            location_name = "Home"
        };
        public General()
        {
            InitializeComponent();
            start = _start;         
            totalDistance = start.distance;
            trip_start = start.trip_start;
            g_location = start.location;

            WeakReferenceMessenger.Default.Register<LocationData>(this, async (send, data) =>
            {
                await UpdateLocationDataAsync(data.Location);
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnStartTracking();
        }
        private async void OnStartTracking()
        {
            try
            {               
                emp_id = "EMP001";
                previousLocation = null;

#if IOS
                // ตรวจสอบ Location Services ด้วย CLLocationManager
                //if (!CLLocationManager.LocationServicesEnabled)
                //{
                //    Console.WriteLine("Location Services ถูกปิด กรุณาเปิดใน Settings");
                //    return;
                //}

                if (locationService == null)
                {
                    locationService = new Platforms.iOS.LocationService(interval);
                }
                locationService.StartUpdatingLocation(async location =>
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UpdateLocationDataAsync(location));
                });
#elif ANDROID
        // ตรวจสอบสถานะ service และเริ่มใหม่
        intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
        intent.PutExtra("TrackingInterval", interval * 1000);
        Platform.AppContext.StartForegroundService(intent);

#endif

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        Console.WriteLine("ไม่ได้รับอนุญาต กรุณาเปิดใช้บริการตำแหน่ง");
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
                        bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK", "Cancel");
                        if (confirm || !confirm)
                        {
                            AppInfo.ShowSettingsUI();
                        }
                        return;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Crash in OnStartTracking: {ex}");
            }
        }

        private async Task UpdateLocationDataAsync(Location location)
        {
            try
            {

                if (previousLocation != null)
                {
                    double dist = CalculateDistance(previousLocation, location);
                    double displacement = CalculateDistanceInactive(10.0, interval);
                    if (dist >= displacement)
                    {
                        totalDistance += CalculateDistance(previousLocation, location);
                    }
                }
                previousLocation = location;

                if (!isStart)
                {
                    ActivePersonalModel ac = new ActivePersonalModel()
                    {
                        driver = emp_id,
                        distance = totalDistance,
                        location = start.location_name,
                        mileage = start.mileage,
                        status = "START",
                        trip = start.trip,
                        date = DateTime.Now
                    };

                    act_general.Add(ac);

                    isStart = true;



                    #region Show Active Personal
                    tripItems = new ObservableCollection<TripItems>();

                    foreach (var ap in act_general)
                    {
                        Color color = new Color();
                        if (ap.status == "START")
                        {
                            color = Color.FromRgb(255, 255, 255);
                        }
                        else
                        {
                            color = Color.FromRgb(255, 255, 255);
                        }
                        TripItems trip_item = new TripItems()
                        {
                            FrameColor = color,
                            TextStatus = ap.status,
                            IconLocationSource = "route.png",
                            TextLocation = $"Location: {ap.location}",
                            IconDateSource = "clock.png",
                            TextDate = $"Date: {ap.date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                        };

                        tripItems.Add(trip_item);
                    }

                    TripCollectionView.ItemsSource = tripItems;

                    #endregion

                    Text_Detail.Text = $"Active Trip Detail : ({tripItems.Count})";
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - trip_start;
                    trip_distance.Text = totalDistance.ToString("#.#") + " km";
                    trip_duration.Text = duration.ToString(@"hh\:mm");
                });

                g_location = location;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
            }
        }

        private double CalculateDistance(Location loc1, Location loc2)
        {
            double R = 6371; // รัศมีโลก (กิโลเมตร)
            double lat1 = loc1.Latitude * Math.PI / 180;
            double lat2 = loc2.Latitude * Math.PI / 180;
            double deltaLat = (loc2.Latitude - loc1.Latitude) * Math.PI / 180;
            double deltaLon = (loc2.Longitude - loc1.Longitude) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double CalculateDistanceInactive(double velocity, int duration)
        {
            return (velocity / 3.6 * duration) / 1000.0;
        }
        private async void StopTripBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await this.ShowPopupAsync(new GeneralStopPopup(start.mileage));

                if (result != null)
                {
                    if (result is GeneralPopupStopModel general)
                    {
                        if (general.location != null && general.location != "")
                        {

                            var popup = new ProgressPopup();
                            this.ShowPopup(popup);

                            #region Stop
#if IOS
                                    locationService?.StopUpdatingLocation();
                                    locationService = null; // รีเซ็ต locationService
#elif ANDROID
                            intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                            Platform.AppContext.StopService(intent);
#endif
                            #endregion

                            GeneralViewModel data = new GeneralViewModel()
                            {
                                emp_id = "EMP001",
                                emp_name = "GUEST",
                                date = DateTime.Now.ToString("dd/MM/yyyy", cultureinfo),
                                trip = start.trip,
                                distance = totalDistance,
                                location = general.location,
                            };
                            var resule = await this.ShowPopupAsync(new GeneralHistoryPopup(data));

                            if (resule == null)
                            {
                                await Shell.Current.GoToAsync("///Initial_Page");
                            }

                            previousLocation = null;
                            totalDistance = 0;
                            isStart = false;
                            trip_start = DateTime.MinValue;
                            await popup.CloseAsync();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StopTripBtn_Clicked Error: {ex}");
            }
        }

        private async void CheckInBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                var popup = new GeneralCheckInAlert { Title = "CHECK IN", Message = "Please Select type of check in?" };
                var result = await Shell.Current.ShowPopupAsync(popup);

                if (result != null)
                {

                    string chkinlocation = "";

                    bool isChkIn = false;
                    if (result.ToString() == "Location")
                    {
                        var result_location = await this.ShowPopupAsync(new GeneralCheckinLocationPopup());

                        if (result_location != null)
                        {
                            if (result_location.ToString().Trim() != "")
                            {
                                chkinlocation = result_location.ToString();
                                isChkIn = true;
                            }
                        }
                    }

                    if (isChkIn)
                    {
                        ActivePersonalModel ac = new ActivePersonalModel()
                        {
                            driver = emp_id,
                            distance = totalDistance,
                            location = chkinlocation,
                            mileage = 0,
                            status = "CHECK IN",
                            trip = start.trip,
                            date = DateTime.Now,
                        };
                        act_general.Add(ac);
                    }

                    #region Show Active Personal
                    tripItems = new ObservableCollection<TripItems>();
                    foreach (var ap in act_general)
                    {
                        Color color = new Color();
                        if (ap.status == "START")
                        {
                            color = Color.FromRgb(255, 255, 255);
                        }
                        else
                        {
                            color = Color.FromRgb(255, 255, 255);
                        }
                        TripItems trip_item = new TripItems()
                        {
                            FrameColor = color,
                            TextStatus = ap.status,
                            IconLocationSource = "route.png",
                            TextLocation = $"Location: {ap.location}",
                            IconDateSource = "clock.png",
                            TextDate = $"Date: {ap.date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                        };

                        tripItems.Add(trip_item);
                    }

                    TripCollectionView.ItemsSource = tripItems;
                    Text_Detail.Text = $"Active Trip Detail : ({tripItems.Count})";

                    #endregion

                }

            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", ex.Message, "OK");
                });
            }

        }       
    }
}