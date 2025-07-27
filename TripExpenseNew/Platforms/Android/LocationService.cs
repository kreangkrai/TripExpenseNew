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
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channel = new NotificationChannel("location_channel", "Location Service", NotificationImportance.Low)
                    {
                        Description = "Channel for location tracking service"
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

                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => StartTrackingAsync(cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnStartCommand Error: {ex.Message}");
                StopSelf();
            }

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            base.OnDestroy();
            Console.WriteLine("LocationService stopped");
        }

        private async Task StartTrackingAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                        var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

                        if (location != null)
                        {
                            //double speedKmh = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;
                            //if (previousLocation != null)
                            //{
                            //    totalDistance += CalculateDistance(previousLocation, location);
                            //}
                            //previousLocation = location;

                            // ส่งข้อมูลไป MainPage
                            WeakReferenceMessenger.Default.Send(new LocationData { Location = location});

                            //Console.WriteLine($"Android Service ==> Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speedKmh}, Distance: {totalDistance}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"StartTrackingAsync Error: {ex.Message}");
                    }

                    await Task.Delay(3000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Location tracking cancelled");
            }
        }
    }
}