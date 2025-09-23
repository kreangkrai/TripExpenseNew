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
using TripExpenseNew.CustomPublicPopup;

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

namespace TripExpenseNew.PublicPage
{
    public partial class Public : ContentPage
    {
        private ILogin Login;
        private Interface.IPublic _Public;
        private ITracking Tracking;
        private ILastTrip LastTrip;
        private DBInterface.IPublic DB_Public;
        private DBInterface.IActivePublic ActivePublic;
        private IEmployee Employee;
        private ILocationCustomer LocationCustomer;
        private ILocationOther LocationOther;
        private IInternet Internet;
        private Location previousLocation = null;
        private Location g_location = null;
        string emp_id = "";
        private double totalDistance = 0;
        PublicPopupStartModel start = new PublicPopupStartModel();
        bool isStart = false;
        bool isWaitStop = false;
        bool isInactive = false;
        DateTime trip_start = DateTime.MinValue;
        DateTime start_tracking = DateTime.MinValue;
        DateTime lastInactive = DateTime.Now;
        CultureInfo cultureinfo = new CultureInfo("en-us");

        TrackingModel tracking = new TrackingModel();
        PublicModel data_public = new PublicModel();
        private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
        int interval = 0;
        int tracking_db = 0;
        List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
        List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
        List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif

        public Public(PublicPopupStartModel _start)
        {
            InitializeComponent();
            _Public = new PublicService();
            DB_Public = new DBService.PublicService();
            Login = new DBService.LoginService();
            start = _start;
            Tracking = new TrackingService();
            ActivePublic = new DBService.ActivePublicService();
            LastTrip = new LastTripService();
            Employee = new EmployeeService();
            LocationCustomer = new LocationCustomerService();
            LocationOther = new LocationOtherService();
            Internet = new InternetService();
            trip_start = start.trip_start;
            g_location = start.location;
            WeakReferenceMessenger.Default.Register<LocationData>(this, async (send, data) =>
            {
                await UpdateLocationDataAsync(data.Location);
            });
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            tracking = await Tracking.GetTracking();
            interval = tracking.time_interval;
            tracking_db = tracking.time_tracking;
            start_tracking = DateTime.Now;
            OnStartTracking();
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
                LoginModel login = await Login.GetLogin(1);
                emp_id = login.emp_id;

                //await RequestNotificationPermission();
                //await SendNotification("สวัสดี", "นี่คือการแจ้งเตือนจาก MAUI!");

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
                        bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK","Cancel");
                        if (confirm || ! confirm)
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
                int velocity_min = tracking.velocity_min;
                double speed = 0;
                if (previousLocation != null)
                {
                    DateTimeOffset start = previousLocation.Timestamp;
                    DateTimeOffset end = location.Timestamp;
                    double duration = (end - start).TotalSeconds;

                    double dist = CalculateDistance(previousLocation, location);

                    speed = ((dist * 1000) / duration) * 3.6;

                    double displacement = CalculateDistanceInactive(velocity_min, interval);
                    if (dist >= displacement)
                    {
                        totalDistance += CalculateDistance(previousLocation, location);
                    }
                }

                previousLocation = location;

                //double speed = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;

                if (!isStart)
                {
                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                    if (start.IsContinue)
                    {
                        data_public = new PublicModel()
                        {
                            date = DateTime.Now,
                            job_id = start.job_id,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            accuracy = location.Accuracy.HasValue ? location.Accuracy.Value : 10.0,
                            location = start.location_name,
                            zipcode = zipcode,
                            location_mode = start.IsCustomer ? "CUSTOMER" : "OTHER",
                            trip = start.trip,
                            status = "CONTINUE",
                            passenger = emp_id

                        };

                        isStart = true;
                        string message = await _Public.Insert(data_public);
                    }
                    else
                    {
                        data_public = new PublicModel()
                        {
                            passenger = emp_id,
                            date = DateTime.Now,
                            job_id = start.job_id,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            accuracy = location.Accuracy.HasValue ? location.Accuracy.Value : 10.0,
                            location = start.location_name,
                            zipcode = zipcode,
                            location_mode = start.IsCustomer ? "CUSTOMER" : "OTHER",
                            trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                            status = "START"
                        };

                        isStart = true;
                        string message = await _Public.Insert(data_public);

                        // Insert Last Trip to Server DB
                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = "",
                            speed = 0,
                            job_id = data_public.job_id,
                            emp_id = data_public.passenger,
                            trip_start = trip_start,
                            date = DateTime.Now,
                            distance = 0,
                            location = data_public.location,
                            latitude = data_public.latitude,
                            longitude = data_public.longitude,
                            accuracy = data_public.accuracy,
                            mileage_start = 0,
                            mileage_stop = 0,
                            mode = "PUBLIC",
                            status = true,
                            trip = data_public.trip,
                            car_id = "",
                            borrower_id = ""
                        };

                        message = await LastTrip.Insert(lastTrip);

                        #region Add Location
                        if (start.location_name != "" && start.location_name != "CTL(HQ)" && start.location_name != "CTL(KBO)" && start.location_name != "CTL(RBO)")
                        {
                            if (start.IsCustomer)
                            {
                                LocationCustomerModel locationCustomer = new LocationCustomerModel()
                                {
                                    emp_id = emp_id,
                                    latitude = location.Latitude,
                                    longitude = location.Longitude,
                                    location = start.location_name,
                                    location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                    zipcode = zipcode,
                                };
                                await LocationCustomer.Insert(locationCustomer);
                            }
                            else
                            {
                                LocationOtherModel locationOther = new LocationOtherModel()
                                {
                                    emp_id = emp_id,
                                    latitude = location.Latitude,
                                    longitude = location.Longitude,
                                    location = start.location_name,
                                    location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                    zipcode = zipcode,
                                };

                                await LocationOther.Insert(locationOther);
                            }
                        }
                        
                        #endregion
                    }


                    // Insert Active Public to Local DB
                    ActivePublicModel active_public = new ActivePublicModel()
                    {
                        passenger = data_public.passenger,
                        location = data_public.location,
                        status = data_public.status,
                        trip = data_public.trip,
                        date = DateTime.Now
                    };

                    int act = await ActivePublic.Insert(active_public);

                    #region Show Active Public
                    tripItems = new ObservableCollection<TripItems>();
                    List<ActivePublicModel> act_publics = await ActivePublic.GetByTrip(data_public.trip);
                    foreach (var ap in act_publics)
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
                    //Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}, Zipcode: {zipcode}");                   
                }
                else
                {

                    if (!isWaitStop)
                    {
                        //INACTIVE

                        double dist = CalculateDistance(g_location, location);
                        double displacement = CalculateDistanceInactive(velocity_min, interval);
                        if (dist < displacement)  // Check ditance beteween point to point less than displacement
                        {
                            int minute_inactive = (int)(DateTime.Now - lastInactive).TotalMinutes;
                            if (minute_inactive >= 15)  // Inactive Each 15 Minute
                            {
                                if (!isInactive)
                                {
                                    PublicModel p = new PublicModel()
                                    {
                                        passenger = emp_id,
                                        date = DateTime.Now,
                                        job_id = start.job_id,
                                        distance = totalDistance,
                                        latitude = location.Latitude,
                                        longitude = location.Longitude,
                                        accuracy = location.Accuracy.HasValue ? location.Accuracy.Value : 10.0,
                                        location = "",
                                        zipcode = "",
                                        location_mode = "",
                                        speed = speed,
                                        trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                        status = "INACTIVE"
                                    };
                                    string message = await _Public.Insert(p);

                                    LastTripModel lastTrip = new LastTripModel()
                                    {
                                        driver = "",
                                        speed = p.speed,
                                        job_id = p.job_id,
                                        emp_id = p.passenger,
                                        trip_start = trip_start,
                                        date = DateTime.Now,
                                        distance = p.distance,
                                        location = p.location,
                                        latitude = p.latitude,
                                        longitude = p.longitude,
                                        accuracy = p.accuracy,
                                        mileage_start = 0,
                                        mileage_stop = 0,
                                        mode = "PUBLIC",
                                        status = true,
                                        trip = p.trip,
                                        car_id = "",
                                        borrower_id = ""

                                    };

                                    string l = await LastTrip.UpdateByTrip(lastTrip);

                                    ActivePublicModel active_public = new ActivePublicModel()
                                    {
                                        passenger = p.passenger,
                                        distance = totalDistance,
                                        location = p.location,
                                        status = p.status,
                                        trip = p.trip,
                                        date = DateTime.Now
                                    };

                                    int act = await ActivePublic.Insert(active_public);

                                    isInactive = true;
                                }
                            }
                        }

                        else
                        {
                            PublicDBModel db_public = new PublicDBModel()
                            {
                                passenger = emp_id,
                                date = DateTime.Now,
                                job_id = start.job_id,
                                distance = totalDistance,
                                latitude = location.Latitude,
                                longitude = location.Longitude,
                                accuracy = location.Accuracy.HasValue ? location.Accuracy.Value : 10.0,
                                location = "",
                                zipcode = "",
                                location_mode = "",
                                speed = speed,
                                trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                status = "NA"
                            };

                            int message = await DB_Public.Insert(db_public);

                            int diff = (int)(DateTime.Now - start_tracking).TotalSeconds;

                            if (diff >= tracking_db)
                            {
                                List<PublicDBModel> db_publics = new List<PublicDBModel>();
                                db_publics = await DB_Public.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                List<PublicModel> publics = new List<PublicModel>();
                                publics = db_publics.Select(s => new PublicModel()
                                {
                                    job_id = s.job_id,
                                    distance = s.distance,
                                    date = s.date,
                                    latitude = s.latitude,
                                    longitude = s.longitude,
                                    location = s.location,
                                    accuracy = s.accuracy,
                                    zipcode = s.zipcode,
                                    location_mode = s.location_mode,
                                    speed = s.speed,
                                    trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                    status = s.status,
                                    passenger = s.passenger
                                }).ToList();
                                string m = await _Public.Inserts(publics);

                                await DB_Public.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                PublicModel _public = publics.FirstOrDefault();

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = "",
                                    speed = _public.speed,
                                    job_id = _public.job_id,
                                    emp_id = _public.passenger,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = _public.distance,
                                    location = _public.location,
                                    latitude = _public.latitude,
                                    longitude = _public.longitude,
                                    accuracy = _public.accuracy,
                                    mileage_start = 0,
                                    mileage_stop = 0,
                                    mode = "PUBLIC",
                                    status = true,
                                    trip = _public.trip,
                                    car_id = "",
                                    borrower_id = ""
                                };

                                string l = await LastTrip.UpdateByTrip(lastTrip);

                                #region Show Active Public
                                tripItems = new ObservableCollection<TripItems>();
                                List<ActivePublicModel> act_publics = await ActivePublic.GetByTrip(_public.trip);
                                foreach (var ap in act_publics)
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
                                //Console.WriteLine($"ALL ==> {m} Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}");
                                start_tracking = DateTime.Now;
                            }

                            lastInactive = DateTime.Now;
                            isInactive = false;
                        }
                    }
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
            StopTripBtn.IsEnabled = false;
            try
            {
                bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
                if (internet)
                {
                    isWaitStop = true;

                    #region Find Location
                    GetLocationCTL.Add(new LocationOtherModel()
                    {
                        location = "CTL(HQ)",
                        latitude = 13.729175,
                        longitude = 100.728538
                    });
                    GetLocationCTL.Add(new LocationOtherModel()
                    {
                        location = "CTL(RBO)",
                        latitude = 12.718476,
                        longitude = 101.162984
                    });
                    GetLocationCTL.Add(new LocationOtherModel()
                    {
                        location = "CTL(KBO)",
                        latitude = 16.444429,
                        longitude = 102.794939
                    });


                    LoginModel login = await Login.GetLogin(1);
                    GetLocationCustomers = await LocationCustomer.GetByEmp(login.emp_id);
                    GetLocationOthers = await LocationOther.GetByEmp(login.emp_id);

                    FindLocationService findLocation = new FindLocationService();
                    Tuple<string, bool> loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, g_location);

                    #endregion
                    var result = await this.ShowPopupAsync(new PublicStopPopup(loc.Item1, loc.Item2));

                    if (result != null)
                    {
                        if (result is PublicPopupStopModel _public)
                        {
                            if (_public.location != null && _public.location != "")
                            {
                                var popup = new ProgressPopup();
                                this.ShowPopup(popup);
                                double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                                var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                                List<PublicDBModel> db_publics = await DB_Public.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                List<PublicModel> publics = db_publics.Select(s => new PublicModel()
                                {
                                    job_id = s.job_id,
                                    distance = s.distance,
                                    date = s.date,
                                    latitude = s.latitude,
                                    longitude = s.longitude,
                                    accuracy = s.accuracy,
                                    location = _public.location,
                                    zipcode = s.zipcode,
                                    location_mode = _public.IsCustomer ? "CUSTOMER" : "OTHER",
                                    speed = s.speed,
                                    trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                    status = s.status,
                                    passenger = s.passenger
                                }).ToList();

                                string message = await _Public.Inserts(publics);
                                if (message == "Success")
                                {
                                    await DB_Public.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                    data_public = new PublicModel()
                                    {
                                        passenger = emp_id,
                                        date = DateTime.Now,
                                        job_id = start.job_id,
                                        distance = totalDistance,
                                        latitude = g_location.Latitude,
                                        longitude = g_location.Longitude,
                                        accuracy = g_location.Accuracy.HasValue ? g_location.Accuracy.Value : 10.0,
                                        location = _public.location,
                                        zipcode = zipcode,
                                        location_mode = _public.IsCustomer ? "CUSTOMER" : "OTHER",
                                        speed = speed,
                                        trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                        status = "STOP"
                                    };
                                    message = await _Public.Insert(data_public);

                                    if (message == "Success")
                                    {
                                        LastTripModel lastTrip = new LastTripModel()
                                        {
                                            driver = "",
                                            speed = data_public.speed,
                                            emp_id = data_public.passenger,
                                            job_id = data_public.job_id,
                                            trip_start = trip_start,
                                            date = data_public.date,
                                            distance = data_public.distance,
                                            location = data_public.location,
                                            latitude = data_public.latitude,
                                            longitude = data_public.longitude,
                                            accuracy = data_public.accuracy,
                                            mileage_start = 0,
                                            mileage_stop = 0,
                                            mode = "PUBLIC",
                                            status = false,
                                            trip = data_public.trip,
                                            car_id = "",
                                            borrower_id = ""
                                        };

                                        message = await LastTrip.UpdateByTrip(lastTrip);

                                        int act = await ActivePublic.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                    }
                                }

                                #region Add Location

                                if (loc.Item1 != _public.location && _public.location != "CTL(HQ)" && _public.location != "CTL(KBO)" && _public.location != "CTL(RBO)") // Insert New Location
                                {
                                    if (_public.IsCustomer)
                                    {
                                        LocationCustomerModel locationCustomer = new LocationCustomerModel()
                                        {
                                            emp_id = emp_id,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = _public.location,
                                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                            zipcode = zipcode,
                                        };
                                        await LocationCustomer.Insert(locationCustomer);
                                    }
                                    else
                                    {
                                        LocationOtherModel locationOther = new LocationOtherModel()
                                        {
                                            emp_id = emp_id,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = _public.location,
                                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                            zipcode = zipcode,
                                        };
                                        await LocationOther.Insert(locationOther);
                                    }
                                }
                                #endregion

                                #region Stop
#if IOS
                                    locationService?.StopUpdatingLocation();
                                    locationService = null; // รีเซ็ต locationService
#elif ANDROID
            intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            Platform.AppContext.StopService(intent);
#endif
                                #endregion

                                previousLocation = null;
                                totalDistance = 0;
                                trip_start = DateTime.MinValue;
                                await Shell.Current.GoToAsync("Home_Page");

                                await popup.CloseAsync();
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
            StopTripBtn.IsEnabled = true;
        }

        private async void CheckInBtn_Clicked(object sender, EventArgs e)
        {
            CheckInBtn.IsEnabled = false;
            try
            {
                var popup = new PublicCheckInAlert { Title = "CHECK IN", Message = "Please Select type of check in?" };
                var result = await Shell.Current.ShowPopupAsync(popup);

                if (result != null)
                {
                    bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
                    if (internet)
                    {
                        #region Find Location
                        GetLocationCTL.Add(new LocationOtherModel()
                        {
                            location = "CTL(HQ)",
                            latitude = 13.729175,
                            longitude = 100.728538
                        });
                        GetLocationCTL.Add(new LocationOtherModel()
                        {
                            location = "CTL(RBO)",
                            latitude = 12.718476,
                            longitude = 101.162984
                        });
                        GetLocationCTL.Add(new LocationOtherModel()
                        {
                            location = "CTL(KBO)",
                            latitude = 16.444429,
                            longitude = 102.794939
                        });


                        LoginModel login = await Login.GetLogin(1);
                        GetLocationCustomers = await LocationCustomer.GetByEmp(login.emp_id);
                        GetLocationOthers = await LocationOther.GetByEmp(login.emp_id);

                        FindLocationService findLocation = new FindLocationService();
                        Tuple<string, bool> loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, g_location);

                        #endregion

                        double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                        var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                        var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                        string chkinlocation = "";
                        string location_mode = "";

                        bool isChkIn = false;
                        if (result.ToString() == "Customer")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_customer = await this.ShowPopupAsync(new PublicCheckinCustomerPopup(chkinlocation));

                            if (result_customer != null)
                            {
                                if (result_customer.ToString().Trim() != "")
                                {
                                    chkinlocation = result_customer.ToString();
                                    isChkIn = true;

                                    if (loc.Item1 != result_customer.ToString() && result_customer.ToString() != "CTL(HQ)" && result_customer.ToString() != "CTL(KBO)" && result_customer.ToString() != "CTL(RBO)") // Insert New Location Customer
                                    {
                                        LocationCustomerModel locationCustomer = new LocationCustomerModel()
                                        {
                                            emp_id = emp_id,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = result_customer.ToString(),
                                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                            zipcode = zipcode,
                                        };
                                        await LocationCustomer.Insert(locationCustomer);
                                    }
                                }
                            }
                            location_mode = "CUSTOMER";
                        }

                        if (result.ToString() == "Other")
                        {
                            if (loc.Item2 == false)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_other = await this.ShowPopupAsync(new PublicCheckinOtherPopup(chkinlocation));

                            if (result_other != null)
                            {
                                if (result_other.ToString().Trim() != "")
                                {
                                    chkinlocation = result_other.ToString();
                                    isChkIn = true;

                                    if (loc.Item1 != result_other.ToString() && result_other.ToString() != "CTL(HQ)" && result_other.ToString() != "CTL(KBO)" && result_other.ToString() != "CTL(RBO)")
                                    {
                                        LocationOtherModel locationOther = new LocationOtherModel()
                                        {
                                            emp_id = emp_id,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = result_other.ToString(),
                                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                            zipcode = zipcode,
                                        };
                                        await LocationOther.Insert(locationOther);
                                    }
                                }
                            }
                            location_mode = "OTHER";
                        }
                      
                        if (isChkIn)
                        {
                            data_public = new PublicModel()
                            {
                                passenger = emp_id,
                                date = DateTime.Now,
                                job_id = start.job_id,
                                distance = totalDistance,
                                latitude = g_location.Latitude,
                                longitude = g_location.Longitude,
                                accuracy = g_location.Accuracy.HasValue ? g_location.Accuracy.Value : 10.0,
                                location = chkinlocation,
                                zipcode = zipcode,
                                location_mode = location_mode,
                                speed = speed,
                                trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                status = "CHECK IN"
                            };

                            string message = await _Public.Insert(data_public);

                            if (message == "Success")
                            {
                                ActivePublicModel active_public = new ActivePublicModel()
                                {
                                    passenger = data_public.passenger,
                                    distance = data_public.distance,
                                    location = data_public.location,
                                    status = data_public.status,
                                    trip = data_public.trip,
                                    date = data_public.date
                                };

                                int act = await ActivePublic.Insert(active_public);

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = "",
                                    speed = data_public.speed,
                                    job_id = data_public.job_id,
                                    emp_id = data_public.passenger,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = data_public.distance,
                                    location = data_public.location,
                                    latitude = data_public.latitude,
                                    longitude = data_public.longitude,
                                    accuracy = data_public.accuracy,
                                    mileage_start = 0,
                                    mileage_stop = 0,
                                    mode = "PUBLIC",
                                    status = true,
                                    trip = data_public.trip,
                                    car_id = "",
                                    borrower_id = ""
                                };

                                message = await LastTrip.UpdateByTrip(lastTrip);

                            }

                            #region Show Active Public
                            tripItems = new ObservableCollection<TripItems>();
                            List<ActivePublicModel> act_publics = await ActivePublic.GetByTrip(data_public.trip);
                            foreach (var ap in act_publics)
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

            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", ex.Message, "OK");
                });
            }
            CheckInBtn.IsEnabled = true;
        }
    }
}