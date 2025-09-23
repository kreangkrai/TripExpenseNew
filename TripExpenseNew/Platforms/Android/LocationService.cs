using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Models;
using AndroidX.Core.Content;
using Android;
using Android.Locations;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using TripExpenseNew.Services;

namespace TripExpenseNew.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private CancellationTokenSource cancellationTokenSource;

        public override IBinder OnBind(Intent intent) => null;

        Location prevLocation = null;
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                // ตรวจสอบสิทธิ์ตำแหน่ง
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    //Console.WriteLine("ไม่มีสิทธิ์เข้าถึงตำแหน่ง");
                    StopSelf();
                    return StartCommandResult.NotSticky;
                }

                if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }

                cancellationTokenSource = new CancellationTokenSource();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channel = new NotificationChannel("location_channel", "Location Service", NotificationImportance.Low)
                    {
                        Description = "ช่องสำหรับบริการติดตามตำแหน่ง"
                    };
                    var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                    notificationManager?.CreateNotificationChannel(channel);
                }

                var notification = new Notification.Builder(this, "location_channel")
                    .SetContentTitle("กำลังติดตามตำแหน่ง")
                    .SetContentText("แอปกำลังบันทึกตำแหน่ง ความเร็ว ระยะทาง และรหัสไปรษณีย์")
                    .SetSmallIcon(Resource.Mipmap.appicon)
                    .SetOngoing(true)
                    .Build();

                StartForeground(1000, notification);

                
                int trackingInterval = intent.GetIntExtra("TrackingInterval", 1000); // ค่าเริ่มต้น 1 วินาที
                //string geolocation_accuracy = intent.GetStringExtra("GeolocationAccuracy");
                //int timeout = intent.GetIntExtra("AccuracyMeter",5);
                //int accuracy_meter = intent.GetIntExtra("AccuracyCourse",10);
                //int accuracy_course = intent.GetIntExtra("Timeout",90);

                AndroidParameterModel android = new AndroidParameterModel()
                {
                    geolocation_accuracy = "HIGH",
                    timeout = 5,
                    accuracy_meter = 30,
                    accuracy_course = 90

                };
                //Console.WriteLine($"เริ่มติดตามตำแหน่งด้วยช่วงเวลา: {trackingInterval}ms");
                Task.Run(() => StartTrackingAsync(cancellationTokenSource.Token, trackingInterval, android));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ข้อผิดพลาดใน OnStartCommand: {ex.Message}");
                StopSelf();
            }

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        private async Task StartTrackingAsync(CancellationToken cancellationToken,int trackingInterval , AndroidParameterModel android)
        {
            try
            {
                var locationManager = (LocationManager)GetSystemService(LocationService);
                if (!locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                {
                    Console.WriteLine("GPS is disabled, please enable it");
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    var loopStart = DateTime.Now;
                    GeolocationAccuracy geo = GeolocationAccuracy.Medium;
                    if (android.geolocation_accuracy == "MEDIUM")
                    {
                        geo = GeolocationAccuracy.Medium;
                    }
                    else if (android.geolocation_accuracy == "HIGH")
                    {
                        geo = GeolocationAccuracy.High;
                    }
                    else if (android.geolocation_accuracy == "BEST")
                    {
                        geo = GeolocationAccuracy.Best;
                    }
                    else
                    {
                        geo = GeolocationAccuracy.Medium;
                    }

                    var request = new GeolocationRequest(geo, TimeSpan.FromSeconds(android.timeout));
                    var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);
                    
                    if (location != null)
                    {
                        if (prevLocation != null)
                        {
                            double accuracy = location.Accuracy.HasValue ? location.Accuracy.Value : 10.0;
                            if (accuracy <= 5)
                            {
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    WeakReferenceMessenger.Default.Send(new LocationData { Location = location });
                                });

                                prevLocation = location;
                            }
                            else if (accuracy > 5 && accuracy <= android.accuracy_meter)
                            {
                                var calculator = new CalculateKalman(location, prevLocation);
                                var filteredLocation = calculator.Calculate();

                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    WeakReferenceMessenger.Default.Send(new LocationData { Location = filteredLocation });
                                });

                                prevLocation = filteredLocation;
                            }
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                WeakReferenceMessenger.Default.Send(new LocationData { Location = location });
                            });

                            prevLocation = location;
                        }
                    }

                    var elapsed = (DateTime.Now - loopStart).TotalMilliseconds;
                    var delay = Math.Max(0, trackingInterval - (int)elapsed);
                    await Task.Delay(delay, cancellationToken);                  
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Location tracking cancelled: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in StartTrackingAsync: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}