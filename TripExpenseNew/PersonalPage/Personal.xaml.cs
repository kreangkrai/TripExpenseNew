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

namespace TripExpenseNew.PersonalPage
{
    public partial class Personal : ContentPage
    {
        private ILogin Login;
        private Interface.IPersonal _Personal;
        private ITracking Tracking;
        private ILastTrip LastTrip;
        private DBInterface.IPersonal DB_Personal;
        private DBInterface.IActivePersonal ActivePersonal;
        private IPassengerPersonal PassengerPersonal;
        private IEmployee Employee;
        private ILocationCustomer LocationCustomer;
        private ILocationOther LocationOther;
        private IMileage Mileage;
        private IInternet Internet;
        private Location previousLocation = null;
        private Location g_location = null;
        private double totalDistance = 0;
        string emp_id = "";
        private int mileage_start = 0;
        PersonalPopupStartModel start = new PersonalPopupStartModel();
        bool isStart = false;
        bool isWaitStop = false;
        DateTime trip_start = DateTime.MinValue;
        DateTime start_tracking = DateTime.MinValue;
        DateTime lastInactive = DateTime.Now;
        CultureInfo cultureinfo = new CultureInfo("en-us");

        TrackingModel tracking = new TrackingModel();
        PersonalModel data_personal = new PersonalModel();
        private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
        private ObservableCollection<PassengerItems> passengerItems = new ObservableCollection<PassengerItems>();
        int interval = 0;
        int tracking_db = 0;
        bool isInactive = false;
        List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
        List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
        List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();
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
            PassengerPersonal = new PassengerPersonalService();
            Employee = new EmployeeService();
            LocationCustomer = new LocationCustomerService();
            LocationOther = new LocationOtherService();
            Mileage = new DBService.MileageService();
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
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            tracking = await Tracking.GetTracking();
            interval = tracking.time_interval;
            tracking_db = tracking.time_tracking;
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

                if (previousLocation != null)
                {
                    double dist = CalculateDistance(previousLocation, location);
                    double displacement = CalculateDistanceInactive(40.0, interval);
                    if (dist >= displacement)
                    {
                        totalDistance += CalculateDistance(previousLocation, location);
                    }
                }
                previousLocation = location;

                double speed = location.Speed.HasValue ? location.Speed.Value * 3.6 : 0;

                if (!isStart)
                {
                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                    if (start.IsContinue)
                    {
                        data_personal = new PersonalModel()
                        {
                            driver = emp_id,
                            date = DateTime.Now,
                            job_id = start.job_id,
                            distance = totalDistance,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            location = start.location_name,
                            zipcode = zipcode,
                            location_mode = start.IsCustomer ? "CUSTOMER" : "OTHER",
                            speed = speed,
                            mileage = start.mileage,
                            trip = start.trip,
                            status = "CONTINUE",
                            cash = 0
                        };

                        isStart = true;
                        string message = await _Personal.Insert(data_personal);


                        #region Show Passenger
                        List<LastTripViewModel> last_trip = await LastTrip.GetByTrip(start.trip);
                        last_trip = last_trip.Where(w => w.emp_id != emp_id && w.status == true).ToList();
                        if (last_trip.Count > 0)
                        {
                            for (int i = 0; i < last_trip.Count; i++)
                            {
                                PassengerItems passengerItem = new PassengerItems()
                                {
                                    TextPassenger = $"{last_trip[i].emp_name}",
                                    IconDatePassengerSource = "clock.png",
                                    TextDatePassenger = $"Date: {last_trip[i].date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                                };

                                passengerItems.Add(passengerItem);
                            }
                            PassengerCollectionView.ItemsSource = passengerItems;
                            frame_passenger.IsVisible = true;
                            Current_Passenger.Text = $"Current Passenger : ({passengerItems.Count})";
                        }
                        #endregion

                    }
                    else
                    {
                        data_personal = new PersonalModel()
                        {
                            driver = emp_id,
                            date = DateTime.Now,
                            job_id = start.job_id,
                            distance = totalDistance,
                            latitude = location.Latitude,
                            longitude = location.Longitude,
                            location = start.location_name,
                            zipcode = zipcode,
                            location_mode = start.IsCustomer ? "CUSTOMER" : "OTHER",
                            speed = speed,
                            mileage = start.mileage,
                            trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                            status = "START",
                            cash = 0
                        };

                        isStart = true;
                        string message = await _Personal.Insert(data_personal);

                        // Insert Last Trip to Server DB
                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = data_personal.driver,
                            speed = data_personal.speed,
                            job_id = data_personal.job_id,
                            emp_id = data_personal.driver,
                            trip_start = trip_start,
                            date = DateTime.Now,
                            distance = data_personal.distance,
                            location = data_personal.location,
                            latitude = data_personal.latitude,
                            longitude = data_personal.longitude,
                            mileage_start = mileage_start,
                            mileage_stop = 0,
                            mode = "PERSONAL",
                            status = true,
                            trip = data_personal.trip,
                            car_id = data_personal.driver
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
                        else
                        {
                            double dist = CalculateDistance(start.location, location);
                            if (dist > 0.1)
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
                        }

                        #endregion
                    }


                    // Insert Active Personal to Local DB
                    ActivePersonalModel active_personal = new ActivePersonalModel()
                    {
                        driver = data_personal.driver,
                        distance = totalDistance,
                        location = data_personal.location,
                        mileage = data_personal.mileage,
                        status = data_personal.status,
                        trip = data_personal.trip,
                        date = DateTime.Now,
                    };

                    int act = await ActivePersonal.Insert(active_personal);

                    #region Show Active Personal
                    tripItems = new ObservableCollection<TripItems>();
                    List<ActivePersonalModel> act_personals = await ActivePersonal.GetByTrip(data_personal.trip);
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
                            TextDate = $"Date: {ap.date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                        };

                        tripItems.Add(trip_item);
                    }

                    TripCollectionView.ItemsSource = tripItems;

                    #endregion

                    Text_Detail.Text = $"Active Trip Detail : ({tripItems.Count})";
                    Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}, Zipcode: {zipcode}");
                }
                else
                {

                    if (!isWaitStop)
                    {
                        //INACTIVE

                        double dist = CalculateDistance(g_location, location);
                        double displacement = CalculateDistanceInactive(40.0, interval);
                        if (dist < displacement)  // Check ditance beteween point to point less than displacement
                        {
                            int minute_inactive = (int)(DateTime.Now - lastInactive).TotalMinutes;
                            if (minute_inactive >= 30)  // Inactive Each 2 Minute
                            {
                                if (!isInactive)
                                {
                                    PersonalModel personal = new PersonalModel()
                                    {
                                        driver = emp_id,
                                        date = DateTime.Now,
                                        job_id = start.job_id,
                                        distance = totalDistance,
                                        latitude = location.Latitude,
                                        longitude = location.Longitude,
                                        location = "",
                                        zipcode = "",
                                        location_mode = "",
                                        speed = speed,
                                        mileage = start.mileage,
                                        trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                        status = "INACTIVE",
                                        cash = 0
                                    };
                                    string message = await _Personal.Insert(personal);

                                    LastTripModel lastTrip = new LastTripModel()
                                    {
                                        driver = personal.driver,
                                        speed = personal.speed,
                                        job_id = personal.job_id,
                                        emp_id = personal.driver,
                                        trip_start = trip_start,
                                        date = DateTime.Now,
                                        distance = personal.distance,
                                        location = personal.location,
                                        latitude = personal.latitude,
                                        longitude = personal.longitude,
                                        mileage_start = mileage_start,
                                        mileage_stop = 0,
                                        mode = "PERSONAL",
                                        status = true,
                                        trip = personal.trip,
                                        car_id = personal.driver
                                    };

                                    string l = await LastTrip.UpdateByTrip(lastTrip);

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

                                    isInactive = true;
                                }
                            }
                        }

                        else
                        {
                            PersonalDBModel db_personal = new PersonalDBModel()
                            {
                                driver = emp_id,
                                date = DateTime.Now,
                                job_id = start.job_id,
                                distance = totalDistance,
                                latitude = location.Latitude,
                                longitude = location.Longitude,
                                location = "",
                                zipcode = "",
                                location_mode = "",
                                speed = speed,
                                mileage = start.mileage,
                                trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                status = "NA",
                                cash = 0
                            };

                            int message = await DB_Personal.Insert(db_personal);

                            int diff = (int)(DateTime.Now - start_tracking).TotalSeconds;

                            if (diff >= tracking_db)
                            {
                                List<PersonalDBModel> db_personals = new List<PersonalDBModel>();
                                db_personals = await DB_Personal.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                List<PersonalModel> personals = new List<PersonalModel>();
                                personals = db_personals.Select(s => new PersonalModel()
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
                                    trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                    status = s.status,
                                    driver = s.driver,
                                    cash = s.cash,
                                }).ToList();
                                string m = await _Personal.Inserts(personals);

                                await DB_Personal.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                PersonalModel personal = personals.FirstOrDefault();

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = personal.driver,
                                    speed = personal.speed,
                                    job_id = personal.job_id,
                                    emp_id = personal.driver,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = personal.distance,
                                    location = personal.location,
                                    latitude = personal.latitude,
                                    longitude = personal.longitude,
                                    mileage_start = mileage_start,
                                    mileage_stop = 0,
                                    mode = "PERSONAL",
                                    status = true,
                                    trip = personal.trip,
                                    car_id = personal.driver
                                };

                                string l = await LastTrip.UpdateByTrip(lastTrip);

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
                                        TextDate = $"Date: {ap.date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                                    };

                                    tripItems.Add(trip_item);
                                }

                                TripCollectionView.ItemsSource = tripItems;

                                #endregion
                                Console.WriteLine($"ALL ==> {m} Lat: {location.Latitude}, Lon: {location.Longitude}, Speed: {speed}, Distance: {totalDistance}");
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

        private double CalculateDistanceInactive(double velocity , int duration)
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
                    var result = await this.ShowPopupAsync(new PersonalStopPopup(loc.Item1, loc.Item2, start.mileage));

                    if (result != null)
                    {
                        if (result is PersonalPopupStopModel personal)
                        {
                            if (personal.location != null && personal.location != "" && personal.mileage != 0)
                            {
                                if (personal.mileage >= start.mileage)
                                {
                                    var popup = new ProgressPopup();
                                    this.ShowPopup(popup);
                                    double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                                    var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                                    List<PersonalDBModel> db_personals = await DB_Personal.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                    List<PersonalModel> personals = db_personals.Select(s => new PersonalModel()
                                    {
                                        job_id = s.job_id,
                                        distance = s.distance,
                                        date = s.date,
                                        latitude = s.latitude,
                                        longitude = s.longitude,
                                        location = personal.location,
                                        zipcode = s.zipcode,
                                        location_mode = personal.IsCustomer ? "CUSTOMER" : "OTHER",
                                        speed = s.speed,
                                        mileage = personal.mileage,
                                        trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                        status = s.status,
                                        driver = s.driver,
                                        cash = s.cash,
                                    }).ToList();

                                    string message = await _Personal.Inserts(personals);
                                    if (message == "Success")
                                    {
                                        await DB_Personal.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                        data_personal = new PersonalModel()
                                        {
                                            driver = emp_id,
                                            date = DateTime.Now,
                                            job_id = start.job_id,
                                            distance = totalDistance,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = personal.location,
                                            zipcode = zipcode,
                                            location_mode = personal.IsCustomer ? "CUSTOMER" : "OTHER",
                                            speed = speed,
                                            mileage = personal.mileage,
                                            trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                            status = "STOP",
                                            cash = 0
                                        };
                                        message = await _Personal.Insert(data_personal);

                                        if (message == "Success")
                                        {
                                            LastTripModel lastTrip = new LastTripModel()
                                            {
                                                driver = data_personal.driver,
                                                speed = data_personal.speed,
                                                emp_id = data_personal.driver,
                                                job_id = data_personal.job_id,
                                                trip_start = trip_start,
                                                date = data_personal.date,
                                                distance = data_personal.distance,
                                                location = data_personal.location,
                                                latitude = data_personal.latitude,
                                                longitude = data_personal.longitude,
                                                mileage_start = mileage_start,
                                                mileage_stop = data_personal.mileage,
                                                mode = "PERSONAL",
                                                status = false,
                                                trip = data_personal.trip,
                                                car_id = data_personal.driver
                                            };

                                            message = await LastTrip.UpdateByTrip(lastTrip);

                                            int act = await ActivePersonal.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                        }
                                    }

                                    #region Add Location

                                    if (loc.Item1 != personal.location && personal.location != "CTL(HQ)" && personal.location != "CTL(KBO)" && personal.location != "CTL(RBO)") // Insert New Location
                                    {
                                        if (personal.IsCustomer)
                                        {
                                            LocationCustomerModel locationCustomer = new LocationCustomerModel()
                                            {
                                                emp_id = emp_id,
                                                latitude = g_location.Latitude,
                                                longitude = g_location.Longitude,
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
                                                latitude = g_location.Latitude,
                                                longitude = g_location.Longitude,
                                                location = start.location_name,
                                                location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff", cultureinfo),
                                                zipcode = zipcode,
                                            };
                                            await LocationOther.Insert(locationOther);
                                        }
                                    }
                                    #endregion

                                    #region GET PASSENGER
                                    //CultureInfo usCulture = new CultureInfo("en-US");
                                    List<PassengerPersonalViewModel> passenger_personals = await PassengerPersonal.GetPassengerPersonalByDriver(data_personal.driver, data_personal.trip);

                                    List<string> emp_list = passenger_personals.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();
                                    List<string> emps = passenger_personals.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger).ToList();
                                    emps = emps.Distinct().ToList();

                                    if (emps.Count > 0)
                                    {
                                        #region ADD PASSENGER
                                        for (int i = 0; i < emps.Count; i++)
                                        {
                                            PassengerPersonalModel passengerPersonal = new PassengerPersonalModel()
                                            {
                                                date = data_personal.date,
                                                driver = data_personal.driver,
                                                trip = data_personal.trip,
                                                job_id = data_personal.job_id,
                                                latitude = data_personal.latitude,
                                                longitude = data_personal.longitude,
                                                location = data_personal.location,
                                                location_mode = data_personal.location_mode,
                                                passenger = emps[i],
                                                status = "STOP",
                                                zipcode = data_personal.zipcode
                                            };
                                            string mes = await PassengerPersonal.Insert(passengerPersonal);

                                            LastTripModel lastTrip_passenger = new LastTripModel()
                                            {
                                                driver = data_personal.driver,
                                                speed = 0,
                                                emp_id = emps[i],
                                                job_id = data_personal.job_id,
                                                trip_start = trip_start,
                                                date = DateTime.Now,
                                                distance = 0,
                                                location = data_personal.location,
                                                latitude = data_personal.latitude,
                                                longitude = data_personal.longitude,
                                                mileage_start = 0,
                                                mileage_stop = 0,
                                                mode = "PASSENGER PERSONAL",
                                                status = false,
                                                trip = data_personal.trip,
                                                car_id = ""
                                            };

                                            mes = await LastTrip.UpdateByTrip(lastTrip_passenger);
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    #region Update Last Mileage
                                    MileageDBModel db_mileage = new MileageDBModel()
                                    {
                                        Id = 1,
                                        mileage = personal.mileage
                                    };
                                    int id = await Mileage.Save(db_mileage);
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
                                    isStart = false;
                                    trip_start = DateTime.MinValue;
                                    await Shell.Current.GoToAsync("Home_Page");

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
                var popup = new CheckInAlert { Title = "CHECK IN", Message = "Please Select type of check in?" };
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
                        double cash = 0;
                        string location_mode = "";

                        bool isChkIn = false;
                        if (result.ToString() == "Customer")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_customer = await this.ShowPopupAsync(new PersonalCheckinCustomerPopup(chkinlocation));

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
                            var result_other = await this.ShowPopupAsync(new PersonalCheckinOtherPopup(chkinlocation));

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

                        if (result.ToString() == "Gas Station")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_gas = await this.ShowPopupAsync(new PersonalCheckinGasPopup());

                            if (result_gas != null)
                            {
                                if (result_gas is Tuple<string, double> data)
                                {
                                    chkinlocation = data.Item1;
                                    cash = data.Item2;
                                    isChkIn = true;
                                }
                            }
                            else
                            {
                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("", "กรุณากรอกข้อมูล", "OK");
                                });
                            }
                            location_mode = "GAS";
                        }

                        if (isChkIn)
                        {
                            PersonalModel personal = new PersonalModel()
                            {
                                driver = emp_id,
                                date = DateTime.Now,
                                job_id = start.job_id,
                                distance = totalDistance,
                                latitude = g_location.Latitude,
                                longitude = g_location.Longitude,
                                location = chkinlocation,
                                zipcode = zipcode,
                                location_mode = location_mode,
                                speed = speed,
                                mileage = start.mileage,
                                trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                status = "CHECK IN",
                                cash = cash
                            };

                            string message = await _Personal.Insert(personal);

                            if (message == "Success")
                            {
                                ActivePersonalModel active_personal = new ActivePersonalModel()
                                {
                                    driver = personal.driver,
                                    distance = personal.distance,
                                    location = personal.location,
                                    mileage = personal.mileage,
                                    status = personal.status,
                                    trip = personal.trip,
                                    date = personal.date,
                                };

                                int act = await ActivePersonal.Insert(active_personal);

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = personal.driver,
                                    speed = personal.speed,
                                    job_id = personal.job_id,
                                    emp_id = personal.driver,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = personal.distance,
                                    location = personal.location,
                                    latitude = personal.latitude,
                                    longitude = personal.longitude,
                                    mileage_start = mileage_start,
                                    mileage_stop = 0,
                                    mode = "PERSONAL",
                                    status = true,
                                    trip = personal.trip,
                                    car_id = personal.driver
                                };

                                message = await LastTrip.UpdateByTrip(lastTrip);

                            }


                            #region GET PASSENGER
                            //CultureInfo usCulture = new CultureInfo("en-US");
                            List<PassengerPersonalViewModel> passenger_personals = await PassengerPersonal.GetPassengerPersonalByDriver(personal.driver, personal.trip);

                            List<string> emp_list = passenger_personals.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();

                            List<string> emps = passenger_personals.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger).ToList();
                            emps = emps.Distinct().ToList();

                            if (emps.Count > 0)
                            {
                                for (int i = 0; i < emps.Count; i++)
                                {
                                    PassengerPersonalModel passengerPersonal = new PassengerPersonalModel()
                                    {
                                        date = personal.date,
                                        driver = personal.driver,
                                        trip = personal.trip,
                                        job_id = personal.job_id,
                                        latitude = personal.latitude,
                                        longitude = personal.longitude,
                                        location = personal.location,
                                        location_mode = personal.location_mode,
                                        passenger = emps[i],
                                        status = "CHECK IN",
                                        zipcode = personal.zipcode
                                    };
                                    message = await PassengerPersonal.Insert(passengerPersonal);

                                    if (message == "Success")
                                    {
                                        LastTripModel lastTrip_passenger = new LastTripModel()
                                        {
                                            driver = emp_id,
                                            speed = 0,
                                            emp_id = emps[i],
                                            job_id = personal.job_id,
                                            trip_start = trip_start,
                                            date = DateTime.Now,
                                            distance = 0,
                                            location = personal.location,
                                            latitude = personal.latitude,
                                            longitude = personal.longitude,
                                            mileage_start = 0,
                                            mileage_stop = 0,
                                            mode = "PASSENGER PERSONAL",
                                            status = true,
                                            trip = personal.trip,
                                            car_id = ""
                                        };

                                        message = await LastTrip.UpdateByTrip(lastTrip_passenger);
                                    }
                                }
                            }

                            #endregion

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
            catch(Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", ex.Message, "OK");
                });
            }

        }
        private async void AddPassengerBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                var result = await this.ShowPopupAsync(new PersonalPassengerPopup());

                if (result != null)
                {
                    if (result is EmployeeModel emp)
                    {
                        #region Show Passenger          
                        PassengerItems passengerItem = new PassengerItems()
                        {
                            TextPassenger = $"{emp.name}",
                            IconDatePassengerSource = "clock.png",
                            TextDatePassenger = $"Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
                        };

                        passengerItems.Add(passengerItem);
                        PassengerCollectionView.ItemsSource = passengerItems;

                        #endregion

                        #region ADD PASSENGER
                        PassengerPersonalModel passengerPersonal = new PassengerPersonalModel()
                        {
                            date = DateTime.Now,
                            driver = emp_id,
                            trip = data_personal.trip,
                            job_id = data_personal.job_id,
                            latitude = data_personal.latitude,
                            longitude = data_personal.longitude,
                            location = data_personal.location,
                            location_mode = data_personal.location_mode,
                            passenger = emp.emp_id,
                            status = "START",
                            zipcode = data_personal.zipcode
                        };
                        string message = await PassengerPersonal.Insert(passengerPersonal);

                        // Insert Last Trip to Server DB
                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = data_personal.driver,
                            speed = 0,
                            emp_id = emp.emp_id,
                            job_id = data_personal.job_id,
                            trip_start = trip_start,
                            date = DateTime.Now,
                            distance = 0,
                            location = data_personal.location,
                            latitude = data_personal.latitude,
                            longitude = data_personal.longitude,
                            mileage_start = 0,
                            mileage_stop = 0,
                            mode = "PASSENGER PERSONAL",
                            status = true,
                            trip = data_personal.trip,
                            car_id = ""
                        };

                        message = await LastTrip.Insert(lastTrip);

                        if (message == "Success")
                        {
                            //MainThread.BeginInvokeOnMainThread(async () =>
                            //{
                            //    await DisplayAlert("", message, "OK");
                            //});
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await DisplayAlert("", "Error", "OK");
                            });
                        }
                        #endregion

                        frame_passenger.IsVisible = true;
                        Current_Passenger.Text = $"Current Passenger : ({passengerItems.Count})";
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
        }

        private async void OnDropOffPassengerItemClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is PassengerItems passengerItem)
                {
                    List<PassengerPersonalViewModel> passenger_personals = await PassengerPersonal.GetPassengerPersonalByDriver(emp_id, data_personal.trip);

                    List<string> emp_list = passenger_personals.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();

                    List<string> emps = passenger_personals.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger_name).ToList();
                    emps = emps.Distinct().ToList();

                    if (emps.Contains(passengerItem.TextPassenger)) // Check Passenger
                    {

                        bool confirm = await DisplayAlert("Confirm Drop Off", $"Drop Off: {passengerItem.TextPassenger}?", "Yes", "No");
                        if (confirm)
                        {
                            EmployeeModel emp = await Employee.GetEmployeeByName(passengerItem.TextPassenger);
                            PassengerPersonalModel passengerPersonal = new PassengerPersonalModel()
                            {
                                date = DateTime.Now,
                                driver = emp_id,
                                trip = data_personal.trip,
                                job_id = data_personal.job_id,
                                latitude = data_personal.latitude,
                                longitude = data_personal.longitude,
                                location = data_personal.location,
                                location_mode = data_personal.location_mode,
                                passenger = emp.emp_id,
                                status = "STOP",
                                zipcode = data_personal.zipcode
                            };
                            string message = await PassengerPersonal.Insert(passengerPersonal);

                            if (message == "Success")
                            {
                                LastTripModel lastTrip_passenger = new LastTripModel()
                                {
                                    driver = passengerPersonal.driver,
                                    speed = 0,
                                    job_id = passengerPersonal.job_id,
                                    emp_id = passengerPersonal.passenger,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = 0,
                                    location = passengerPersonal.location,
                                    latitude = passengerPersonal.latitude,
                                    longitude = passengerPersonal.longitude,
                                    mileage_start = 0,
                                    mileage_stop = 0,
                                    mode = "PASSENGER PERSONAL",
                                    status = false,
                                    trip = passengerPersonal.trip,
                                    car_id = ""
                                };

                                message = await LastTrip.UpdateByTrip(lastTrip_passenger);

                                passengerItems.Remove(passengerItem);
                                Current_Passenger.Text = $"Current Passenger : ({passengerItems.Count})";
                            }

                            if (passengerItems.Count == 0)
                            {
                                frame_passenger.IsVisible = false;
                            }
                        }
                    }
                    else
                    {
                        passengerItems.Remove(passengerItem);
                        Current_Passenger.Text = $"Current Passenger : ({passengerItems.Count})";
                        if (passengerItems.Count == 0)
                        {
                            frame_passenger.IsVisible = false;
                        }

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("", $"{passengerItem.TextPassenger} dropped off.", "OK");
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
        }
    }
}