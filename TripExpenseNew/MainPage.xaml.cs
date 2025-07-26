using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Models;


#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS
using CoreLocation;
#endif

namespace TripExpenseNew
{
    public partial class MainPage : ContentPage
    {
        private bool isTracking = false;
        private CancellationTokenSource cancellationTokenSource;
        private Location previousLocation = null;
        private double totalDistance = 0;

#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Platforms.Android.LocationService locationService;
#endif

        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<LocationData>(this,async (send,data) =>
            {
                Console.WriteLine($"==================Reguster : {data.Location.Longitude}");
                await UpdateLocationDataAsync(data.Location);
                totalDistance = data.TotalDistance; // อัปเดต totalDistance จาก Service
            });

#if IOS
            try
            {
                locationService = new Platforms.iOS.LocationService();
            }
            catch (Exception ex)
            {
                LocationLabel.Text = $"เกิดข้อผิดพลาดในการเริ่มต้น LocationService: {ex.Message}";
                Console.WriteLine($"LocationService Initialization Error: {ex}");
            }
#elif ANDROID
        try
            {
                locationService = new Platforms.Android.LocationService();
            }
            catch (Exception ex)
            {
                LocationLabel.Text = $"เกิดข้อผิดพลาดในการเริ่มต้น LocationService: {ex.Message}";
                Console.WriteLine($"LocationService Initialization Error: {ex}");
            }
#endif
        }

        private async void OnToggleTrackingClicked(object sender, EventArgs e)
        {
            try
            {
                if (!isTracking)
                {
#if IOS
                    // ตรวจสอบ Location Services ด้วย CLLocationManager
                    if (!CLLocationManager.LocationServicesEnabled)
                    {
                        LocationLabel.Text = "Location Services ถูกปิด กรุณาเปิดใน Settings";
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
                            LocationLabel.Text = "ไม่ได้รับอนุญาต กรุณาเปิดใช้บริการตำแหน่ง";
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
                            LocationLabel.Text = "ไม่ได้รับอนุญาต Background Location";
                            return;
                        }
                    }
#endif

                    isTracking = true;
                    ToggleButton.Text = "หยุด";
                    cancellationTokenSource = new CancellationTokenSource();
                    previousLocation = null;
                    totalDistance = 0;

#if IOS
                    if (locationService == null)
                    {
                        LocationLabel.Text = "LocationService ไม่ได้เริ่มต้น";
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
                    ToggleButton.Text = "เริ่ม";
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
                    LocationLabel.Text = $"เกิดข้อผิดพลาด: {ex.Message}";
                });
                Console.WriteLine($"Crash in OnToggleTrackingClicked: {ex}");
            }
        }

        private async Task StartTrackingAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

                    if (location != null)
                    {
                        await UpdateLocationDataAsync(location);
                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LocationLabel.Text = "ไม่สามารถดึงตำแหน่งได้";
                            SpeedLabel.Text = "ความเร็ว: N/A";
                            DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
                            ZipcodeLabel.Text = "รหัสไปรษณีย์: N/A";
                        });
                    }

                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text = $"หยุดการติดตาม: {ex.Message}";
                    SpeedLabel.Text = "ความเร็ว: N/A";
                    DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
                    ZipcodeLabel.Text = "รหัสไปรษณีย์: N/A";
                });
                Console.WriteLine($"StartTrackingAsync Error: {ex}");
            }
        }

        private async Task UpdateLocationDataAsync(Location location)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text = $"ละติจูด: {location.Latitude:F6}, ลองจิจูด: {location.Longitude:F6}";
                    SpeedLabel.Text = $"ความเร็ว: {(location.Speed.HasValue ? location.Speed.Value * 3.6 : 0):F2} กม./ชม.";
                });

                if (previousLocation != null)
                {
                    totalDistance += CalculateDistance(previousLocation, location);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        DistanceLabel.Text = $"ระยะทาง: {totalDistance:F2} กม.";
                    });
                }
                previousLocation = location;

                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ZipcodeLabel.Text = $"รหัสไปรษณีย์: {zipcode}";
                });

                Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {(location.Speed.HasValue ? location.Speed.Value * 3.6 : 0)}, Distance: {totalDistance}, Zipcode: {zipcode}");
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocationLabel.Text = $"ข้อผิดพลาดใน UpdateLocationData: {ex.Message}";
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
    }
}