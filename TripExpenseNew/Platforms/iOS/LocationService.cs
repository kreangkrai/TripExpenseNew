using CommunityToolkit.Mvvm.Messaging;
using CoreLocation;
using Foundation;
using Microsoft.Maui.Devices.Sensors;
using System;
using TripExpenseNew.Models;

namespace TripExpenseNew.Platforms.iOS
{
    public class LocationService : IDisposable
    {
        private CLLocationManager locationManager;
        private Action<Location> onLocationUpdate;
        private DateTime lastUpdateTime = DateTime.MinValue;
        private readonly TimeSpan updateInterval = new TimeSpan(0);
        public LocationService(int interval)
        {
            try
            {
                updateInterval = TimeSpan.FromSeconds(interval);
                locationManager = new CLLocationManager
                {
                    PausesLocationUpdatesAutomatically = false,
                    AllowsBackgroundLocationUpdates = true
                };
                locationManager.Delegate = new LocationManagerDelegate(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LocationService Initialization Error: {ex}");
                throw;
            }
        }

        public void StartUpdatingLocation(Action<Location> locationUpdateCallback)
        {
            try
            {
                onLocationUpdate = locationUpdateCallback;
                if (CLLocationManager.LocationServicesEnabled)
                {
                    locationManager.RequestWhenInUseAuthorization();
                    locationManager.RequestAlwaysAuthorization();
                    locationManager.StartUpdatingLocation();                  
                }
                else
                {
                    Console.WriteLine("Location Services are disabled on this device.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartUpdatingLocation Error: {ex}");
                throw;
            }
        }

        public void StopUpdatingLocation()
        {
            try
            {
                locationManager?.StopUpdatingLocation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StopUpdatingLocation Error: {ex}");
            }
        }

        public void Dispose()
        {
            try
            {
                locationManager?.Dispose();
                locationManager = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dispose Error: {ex}");
            }
        }

        private class LocationManagerDelegate : CLLocationManagerDelegate
        {
            private readonly LocationService service;

            public LocationManagerDelegate(LocationService service)
            {
                this.service = service;
            }

            public override async void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
            {
                try
                {
                    foreach (var loc in locations)
                    {
                        if (DateTime.Now - service.lastUpdateTime >= service.updateInterval)
                        {
                            var location = new Location(loc.Coordinate.Latitude, loc.Coordinate.Longitude)
                            {
                                Speed = loc.Speed >= 0 ? loc.Speed : null
                            };
                            service.onLocationUpdate?.Invoke(location);
                            service.lastUpdateTime = DateTime.Now;
                            //WeakReferenceMessenger.Default.Send(new LocationData { Location = location });
                            await Task.Delay((int)service.updateInterval.TotalMilliseconds);
                            //Console.WriteLine($"===== {DateTime.Now} Update ======");
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LocationsUpdated Error: {ex}");
                }
            }

            public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
            {
                //Console.WriteLine($"Authorization Changed: {status}");
            }

            public override void Failed(CLLocationManager manager, NSError error)
            {
                Console.WriteLine($"Location Manager Failed: {error}");
            }
        }
    }
}