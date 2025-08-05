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
using Plugin.LocalNotification;

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

namespace TripExpenseNew
{
    public partial class Personal : ContentPage
    {
        private readonly IAuthen AuthenService;
        private ILogin Login;
        private Interface.IPersonal _Personal;
        private bool isTracking = false;
        private CancellationTokenSource cancellationTokenSource;
        private Location previousLocation = null;
        private double totalDistance = 0;
        string emp_id = "";

#if IOS
        private Platforms.iOS.LocationService locationService;

#endif

        public Personal(IAuthen _AuthenService, Interface.IPersonal _Personal_, ILogin _Login)
        {
            InitializeComponent();
            AuthenService = _AuthenService;
            _Personal = _Personal_;
            Login = _Login;
            
            WeakReferenceMessenger.Default.Register<LocationData>(this, async (send, data) =>
            {
                await UpdateLocationDataAsync(data.Location);
                totalDistance = data.TotalDistance;
            });

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

        private async void OnToggleTrackingClicked(object sender, EventArgs e)
        {
            try
            {
                LoginModel login = await Login.GetLogin(1);
                emp_id = login.emp_id;

                await RequestNotificationPermission();
                await SendNotification("สวัสดี", "นี่คือการแจ้งเตือนจาก MAUI!");

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
                    previousLocation = null;
                    totalDistance = 0;

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
                    //ToggleButton.Text = "เริ่ม";
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
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    //LocationLabel.Text = $"เกิดข้อผิดพลาด: {ex.Message}";
                });
                Console.WriteLine($"Crash in OnToggleTrackingClicked: {ex}");
            }
        }

        //private async Task StartTrackingAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        while (!cancellationToken.IsCancellationRequested)
        //        {
        //            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
        //            var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

        //            if (location != null)
        //            {
        //                await UpdateLocationDataAsync(location);
        //            }
        //            else
        //            {
        //                await MainThread.InvokeOnMainThreadAsync(() =>
        //                {
        //                    LocationLabel.Text = "ไม่สามารถดึงตำแหน่งได้";
        //                    SpeedLabel.Text = "ความเร็ว: N/A";
        //                    DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
        //                    ZipcodeLabel.Text = "รหัสไปรษณีย์: N/A";
        //                });
        //            }

        //            await Task.Delay(5000, cancellationToken);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await MainThread.InvokeOnMainThreadAsync(() =>
        //        {
        //            LocationLabel.Text = $"หยุดการติดตาม: {ex.Message}";
        //            SpeedLabel.Text = "ความเร็ว: N/A";
        //            DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
        //            ZipcodeLabel.Text = "รหัสไปรษณีย์: N/A";
        //        });
        //        Console.WriteLine($"StartTrackingAsync Error: {ex}");
        //    }
        //}

        private async Task UpdateLocationDataAsync(Location location)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    //LocationLabel.Text = $"ละติจูด: {location.Latitude:F6}, ลองจิจูด: {location.Longitude:F6}";
                    //SpeedLabel.Text = $"ความเร็ว: {(location.Speed.HasValue ? location.Speed.Value * 3.6 : 0):F2} กม./ชม.";
                });

                if (previousLocation != null)
                {
                    totalDistance += CalculateDistance(previousLocation, location);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        //DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
                    });
                }
                previousLocation = location;

                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    //ZipcodeLabel.Text = $"รหัสไปรษณีย์: {zipcode}";
                });

                double speed = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;
                PersonalModel personal = new PersonalModel()
                {
                    driver = emp_id,
                    date = DateTime.Now,
                    job_id = "J250009",
                    distance = totalDistance,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    location = zipcode,
                    zipcode = zipcode,
                    location_mode = "CUSTOMER",
                    speed = speed,
                    mileage = 1234,
                    trip = DateTime.Now,
                    status = "START"
                };
                string message = await _Personal.Insert(personal);
                Console.WriteLine($"ALL ==> {message} Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}, Zipcode: {zipcode}");
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    //LocationLabel.Text = $"ข้อผิดพลาดใน UpdateLocationData: {ex.Message}";
                });
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

        private async void StopTripBtn_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("Home_Page");
        }
    }
}