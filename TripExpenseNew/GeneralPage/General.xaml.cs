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
using Plugin.LocalNotification;
using TripExpenseNew.ViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using TripExpenseNew.PassengerPage;
using TripExpenseNew.CustomPopup;
using System.Globalization;
using TripExpenseNew.CustomPersonalPopup;
using TripExpenseNew.CustomGeneralPopup;
#if IOS
using UserNotifications;
using Microsoft.Maui.Maps;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using System.Reflection.Emit;

#endif
#if IOS
using CoreLocation;
#endif

namespace TripExpenseNew.GeneralPage
{
    public partial class General : ContentPage
    {
        private IInternet Internet;
        private Location previousLocation = null;
        private Location g_location = null;
        private double totalDistance = 0;
        string emp_id = "";
        private int mileage_start = 0;
        GeneralPopupStartModel start = new GeneralPopupStartModel();
        bool isStart = false;
        bool isWaitStop = false;
        DateTime trip_start = DateTime.MinValue;
        DateTime start_tracking = DateTime.MinValue;
        CultureInfo cultureinfo = new CultureInfo("en-us");
        List<ActivePersonalModel> act_general = new List<ActivePersonalModel>();
        TrackingModel tracking = new TrackingModel();
        private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
        int interval = 0;
        int tracking_db = 0;
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif

        public General(GeneralPopupStartModel _start)
        {
            InitializeComponent();
            start = _start;         
            Internet = new InternetService();
            totalDistance = start.distance;
            trip_start = start.trip_start;
            g_location = start.location;
            mileage_start = start.mileage;

            WeakReferenceMessenger.Default.Register<LocationData>(this, async (send, data) =>
            {
                await UpdateLocationDataAsync(data.Location);
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            interval = tracking.time_interval;
            start_tracking = DateTime.Now;

            OnStartTracking();
#if IOS
            try
            {
                locationService = new Platforms.iOS.LocationService(interval);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocationService Initialization Error: {ex}");
            }
#endif
        }

        async Task RequestNotificationPermission()
        {
            if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
        }

        public async Task SendNotification(string title, string message)
        {
#if IOS
            try
            {
                // ขอสิทธิ์
                var center = UNUserNotificationCenter.Current;
                var (approved, error) = await center.RequestAuthorizationAsync(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);
                if (!approved)
                {
                    Console.WriteLine("Notification permission denied");
                    return;
                }

                // สร้างการแจ้งเตือน
                var content = new UNMutableNotificationContent
                {
                    Title = title,
                    Body = message,
                    Badge = 1,
                    Sound = UNNotificationSound.Default
                };

                // กำหนดเวลา
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(2, false);
                var request = UNNotificationRequest.FromIdentifier(((int)DateTime.Now.Ticks).ToString(), content, trigger);

                // ส่งการแจ้งเตือน
                await center.AddNotificationRequestAsync(request);
                Console.WriteLine("Native iOS notification sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending native notification: {ex.Message}");
            }
#else
            // ใช้ Plugin.LocalNotification สำหรับ Android
            try
            {
                var notification = new NotificationRequest
                {
                    NotificationId = (int)DateTime.Now.Ticks,
                    Title = title,
                    Description = message,
                    BadgeNumber = 1,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(2)
                    }
                };

                Console.WriteLine("Sending notification...");
                await LocalNotificationCenter.Current.Show(notification);
                Console.WriteLine("Notification sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
#endif
        }

        private async void OnStartTracking()
        {
            try
            {               
                emp_id = "EMP001";
                previousLocation = null;

#if IOS
                // ตรวจสอบ Location Services ด้วย CLLocationManager
                if (!CLLocationManager.LocationServicesEnabled)
                {
                    Console.WriteLine("Location Services ถูกปิด กรุณาเปิดใน Settings");
                    return;
                }

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
                    ActivePersonalModel ap = new ActivePersonalModel()
                    {
                        driver = emp_id,
                        distance = totalDistance,
                        location = start.location_name,
                        mileage = start.mileage,
                        status = "START",
                        trip = start.trip,
                        date = DateTime.Now
                    };

                    act_general.Add(ap);

                    isStart = true;
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

                #endregion

                Text_Detail.Text = $"Active Trip Detail : ({tripItems.Count})";

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
                bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
                if (internet)
                {
                    isWaitStop = true;

                    var result = await this.ShowPopupAsync(new GeneralStopPopup(start.mileage));

                    if (result != null)
                    {
                        if (result is GeneralPopupStopModel general)
                        {
                            if (general.location != null && general.location != "" && general.mileage != 0)
                            {
                                if (general.mileage >= start.mileage)
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
                                        mileage_start = start.mileage,
                                        mileage_stop  = general.mileage,
                                        emp_id = "EMP001",
                                        emp_name = "GUEST",
                                        date = DateTime.Now.ToString("dd/MM/yyyy",cultureinfo),
                                        trip = start.trip,
                                        distance = totalDistance,
                                        location = general.location,
                                    };
                                    var resule = await this.ShowPopupAsync(new GeneralHistoryPopup(data));

                                    if (resule == null)
                                    {
                                        await Shell.Current.GoToAsync("Initial_Page");
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
                                        await DisplayAlert("", "กรุณาใส่ข้อมูลไมล์ให้ถูกต้อง", "ตกลง");
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
                        }
                    }
                    else
                    {
                        isWaitStop = false;
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
                    bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
                    if (internet)
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
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "Cann't connect to server", "OK");
                    });
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