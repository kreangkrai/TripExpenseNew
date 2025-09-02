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
using TripExpenseNew.CustomCompanyPopup;

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

namespace TripExpenseNew.CompanyPage
{
    public partial class Company : ContentPage
    {
        private ILogin Login;
        private Interface.ICompany _Company;
        private ITracking Tracking;
        private ILastTrip LastTrip;
        private DBInterface.ICompany DB_Company;
        private DBInterface.IActiveCompany ActiveCompany;
        private IPassengerCompany PassengerCompany;
        private IEmployee Employee;
        private ILocationCustomer LocationCustomer;
        private ILocationOther LocationOther;
        private IMileage Mileage;
        private IInternet Internet;
        private Location previousLocation = null;
        private Location g_location = null;
        private Location last_location_for_passenger = null;
        private double totalDistance = 0;
        string emp_id = "";
        private int mileage_start = 0;
        CompanyPopupStartModel start = new CompanyPopupStartModel();
        bool isStart = false;
        bool isWaitStop = false;
        DateTime trip_start = DateTime.MinValue;
        DateTime start_tracking = DateTime.MinValue;
        DateTime lastInactive = DateTime.Now;
        CultureInfo cultureinfo = new CultureInfo("en-us");

        TrackingModel tracking = new TrackingModel();
        CompanyModel data_company = new CompanyModel();
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

        public Company(CompanyPopupStartModel _start)
        {
            InitializeComponent();
            _Company = new CompanyService();
            Login = new DBService.LoginService();
            start = _start;
            Tracking = new TrackingService();
            DB_Company = new DBService.CompanyService();
            ActiveCompany = new DBService.ActiveCompanyService();
            LastTrip = new LastTripService();
            PassengerCompany = new PassengerCompanyService();
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
            Text_Car.Text = $"(Company Car {start.car_id})";
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
                        bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK", "Cancel");
                        if (confirm || !confirm)
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
                    double displacement = CalculateDistanceInactive(10.0, interval);
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
                        data_company = new CompanyModel()
                        {
                            driver = emp_id,
                            car_id = start.car_id,                           
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
                            cash = 0,
                            fleetcard = 0,
                            borrower = start.borrower
                        };

                        isStart = true;
                        string message = await _Company.Insert(data_company);


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
                        data_company = new CompanyModel()
                        {
                            driver = emp_id,
                            car_id = start.car_id,
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
                            cash = 0,
                            fleetcard = 0,
                            borrower = start.borrower
                        };

                        isStart = true;
                        string message = await _Company.Insert(data_company);

                        // Insert Last Trip to Server DB
                        LastTripModel lastTrip = new LastTripModel()
                        {
                            driver = data_company.driver,
                            speed = data_company.speed,
                            job_id = data_company.job_id,
                            emp_id = data_company.driver,
                            trip_start = trip_start,
                            date = DateTime.Now,
                            distance = data_company.distance,
                            location = data_company.location,
                            latitude = data_company.latitude,
                            longitude = data_company.longitude,
                            mileage_start = mileage_start,
                            mileage_stop = 0,
                            mode = "COMPANY",
                            status = true,
                            trip = data_company.trip,
                            car_id = data_company.car_id,
                            borrower_id = data_company.borrower
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

                        last_location_for_passenger = location;
                    }


                    // Insert Active Company to Local DB
                    ActiveCompanyModel active_company = new ActiveCompanyModel()
                    {
                        driver = data_company.driver,
                        distance = totalDistance,
                        location = data_company.location,
                        mileage = data_company.mileage,
                        status = data_company.status,
                        trip = data_company.trip,
                        date = DateTime.Now,
                    };

                    int act = await ActiveCompany.Insert(active_company);

                    #region Show Active Personal
                    tripItems = new ObservableCollection<TripItems>();
                    List<ActiveCompanyModel> act_companies = await ActiveCompany.GetByTrip(data_company.trip);
                    foreach (var ap in act_companies)
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
                        double displacement = CalculateDistanceInactive(10.0, interval);
                        if (dist < displacement)  // Check ditance beteween point to point less than displacement
                        {
                            int minute_inactive = (int)(DateTime.Now - lastInactive).TotalMinutes;
                            if (minute_inactive >= 15)  // Inactive Each 15 Minute
                            {
                                if (!isInactive)
                                {
                                    CompanyModel company = new CompanyModel()
                                    {
                                        driver = emp_id,
                                        car_id = start.car_id,
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
                                        cash = 0,
                                        fleetcard = 0,
                                        borrower = start.borrower
                                    };
                                    string message = await _Company.Insert(company);

                                    LastTripModel lastTrip = new LastTripModel()
                                    {
                                        driver = company.driver,                                       
                                        speed = company.speed,
                                        job_id = company.job_id,
                                        emp_id = company.driver,
                                        trip_start = trip_start,
                                        date = DateTime.Now,
                                        distance = company.distance,
                                        location = company.location,
                                        latitude = company.latitude,
                                        longitude = company.longitude,
                                        mileage_start = mileage_start,
                                        mileage_stop = 0,
                                        mode = "COMPANY",
                                        status = true,
                                        trip = company.trip,
                                        car_id = company.car_id,
                                        borrower_id = company.borrower
                                    };

                                    string l = await LastTrip.UpdateByTrip(lastTrip);

                                    ActiveCompanyModel active_company = new ActiveCompanyModel()
                                    {
                                        driver = company.driver,
                                        distance = totalDistance,
                                        location = company.location,
                                        mileage = company.mileage,
                                        status = company.status,
                                        trip = company.trip,
                                        date = DateTime.Now,
                                        car_id = company.car_id
                                    };

                                    int act = await ActiveCompany.Insert(active_company);

                                    isInactive = true;
                                }
                            }
                        }

                        else
                        {
                            CompanyDBModel db_company = new CompanyDBModel()
                            {
                                driver = emp_id,
                                car_id = start.car_id,
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
                                cash = 0,
                                fleetcard = 0,
                                borrower = start.borrower,
                            };

                            int message = await DB_Company.Insert(db_company);

                            int diff = (int)(DateTime.Now - start_tracking).TotalSeconds;

                            if (diff >= tracking_db)
                            {
                                List<CompanyDBModel> db_companies = new List<CompanyDBModel>();
                                db_companies = await DB_Company.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                List<CompanyModel> companies = new List<CompanyModel>();
                                companies = db_companies.Select(s => new CompanyModel()
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
                                    car_id = s.car_id,
                                    fleetcard = s.fleetcard,
                                    borrower = s.borrower,
                                }).ToList();
                                string m = await _Company.Inserts(companies);

                                await DB_Company.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                CompanyModel company = companies.FirstOrDefault();

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = company.driver,
                                    speed = company.speed,
                                    job_id = company.job_id,
                                    emp_id = company.driver,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = company.distance,
                                    location = company.location,
                                    latitude = company.latitude,
                                    longitude = company.longitude,
                                    mileage_start = mileage_start,
                                    mileage_stop = 0,
                                    mode = "COMPANY",
                                    status = true,
                                    trip = company.trip,
                                    car_id = company.car_id,
                                    borrower_id = company.borrower
                                };

                                string l = await LastTrip.UpdateByTrip(lastTrip);

                                #region Show Active Personal
                                tripItems = new ObservableCollection<TripItems>();
                                List<ActiveCompanyModel> act_companies = await ActiveCompany.GetByTrip(company.trip);
                                foreach (var ap in act_companies)
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
                    var result = await this.ShowPopupAsync(new CompanyStopPopup(loc.Item1, loc.Item2, start.mileage,start.car_id));

                    if (result != null)
                    {
                        if (result is CompanyPopupStopModel company)
                        {
                            if (company.location != null && company.location != "" && company.mileage != 0)
                            {
                                if (company.mileage >= start.mileage)
                                {
                                    var popup = new ProgressPopup();
                                    this.ShowPopup(popup);
                                    double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                                    var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                                    var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                                    List<CompanyDBModel> db_companies = await DB_Company.GetByTrip(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                    List<CompanyModel> companies = db_companies.Select(s => new CompanyModel()
                                    {
                                        job_id = s.job_id,
                                        distance = s.distance,
                                        date = s.date,
                                        latitude = s.latitude,
                                        longitude = s.longitude,
                                        location = company.location,
                                        zipcode = s.zipcode,
                                        location_mode = company.IsCustomer ? "CUSTOMER" : "OTHER",
                                        speed = s.speed,
                                        mileage = company.mileage,
                                        trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                        status = s.status,
                                        driver = s.driver,
                                        cash = s.cash,
                                        fleetcard = s.fleetcard,
                                        borrower = s.borrower,
                                        car_id = s.car_id
                                    }).ToList();

                                    string message = await _Company.Inserts(companies);
                                    if (message == "Success")
                                    {
                                        await DB_Company.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));

                                        data_company = new CompanyModel()
                                        {
                                            driver = emp_id,
                                            car_id = company.car_id,
                                            date = DateTime.Now,
                                            job_id = start.job_id,
                                            distance = totalDistance,
                                            latitude = g_location.Latitude,
                                            longitude = g_location.Longitude,
                                            location = company.location,
                                            zipcode = zipcode,
                                            location_mode = company.IsCustomer ? "CUSTOMER" : "OTHER",
                                            speed = speed,
                                            mileage = company.mileage,
                                            trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                            status = "STOP",
                                            cash = 0,
                                            fleetcard = 0,
                                            borrower = start.borrower
                                        };
                                        message = await _Company.Insert(data_company);

                                        if (message == "Success")
                                        {
                                            LastTripModel lastTrip = new LastTripModel()
                                            {
                                                driver = data_company.driver,
                                                speed = data_company.speed,
                                                emp_id = data_company.driver,
                                                job_id = data_company.job_id,
                                                trip_start = trip_start,
                                                date = data_company.date,
                                                distance = data_company.distance,
                                                location = data_company.location,
                                                latitude = data_company.latitude,
                                                longitude = data_company.longitude,
                                                mileage_start = mileage_start,
                                                mileage_stop = data_company.mileage,
                                                mode = "COMPANY",
                                                status = false,
                                                trip = data_company.trip,
                                                car_id = data_company.car_id,
                                                borrower_id = data_company.borrower
                                                
                                            };

                                            message = await LastTrip.UpdateByTrip(lastTrip);

                                            int act = await ActiveCompany.Delete(trip_start.ToString("yyyyMMddHHmmss", cultureinfo));
                                        }
                                    }

                                    #region Add Location

                                    if (loc.Item1 != company.location && company.location != "CTL(HQ)" && company.location != "CTL(KBO)" && company.location != "CTL(RBO)") // Insert New Location
                                    {
                                        if (company.IsCustomer)
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
                                    List<PassengerCompanyViewModel> passenger_companies = await PassengerCompany.GetPassengerCompanyByDriver(data_company.driver, data_company.trip);

                                    List<string> emp_list = passenger_companies.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();
                                    List<string> emps = passenger_companies.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger).ToList();
                                    emps = emps.Distinct().ToList();

                                    if (emps.Count > 0)
                                    {
                                        #region ADD PASSENGER
                                        for (int i = 0; i < emps.Count; i++)
                                        {
                                            PassengerCompanyModel passengerCompany = new PassengerCompanyModel()
                                            {
                                                date = data_company.date,
                                                driver = data_company.driver,
                                                trip = data_company.trip,
                                                job_id = data_company.job_id,
                                                latitude = data_company.latitude,
                                                longitude = data_company.longitude,
                                                location = data_company.location,
                                                location_mode = data_company.location_mode,
                                                passenger = emps[i],
                                                status = "STOP",
                                                zipcode = data_company.zipcode,
                                                car_id = data_company.car_id,
                                            };
                                            string mes = await PassengerCompany.Insert(passengerCompany);

                                            LastTripModel lastTrip_passenger = new LastTripModel()
                                            {
                                                driver = data_company.driver,
                                                speed = 0,
                                                emp_id = emps[i],
                                                job_id = data_company.job_id,
                                                trip_start = trip_start,
                                                date = DateTime.Now,
                                                distance = 0,
                                                location = data_company.location,
                                                latitude = data_company.latitude,
                                                longitude = data_company.longitude,
                                                mileage_start = 0,
                                                mileage_stop = 0,
                                                mode = "PASSENGER COMPANY",
                                                status = false,
                                                trip = data_company.trip,
                                                car_id = data_company.car_id,
                                                borrower_id = ""
                                                
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
                                        mileage = company.mileage
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
                var popup = new CompanyCheckInAlert { Title = "CHECK IN", Message = "Please Select type of check in?" };
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
                        double fleet = 0;
                        int mileage = 0;
                        string location_mode = "";

                        bool isChkIn = false;
                        if (result.ToString() == "Customer")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_customer = await this.ShowPopupAsync(new CompanyCheckinCustomerPopup(chkinlocation));

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
                            var result_other = await this.ShowPopupAsync(new CompanyCheckinOtherPopup(chkinlocation));

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

                        if (result.ToString() == "Gas Station (cash)")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_gas = await this.ShowPopupAsync(new CompanyCheckinGasCashPopup());

                            if (result_gas != null)
                            {
                                if (result_gas is Tuple<string, double,int> data)
                                {
                                    chkinlocation = data.Item1;
                                    cash = data.Item2;
                                    mileage = data.Item3;
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

                        if (result.ToString() == "Gas Station (fleetcard)")
                        {
                            if (loc.Item2 == true)
                            {
                                chkinlocation = loc.Item1;
                            }
                            var result_gas = await this.ShowPopupAsync(new CompanyCheckinGasFleetCardPopup());

                            if (result_gas != null)
                            {
                                if (result_gas is Tuple<string, double,int> data)
                                {
                                    chkinlocation = data.Item1;
                                    fleet = data.Item2;
                                    mileage= data.Item3;
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
                            data_company = new CompanyModel()
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
                                mileage = mileage,
                                trip = trip_start.ToString("yyyyMMddHHmmss", cultureinfo),
                                status = "CHECK IN",
                                cash = cash,
                                fleetcard = fleet,
                                car_id = start.car_id,  
                                borrower = start.borrower
                            };

                            string message = await _Company.Insert(data_company);

                            if (message == "Success")
                            {
                                ActiveCompanyModel active_company = new ActiveCompanyModel()
                                {
                                    driver = data_company.driver,
                                    distance = data_company.distance,
                                    location = data_company.location,
                                    mileage = data_company.mileage,
                                    status = data_company.status,
                                    trip = data_company.trip,
                                    date = data_company.date,
                                    car_id = data_company.car_id
                                };

                                int act = await ActiveCompany.Insert(active_company);

                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = data_company.driver,
                                    speed = data_company.speed,
                                    job_id = data_company.job_id,
                                    emp_id = data_company.driver,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = data_company.distance,
                                    location = data_company.location,
                                    latitude = data_company.latitude,
                                    longitude = data_company.longitude,
                                    mileage_start = mileage_start,
                                    mileage_stop = 0,
                                    mode = "COMPANY",
                                    status = true,
                                    trip = data_company.trip,
                                    car_id = data_company.driver,
                                    borrower_id = data_company.borrower
                                };

                                message = await LastTrip.UpdateByTrip(lastTrip);

                            }


                            #region GET PASSENGER
                            List<PassengerCompanyViewModel> passenger_companies = await PassengerCompany.GetPassengerCompanyByDriver(data_company.driver, data_company.trip);

                            List<string> emp_list = passenger_companies.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();

                            List<string> emps = passenger_companies.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger).ToList();
                            emps = emps.Distinct().ToList();

                            if (emps.Count > 0)
                            {
                                for (int i = 0; i < emps.Count; i++)
                                {
                                    PassengerCompanyModel passengerCompany = new PassengerCompanyModel ()
                                    {
                                        date = data_company.date,
                                        driver = data_company.driver,
                                        trip = data_company.trip,
                                        job_id = data_company.job_id,
                                        latitude = data_company.latitude,
                                        longitude = data_company.longitude,
                                        location = data_company.location,
                                        location_mode = data_company.location_mode,
                                        passenger = emps[i],
                                        status = "CHECK IN",
                                        zipcode = data_company.zipcode,
                                        car_id = data_company.car_id
                                    };
                                    message = await PassengerCompany.Insert(passengerCompany);

                                    if (message == "Success")
                                    {
                                        LastTripModel lastTrip_passenger = new LastTripModel()
                                        {
                                            driver = emp_id,
                                            speed = 0,
                                            emp_id = emps[i],
                                            job_id = data_company.job_id,
                                            trip_start = trip_start,
                                            date = DateTime.Now,
                                            distance = 0,
                                            location = data_company.location,
                                            latitude = data_company.latitude,
                                            longitude = data_company.longitude,
                                            mileage_start = 0,
                                            mileage_stop = 0,
                                            mode = "PASSENGER COMPANY",
                                            status = true,
                                            trip = data_company.trip,
                                            car_id = data_company.car_id,
                                            borrower_id = ""
                                        };

                                        message = await LastTrip.UpdateByTrip(lastTrip_passenger);
                                    }
                                }
                            }

                            #endregion

                            #region Show Active Personal
                            tripItems = new ObservableCollection<TripItems>();
                            List<ActiveCompanyModel> act_companies = await ActiveCompany.GetByTrip(data_company.trip);
                            foreach (var ap in act_companies)
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

                            last_location_for_passenger = g_location;
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

        }
        private async void AddPassengerBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (last_location_for_passenger != null)
                {
                    double dist = CalculateDistance(last_location_for_passenger, g_location);
                    if (dist < 0.5) // 500 meters 
                    {
                        var result = await this.ShowPopupAsync(new CompanyPassengerPopup());

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
                                PassengerCompanyModel passengerCompany = new PassengerCompanyModel()
                                {
                                    date = DateTime.Now,
                                    driver = emp_id,
                                    trip = data_company.trip,
                                    job_id = data_company.job_id,
                                    latitude = data_company.latitude,
                                    longitude = data_company.longitude,
                                    location = data_company.location,
                                    location_mode = data_company.location_mode,
                                    passenger = emp.emp_id,
                                    status = "START",
                                    zipcode = data_company.zipcode,
                                    car_id = data_company.car_id
                                };
                                string message = await PassengerCompany.Insert(passengerCompany);

                                // Insert Last Trip to Server DB
                                LastTripModel lastTrip = new LastTripModel()
                                {
                                    driver = data_company.driver,
                                    speed = 0,
                                    emp_id = emp.emp_id,
                                    job_id = data_company.job_id,
                                    trip_start = trip_start,
                                    date = DateTime.Now,
                                    distance = 0,
                                    location = data_company.location,
                                    latitude = data_company.latitude,
                                    longitude = data_company.longitude,
                                    mileage_start = 0,
                                    mileage_stop = 0,
                                    mode = "PASSENGER COMPANY",
                                    status = true,
                                    trip = data_company.trip,
                                    car_id = data_company.car_id,
                                    borrower_id = ""
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
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("", "Please check-in before adding a passenger", "OK");
                        });
                    }
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "Please check-in before adding a passenger", "OK");
                    });
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
                if (last_location_for_passenger != null)
                {
                    double dist = CalculateDistance(last_location_for_passenger, g_location);
                    if (dist < 0.5)
                    {
                        if (sender is Button button && button.CommandParameter is PassengerItems passengerItem)
                        {
                            List<PassengerCompanyViewModel> passenger_companies = await PassengerCompany.GetPassengerCompanyByDriver(emp_id, data_company.trip);

                            List<string> emp_list = passenger_companies.Where(w => w.status == "STOP").Select(s => s.passenger).ToList();

                            List<string> emps = passenger_companies.Where(w => !emp_list.Contains(w.passenger)).Select(s => s.passenger_name).ToList();
                            emps = emps.Distinct().ToList();

                            if (emps.Contains(passengerItem.TextPassenger)) // Check Passenger
                            {

                                bool confirm = await DisplayAlert("Confirm Drop Off", $"Drop Off: {passengerItem.TextPassenger}?", "Yes", "No");
                                if (confirm)
                                {
                                    EmployeeModel emp = await Employee.GetEmployeeByName(passengerItem.TextPassenger);
                                    PassengerCompanyModel passengerCompany = new PassengerCompanyModel()
                                    {
                                        date = DateTime.Now,
                                        driver = emp_id,
                                        trip = data_company.trip,
                                        job_id = data_company.job_id,
                                        latitude = data_company.latitude,
                                        longitude = data_company.longitude,
                                        location = data_company.location,
                                        location_mode = data_company.location_mode,
                                        passenger = emp.emp_id,
                                        status = "STOP",
                                        zipcode = data_company.zipcode,
                                        car_id = data_company.car_id
                                    };
                                    string message = await PassengerCompany.Insert(passengerCompany);

                                    if (message == "Success")
                                    {
                                        LastTripModel lastTrip_passenger = new LastTripModel()
                                        {
                                            driver = passengerCompany.driver,
                                            speed = 0,
                                            job_id = passengerCompany.job_id,
                                            emp_id = passengerCompany.passenger,
                                            trip_start = trip_start,
                                            date = DateTime.Now,
                                            distance = 0,
                                            location = passengerCompany.location,
                                            latitude = passengerCompany.latitude,
                                            longitude = passengerCompany.longitude,
                                            mileage_start = 0,
                                            mileage_stop = 0,
                                            mode = "PASSENGER COMPANY",
                                            status = false,
                                            trip = passengerCompany.trip,
                                            car_id = passengerCompany.car_id,
                                            borrower_id = ""
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
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("", "Please check-in before dropping off a passenger", "OK");
                        });
                    }
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "Please check-in before dropping off a passenger", "OK");
                    });
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