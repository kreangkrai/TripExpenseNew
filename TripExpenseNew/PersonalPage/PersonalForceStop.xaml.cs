namespace TripExpenseNew.PersonalPage;

using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Models;
using TripExpenseNew.Interface;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Services;
using TripExpenseNew.DBService;
using TripExpenseNew.CustomPopup;
using CommunityToolkit.Maui.Views;

#if IOS
using UserNotifications;
using Microsoft.Maui.Maps;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;

#endif
#if IOS
using CoreLocation;
#endif

public partial class PersonalForceStop : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IMileage Mileage;
    private DBInterface.IPersonal DB_Personal;
    private Interface.IPersonal _Personal;
    private ILastTrip LastTrip;
    private DBInterface.IActivePersonal ActivePersonal;
    private IPassengerPersonal PassengerPersonal;
    private CancellationTokenSource cancellationTokenSource;
    private bool isTracking = true;
    Tuple<string, bool> loc = new Tuple<string, bool>("", false);
    Location g_location = null;
    bool IsCustomer = false;
    LastTripViewModel trip = new LastTripViewModel ();
    string emp_id = "";
    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();

#if IOS
        private Platforms.iOS.LocationService locationService;
#elif ANDROID
    private Intent intent = new Intent();
#endif

    int interval = 0;
    int mileage_start = 0;
    public PersonalForceStop(LastTripViewModel _trip)
    {
        InitializeComponent();
        Login = new LoginService();
        LocationCustomer = new LocationCustomerService();
        LocationOther = new LocationOtherService();
        Mileage = new MileageService();
        DB_Personal = new DBService.PersonalService();
        _Personal = new Services.PersonalService();
        LastTrip = new LastTripService();
        ActivePersonal = new ActivePersonalService();
        PassengerPersonal = new PassengerPersonalService();
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
            UpdateLocationDataAsync(data.Location);

        });

        Text_TripName.Text = _trip.trip;
        Text_MileageStart.Text = _trip.mileage.ToString();
        mileage_start = _trip.mileage;
        emp_id = _trip.emp_id;
        trip = _trip;
        timePicker.Time = new TimeSpan(17, 30, 0); // ตั้งเวลาเริ่มต้นเป็น 17:30
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }
        }

        status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }
        }

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

#if IOS
            try
            {
                locationService = new Platforms.iOS.LocationService(5);
            }
            catch (Exception ex)
            {
                //LocationLabel.Text = $"เกิดข้อผิดพลาดในการเริ่มต้น LocationService: {ex.Message}";
                Console.WriteLine($"LocationService Initialization Error: {ex}");
            }
#endif

        await GetLocation();
    }

    private async Task GetLocation()
    {
        try
        {
            if (isTracking)
            {
#if IOS
                    // ตรวจสอบ Location Services ด้วย CLLocationManager
                    if (!CLLocationManager.LocationServicesEnabled)
                    {
                        //LocationLabel.Text = "Location Services ถูกปิด กรุณาเปิดใน Settings";
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
                        //LocationLabel.Text = "ไม่ได้รับอนุญาต กรุณาเปิดใช้บริการตำแหน่ง";
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
                        //LocationLabel.Text = "ไม่ได้รับอนุญาต Background Location";
                        return;
                    }
                }
#endif


                cancellationTokenSource = new CancellationTokenSource();

#if IOS
                    if (locationService == null)
                    {
                       // LocationLabel.Text = "LocationService ไม่ได้เริ่มต้น";
                        return;
                    }
                    locationService.StartUpdatingLocation(async location =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => UpdateLocationDataAsync(location));
                    });
#elif ANDROID
                intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
                intent.PutExtra("TrackingInterval", 5000);
                Platform.AppContext.StartForegroundService(intent);
#endif
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Crash in OnToggleTrackingClicked: {ex}");
        }
    }

    private void UpdateLocationDataAsync(Location location)
    {
        try
        {
            if (location != null)
            {

                FindLocationService findLocation = new FindLocationService();
                loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, location);

                g_location = location;
                IsCustomer = loc.Item2;
                if (IsCustomer) // Customer
                {
                    CustomerBtn.BackgroundColor = Colors.Blue;
                    OtherBtn.BackgroundColor = Colors.Grey;
                }
                else
                {
                    CustomerBtn.BackgroundColor = Colors.Grey;
                    OtherBtn.BackgroundColor = Colors.Blue;
                }


                Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}");
            }
            else
            {

            }

            #region STOP
#if IOS
            locationService?.StopUpdatingLocation();
            locationService = null;
#elif ANDROID
            intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            Platform.AppContext.StopService(intent);
#endif
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
        }
    }

    private void OtherBtn_Clicked(object sender, EventArgs e)
    {
        IsCustomer = false;
        CustomerBtn.BackgroundColor = Colors.Grey;
        OtherBtn.BackgroundColor = Colors.Blue;
    }

    private void CustomerBtn_Clicked(object sender, EventArgs e)
    {
        IsCustomer = true;

        CustomerBtn.BackgroundColor = Colors.Blue;
        OtherBtn.BackgroundColor = Colors.Grey;
    }

    private async void ConfirmBtn_Clicked(object sender, EventArgs e)
    {
        if (Text_Location.Text.Trim() != "" && Text_MileageStop.Text.Trim() != "")
        {

            int mileage_stop = Int32.Parse(Text_MileageStop.Text);
            if (mileage_stop >= mileage_start)
            {
                var popup = new ProgressPopup();
                this.ShowPopup(popup);

                DateTime date = new DateTime (trip.trip_start.Year, trip.trip_start.Month, trip.trip_start.Day,timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                
                double speed = g_location?.Speed.HasValue ?? false ? g_location.Speed.Value * 3.6 : 0;
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(g_location.Latitude, g_location.Longitude);
                var zipcode = placemarks?.FirstOrDefault()?.PostalCode ?? "N/A";

                //List<PersonalDBModel> db_personals = await DB_Personal.GetByTrip(trip.trip);
                //List<PersonalModel> personals = db_personals.Select(s => new PersonalModel()
                //{
                //    job_id = s.job_id,
                //    distance = s.distance,
                //    date = date,
                //    latitude = g_location.Latitude,
                //    longitude = g_location.Longitude,
                //    location = Text_Location.Text,
                //    zipcode = zipcode,
                //    location_mode = IsCustomer ? "CUSTOMER" : "OTHER",
                //    speed = speed,
                //    mileage = mileage_stop,
                //    trip = trip.trip,
                //    status = "STOP",
                //    driver = s.driver,
                //    cash = 0,
                //}).ToList();

                //string message = await _Personal.Inserts(personals);

                PersonalModel data_personal = new PersonalModel();
                string message = "Success";
                
                    await DB_Personal.Delete(trip.trip);

                    data_personal = new PersonalModel()
                    {
                        driver = emp_id,
                        date = date,
                        job_id = trip.job_id,
                        distance = trip.distance,
                        latitude = g_location.Latitude,
                        longitude = g_location.Longitude,
                        location = Text_Location.Text,
                        zipcode = zipcode,
                        location_mode = IsCustomer ? "CUSTOMER" : "OTHER",
                        speed = speed,
                        mileage = mileage_stop,
                        trip = trip.trip,
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
                        trip_start = trip.trip_start,
                        date = data_personal.date,
                        distance = data_personal.distance,
                        location = data_personal.location,
                        latitude = data_personal.latitude,
                        longitude = data_personal.longitude,
                        mileage = data_personal.mileage,
                        mode = "PERSONAL",
                        status = false,
                        trip = data_personal.trip,
                        car_id = data_personal.driver
                    };

                    message = await LastTrip.UpdateByTrip(lastTrip);

                    int act = await ActivePersonal.Delete(trip.trip);
                }
                

                #region Add Location

                if (loc.Item1 != Text_Location.Text && Text_Location.Text != "CTL(HQ)" && Text_Location.Text != "CTL(KBO)" && Text_Location.Text != "CTL(RBO)") // Insert New Location
                {
                    if (IsCustomer)
                    {
                        LocationCustomerModel locationCustomer = new LocationCustomerModel()
                        {
                            emp_id = emp_id,
                            latitude = g_location.Latitude,
                            longitude = g_location.Longitude,
                            location = Text_Location.Text,
                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
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
                            location = Text_Location.Text,
                            location_id = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
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
                            trip_start = trip.trip_start,
                            date = date,
                            distance = 0,
                            location = data_personal.location,
                            latitude = data_personal.latitude,
                            longitude = data_personal.longitude,
                            mileage = 0,
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
                    mileage = mileage_stop
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
                await popup.CloseAsync();
                await Shell.Current.GoToAsync("Home_Page");
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", "กรุณาใส่ข้อมูลให้ถูกต้อง", "ตกลง");
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

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
#if IOS
        locationService?.StopUpdatingLocation();
        locationService = null;
#elif ANDROID
        intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
        Platform.AppContext.StopService(intent);
#endif

        await Shell.Current.GoToAsync("Home_Page");
    }
}