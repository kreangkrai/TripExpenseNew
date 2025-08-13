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

#if IOS
using UserNotifications;
#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using System.Reflection.Emit;

#endif
#if IOS
using CoreLocation;
#endif

namespace TripExpenseNew
{
    public partial class Personal : ContentPage
    {
        private ILogin Login;
        private Interface.IPersonal _Personal;
        private ITracking Tracking;
        private ILastTrip LastTrip;
        private DBInterface.IPersonal DB_Personal;
        private DBInterface.IActivePersonal ActivePersonal;
        private Location previousLocation = null;
        private Location g_location = null;
        private double totalDistance = 0;
        string emp_id = "";
        PersonalPopupStartModel start = new PersonalPopupStartModel();
        bool isStart = false;
        bool isStop = false;
        DateTime start_date = DateTime.MinValue;
        DateTime start_tracking = DateTime.MinValue;
        TrackingModel tracking = new TrackingModel();
        private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
        private ObservableCollection<PassengerItems> passengerItems = new ObservableCollection<PassengerItems>();
        int interval = 0;
        int tracking_db = 0;
#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif

        public Personal(PersonalPopupStartModel _start)
        {
            InitializeComponent();
            _Personal = new PersonalService();
            Login = new DBService.LoginService();
            start = _start;
            Tracking = new TrackingService();
            DB_Personal = new DBService.PersonalService();
            ActivePersonal = new DBService.ActivePersonalService();
            LastTrip = new LastTripService();

            WeakReferenceMessenger.Default.Register<LocationData>(this, async (send, data) =>
            {
                await UpdateLocationDataAsync(data.Location);
            });
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            OnStartTracking();
            tracking = await Tracking.GetTracking();
            interval = tracking.time_interval;
            tracking_db = tracking.time_tracking;
            start_tracking = DateTime.Now;
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
                LoginModel login = await Login.GetLogin(1);
                emp_id = login.emp_id;

                await RequestNotificationPermission();
                await SendNotification("สวัสดี", "นี่คือการแจ้งเตือนจาก MAUI!");

                previousLocation = null;
                totalDistance = 0;

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

        // หยุด service ก่อน (ถ้ายังทำงานอยู่) เพื่อให้แน่ใจว่าเริ่มใหม่ในสถานะสะอาด
        Platform.AppContext.StopService(intent);
        await Task.Delay(100); // รอให้ service หยุดสมบูรณ์
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
                        Console.WriteLine("ไม่ได้รับอนุญาต Background Location");
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
                    totalDistance += CalculateDistance(previousLocation, location);
                }
                previousLocation = location;

                double speed = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;
                PersonalModel personal = new PersonalModel();
                if (!isStart)
                {
                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                    DateTime _start = DateTime.Now;
                    start_date = _start;
                    personal = new PersonalModel()
                    {
                        driver = emp_id,
                        date = _start,
                        job_id = start.job,
                        distance = totalDistance,
                        latitude = location.Latitude,
                        longitude = location.Longitude,
                        location = start.location,
                        zipcode = zipcode,
                        location_mode = start.IsCustomer ? "CUSTOMER" : "OTHER",
                        speed = speed,
                        mileage = start.mileage,
                        trip = start_date,
                        status = "START"
                    };

                    isStart = true;
                    string message = await _Personal.Insert(personal);
                    
                    // Insert Last Trip to Server DB
                    LastTripModel lastTrip = new LastTripModel()
                    {
                        driver = personal.driver,
                        speed = personal.speed,
                        emp_id = personal.driver,
                        distance = personal.distance,
                        location = personal.location,
                        mileage = personal.mileage,
                        mode = "PERSONAL",
                        status = true,
                        trip = personal.trip,
                        car_id = personal.driver
                    };

                    string l = await LastTrip.Insert(lastTrip);

                    // Insert Active Personal to Local DB
                    ActivePersonalModel active_personal = new ActivePersonalModel()
                    {
                        driver = personal.driver,
                        distance = totalDistance,
                        location = personal.location,
                        mileage = personal.mileage,
                        status = personal.status,
                        trip = personal.trip,
                        date = DateTime.Now,
                    };

                    int act = await ActivePersonal.Insert(active_personal);

                    #region Show Active Personal
                    tripItems = new ObservableCollection<TripItems>();
                    List<ActivePersonalModel> act_personals = await ActivePersonal.GetByTrip(personal.trip);
                    foreach (var ap in act_personals)
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
                            TextDate = $"Date: {ap.date.ToString("yyyy-MM-dd HH:mm:ss")}"
                        };

                        tripItems.Add(trip_item);
                    }
                    
                    TripCollectionView.ItemsSource = tripItems;

                    #endregion
                    Console.WriteLine($"ALL ==> {message} Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}, Zipcode: {zipcode}");
                }
                else
                {
                    PersonalDBModel db_personal = new PersonalDBModel()
                    {
                        driver = emp_id,
                        date = DateTime.Now,
                        job_id = start.job,
                        distance = totalDistance,
                        latitude = location.Latitude,
                        longitude = location.Longitude,
                        location = "",
                        zipcode = "",
                        location_mode = "",
                        speed = speed,
                        mileage = 0,
                        trip = start_date,
                        status = "NA"
                    };

                    int message = await DB_Personal.Insert(db_personal);

                    int diff = (int)(DateTime.Now - start_tracking).TotalSeconds;

                    if (diff >= tracking_db)
                    {
                        List<PersonalDBModel> db_personals = new List<PersonalDBModel>();
                        db_personals = await DB_Personal.GetByTrip(start_date);

                        List<PersonalModel> personals = new List<PersonalModel>();
                        personals = db_personals.Select(s=> new PersonalModel()
                        {
                            job_id = s.job_id,
                            distance = s.distance,
                            date = s.date,
                            latitude = s.latitude,
                            longitude = s.longitude,
                            location = s.location,
                            zipcode = s.zipcode,
                            location_mode = s.location_mode,
                            speed = s.speed,
                            mileage = s.mileage,
                            trip = start_date,
                            status = s.status,
                            driver = s.driver,                           
                        }).ToList();
                        string m = await _Personal.Inserts(personals);

                        await DB_Personal.Delete(start_date);

                        personal = personals.FirstOrDefault();

                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = personal.driver,
                            speed = personal.speed,
                            emp_id = personal.driver,
                            distance = personal.distance,
                            location = personal.location,
                            mileage = personal.mileage,
                            mode = "PERSONAL",
                            status = true,
                            trip = personal.trip,
                            car_id = personal.driver
                        };

                        string l = await LastTrip.Insert(lastTrip);

                        
                        ActivePersonalModel active_personal = new ActivePersonalModel()
                        {
                            driver = personal.driver,
                            distance = totalDistance,
                            location = personal.location,
                            mileage = personal.mileage,
                            status = personal.status,
                            trip = personal.trip,
                            date = DateTime.Now,
                        };

                        int act = await ActivePersonal.Insert(active_personal);

                        #region Show Active Personal
                        tripItems = new ObservableCollection<TripItems>();
                        List<ActivePersonalModel> act_personals = await ActivePersonal.GetByTrip(personal.trip);
                        foreach (var ap in act_personals)
                        {
                            Color color = new Color();
                            if (ap.status == "START")
                            {
                                color = Color.FromRgb(255,255,255);
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
                                TextDate = $"Date: {ap.date.ToString("yyyy-MM-dd HH:mm:ss")}"
                            };

                            tripItems.Add(trip_item);
                        }

                        TripCollectionView.ItemsSource = tripItems;

                        #endregion
                        Console.WriteLine($"ALL ==> {m} Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}");
                        start_tracking = DateTime.Now;
                    }            
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DateTime now = DateTime.Now;
                    TimeSpan duration = now - start_date;
                    trip_distance.Text = totalDistance.ToString("#.#") + " km";
                    trip_duration.Text = duration.ToString(@"hh\:mm\:ss");
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

        private async void StopTripBtn_Clicked(object sender, EventArgs e)
        {
            var popup = new ProgressPopup();
            this.ShowPopup(popup);
            try
            {           
                double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                List<PersonalDBModel> db_personals = await DB_Personal.GetByTrip(start_date);
                List<PersonalModel> personals = db_personals.Select(s => new PersonalModel()
                {
                    job_id = s.job_id,
                    distance = s.distance,
                    date = s.date,
                    latitude = s.latitude,
                    longitude = s.longitude,
                    location = s.location,
                    zipcode = s.zipcode,
                    location_mode = s.location_mode,
                    speed = s.speed,
                    mileage = s.mileage,
                    trip = start_date,
                    status = s.status,
                    driver = s.driver,
                }).ToList();
                string m = await _Personal.Inserts(personals);

                await DB_Personal.Delete(start_date);

                PersonalModel personal = new PersonalModel()
                {
                    driver = emp_id,
                    date = DateTime.Now,
                    job_id = start.job,
                    distance = totalDistance,
                    latitude = g_location.Latitude,
                    longitude = g_location.Longitude,
                    location = "",
                    zipcode = zipcode,
                    location_mode = "",
                    speed = speed,
                    mileage = 12348,
                    trip = start_date,
                    status = "STOP"
                };
                string message = await _Personal.Insert(personal);

                LastTripModel lastTrip = new LastTripModel()
                {
                    driver = personal.driver,
                    speed = personal.speed,
                    emp_id = personal.driver,
                    distance = personal.distance,
                    location = personal.location,
                    mileage = personal.mileage,
                    mode = "PERSONAL",
                    status = false,
                    trip = personal.trip,
                    car_id = personal.driver
                };

                string l = await LastTrip.Insert(lastTrip);

                int act = await ActivePersonal.Delete(start_date);

                if (message != null)
                {
#if IOS
                    locationService?.StopUpdatingLocation();
                    locationService = null; // รีเซ็ต locationService
#elif ANDROID
            intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            Platform.AppContext.StopService(intent);
#endif                
                    previousLocation = null;
                    totalDistance = 0;
                    isStart = false;
                    start_date = DateTime.MinValue;
                    await Shell.Current.GoToAsync("Home_Page");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StopTripBtn_Clicked Error: {ex}");
            }
            await popup.CloseAsync();
        }

        private async void CheckInBtn_Clicked(object sender, EventArgs e)
        {
            var popup = new CheckInAlert { Title = "CHECK IN", Message = "Please Select type of check in?" };
            var result = await Shell.Current.ShowPopupAsync(popup);
            await Shell.Current.DisplayAlert("Result", $"You clicked: {result}", "OK");
        }

        private async void AddPassengerBtn_Clicked(object sender, EventArgs e)
        {
            var result = await this.ShowPopupAsync(new PersonalPassengerPopup());

            if (result != null)
            {
                if (result is EmployeeModel emp)
                {
                    //await Shell.Current.GoToAsync("Personal");
                    //await Navigation.PushAsync(new Personal(personal));
                    Console.WriteLine(result);

                    #region Show Passenger          
                    PassengerItems passengerItem = new PassengerItems()
                    {
                        TextPassenger = $"{emp.name}",
                        IconDatePassengerSource = "clock.png",
                        TextDatePassenger = $"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"
                    };

                    passengerItems.Add(passengerItem);
                    PassengerCollectionView.ItemsSource = passengerItems;

                    #endregion
                }
            }
        }

        private async void OnDropOffPassengerItemClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PassengerItems passengerItem)
            {
                bool confirm = await DisplayAlert("Confirm Drop Off", $"Drop Off: {passengerItem.TextPassenger}?", "Yes", "No");
                if (confirm)
                {
                    passengerItems.Remove(passengerItem);
                }
            }
        }
    }
}