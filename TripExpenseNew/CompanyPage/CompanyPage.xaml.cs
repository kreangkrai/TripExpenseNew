using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;
using TripExpenseNew.CustomPopup;
using ZXing.Net.Maui;
using TripExpenseNew.Interface;
using TripExpenseNew.DBInterface;
using TripExpenseNew.Models;
using CommunityToolkit.Mvvm.Messaging;
using TripExpenseNew.Services;
using TripExpenseNew.DBModels;
using TripExpenseNew.ViewModels;
using TripExpenseNew.CustomCompanyPopup;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if IOS
using UserNotifications;

#endif

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS
using CoreLocation;
#endif

namespace TripExpenseNew.CompanyPage;

public partial class CompanyPage : ContentPage
{
    private ILocationCustomer LocationCustomer;
    private ILocationOther LocationOther;
    private ILogin Login;
    private IInternet Internet;
    private ICar Car;
    private ILastTrip LastTrip;
    private IMileage Mileage;
    private IBorrower Borrower;
    private bool isTracking = true;
    Tuple<string, bool> loc = new Tuple<string, bool>("", false);
    Location g_location = null;
    List<LocationCustomerModel> GetLocationCustomers = new List<LocationCustomerModel>();
    List<LocationOtherModel> GetLocationOthers = new List<LocationOtherModel>();
    List<LocationOtherModel> GetLocationCTL = new List<LocationOtherModel>();

    string car_id = "";
#if IOS
    private Platforms.iOS.LocationService locationService;
#elif ANDROID
        private Intent intent = new Intent();
#endif

    public CompanyPage(ILocationCustomer _LocationCustomer, ILogin _Login, ILocationOther _LocationOther, IInternet _Internet, ICar _Car, ILastTrip _LastTrip, IMileage _Mileage, IBorrower _Borrower)
    {
        InitializeComponent();
        Login = _Login;
        LocationCustomer = _LocationCustomer;
        LocationOther = _LocationOther;
        Internet = _Internet;
        Car = _Car;
        LastTrip = _LastTrip;
        Mileage = _Mileage;
        Borrower = _Borrower;
        WeakReferenceMessenger.Default.Register<LocationData>(this, (send, data) =>
        {
            if (send != null)
            {
                UpdateLocationDataAsync(data.Location);
            }

        });
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
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool confirm = await DisplayAlert("", "Please select type of location permission to Always.", "OK", "Cancel");
                    if (confirm || !confirm)
                    {
                        AppInfo.ShowSettingsUI();
                    }
                });

                return;
            }
        }

        //if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        //{
        //    await LocalNotificationCenter.Current.RequestNotificationPermission();
        //}

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
    private async void CompanyCancel_Clicked(object sender, EventArgs e)
    {
        CompanyCancel.IsEnabled = false;
        await Shell.Current.GoToAsync("Home_Page");
        CompanyCancel.IsEnabled = true;
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
                intent.PutExtra("TrackingInterval", 2000);
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonScanQR scan)
                    {
                        scan.ButtonScanQRText = "SCAN QR";
                    }
                    else
                    {
                        ScanQR.IsEnabled = true;
                        ScanQR.TextColor = Colors.White;
                        ScanQR.BackgroundColor = Color.FromArgb("#297CC0");
                        ScanQR.Text = "SCAN QR";
                    }
                });


                FindLocationService findLocation = new FindLocationService();
                loc = findLocation.FindLocation(GetLocationCTL, GetLocationOthers, GetLocationCustomers, location);

                g_location = location;
                //Console.WriteLine($"ALL ==> Lat: {location.Latitude}, Lon: {location.Longitude}");
            }

            #region STOP
#if IOS
            locationService?.StopUpdatingLocation();
            locationService = null;
            //#elif ANDROID
            //intent = new Intent(Platform.AppContext, typeof(TripExpenseNew.Platforms.Android.LocationService));
            //Platform.AppContext.StopService(intent);
#endif
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateLocationDataAsync Error: {ex}");
        }
    }
    private async void ScanQR_Clicked(object sender, EventArgs e)
    {
        ScanQR.IsEnabled = false;
        var popup = new ScanQRPopup();

        popup.OnQRScanned += async (qrValue) =>
        {
            car_id = qrValue;
            CarModel car = await Car.GetByCar(car_id);
            if (car.car_id != null)
            {
                List<LastTripViewModel> trips = await LastTrip.GetByCar(car_id);
                LastTripViewModel trip = trips.Where(w => w.status == true).LastOrDefault();
                if (trip == null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Btn_Start.IsEnabled = true;
                        Btn_Start.TextColor = Colors.White;
                        Btn_Start.BackgroundColor = Color.FromArgb("#297CC0");
                        Btn_Start.Text = qrValue;

                    });
                }
                else
                {
                    Btn_Start.IsEnabled = false;
                    Btn_Start.TextColor = Colors.White;
                    Btn_Start.BackgroundColor = Colors.Grey;
                    Btn_Start.Text = "Processing..";
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("COMPANY CAR", $"This car {trip.license_plate} using by\n {trip.driver_name}", "OK");
                    });
                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", "Car not found!", "OK");
                });
            }
        };
        await this.ShowPopupAsync(popup);

        //if (result != null)
        //{
        //    car_id = result.ToString();
        //    CarModel car = await Car.GetByCar(car_id);
        //    if (car.car_id != null)
        //    {
        //        List<LastTripViewModel> trips = await LastTrip.GetByCar(car_id);                
        //        LastTripViewModel trip = trips.Where(w => w.status == true).LastOrDefault();
        //        if (trip == null)
        //        {
        //            MainThread.BeginInvokeOnMainThread(() =>
        //            {
        //                if (BindingContext is ButtonCompanyStart viewModel)
        //                {                          
        //                    Btn_Start.IsEnabled = true;
        //                    Btn_Start.TextColor = Colors.White;
        //                    Btn_Start.BackgroundColor = Color.FromArgb("#297CC0");
        //                    viewModel.ButtonCompanyStartText = "START";
        //                }
        //                else
        //                {
        //                    Btn_Start.IsEnabled = true;
        //                    Btn_Start.TextColor = Colors.White;
        //                    Btn_Start.BackgroundColor = Color.FromArgb("#297CC0");
        //                    Btn_Start.Text = "START";
        //                }
        //            });                   
        //        }
        //        else
        //        {
        //            Btn_Start.IsEnabled = false;
        //            Btn_Start.TextColor = Colors.White;
        //            Btn_Start.BackgroundColor = Colors.Grey;
        //            Btn_Start.Text = "Processing..";
        //            MainThread.BeginInvokeOnMainThread(async () =>
        //            {
        //                await DisplayAlert("COMPANY CAR", $"This car {trip.license_plate} using by\n {trip.driver_name}", "OK");
        //            });
        //        }
        //    }
        //    else
        //    {
        //        MainThread.BeginInvokeOnMainThread(async () =>
        //        {
        //            await DisplayAlert("", "Car not found!", "OK");
        //        });
        //    }
        //}
        //else
        //{
        //    MainThread.BeginInvokeOnMainThread(async () =>
        //    {
        //        await DisplayAlert("", "Invalid QR Code", "ตกลง");
        //    });
        //}
        ScanQR.IsEnabled = true;
    }

    private async void Btn_Start_Clicked(object sender, EventArgs e)
    {
        Btn_Start.IsEnabled = false;
        bool internet = await Internet.CheckServerConnection("/api/CurrentTime/get");
        if (internet)
        {
            MileageDBModel mileage = await Mileage.GetMileage(1);

            //car_id = "CAR99";

            BorrowerViewModel borrower_id = await Borrower.GetBorrowerByCar(car_id);
            if (borrower_id.borrow_id != null)
            {
                var result = await this.ShowPopupAsync(new CompanyStartPopup(loc.Item1, loc.Item2, mileage.mileage, car_id));

                if (result is CompanyPopupStartModel company)
                {
                    if (company.location_name != null && company.location_name != "" && company.mileage != 0)
                    {
                        company.location = g_location;
                        company.IsContinue = false;
                        company.trip_start = DateTime.Now;
                        company.job_id = company.job_id != null ? company.job_id : "";
                        company.car_id = company.car_id;
                        company.borrower = borrower_id.borrower;
                        await Navigation.PushAsync(new Company(company));
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("", "กรุณาใส่ข้อมูล", "OK");
                        });
                    }
                }
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", "ไม่มีข้อมูลการยืมรถ", "OK");
                });
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", "Cann't connect to server", "OK");
            });
        }
        Btn_Start.IsEnabled = true;
    }
}
