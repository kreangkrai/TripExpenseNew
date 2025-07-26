using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TripExpenseNew.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private CancellationTokenSource cancellationTokenSource;
        private Location previousLocation = null;
        private double totalDistance = 0;

        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Console.WriteLine("===============Back Ground Android====================");
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
                            double speedKmh = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;
                            if (previousLocation != null)
                            {
                                totalDistance += CalculateDistance(previousLocation, location);
                            }
                            previousLocation = location;

                            // ส่งข้อมูลไป MainPage
                            //MessagingCenter.Send(this, "LocationUpdate", new LocationData { Location = location, TotalDistance = totalDistance });

                            Console.WriteLine($"Android Service ==> Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speedKmh}, Distance: {totalDistance}");
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

        private double CalculateDistance(Location loc1, Location loc2)
        {
            double R = 6371;
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

    public class LocationData
    {
        public Location Location { get; set; }
        public double TotalDistance { get; set; }
    }
}