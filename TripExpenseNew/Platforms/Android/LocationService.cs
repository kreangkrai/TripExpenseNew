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

namespace TripExpenseNew.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private CancellationTokenSource cancellationTokenSource;

        public override IBinder OnBind(Intent intent) => null;

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

                int trackingInterval = intent.GetIntExtra("TrackingInterval", 5000); // ค่าเริ่มต้น 5 วินาที
                //Console.WriteLine($"เริ่มติดตามตำแหน่งด้วยช่วงเวลา: {trackingInterval}ms");
                Task.Run(() => StartTrackingAsync(cancellationTokenSource.Token, trackingInterval));
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
                    //DateTime n = DateTime.Now;
                    //Console.WriteLine($"CancellationTokenSource ถูกยกเลิกและกำจัดแล้ว {n}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        private async Task StartTrackingAsync(CancellationToken cancellationToken,int trackingInterval)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                        var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);
                        if (location != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                WeakReferenceMessenger.Default.Send(new LocationData { Location = location });
                            });
                        }
                        else
                        {
                            Console.WriteLine("Location is null");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Inner loop error: {ex.GetType().Name} - {ex.Message}");
                    }
                    await Task.Delay(trackingInterval, cancellationToken);
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