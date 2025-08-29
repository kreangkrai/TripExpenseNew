namespace TripExpenseNew;

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using TripExpenseNew.CompanyPage;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.PassengerPage;
using TripExpenseNew.PersonalPage;
using TripExpenseNew.ViewModels;

public partial class Home_Page : ContentPage
{
    private ILastTrip LastTrip;
    private ILogin Login;
    private IEmployee Employee;
    CultureInfo cultureinfo = new CultureInfo("en-us");
    LoginModel emp_id = new LoginModel();
    List<LastTripViewModel> trips = new List<LastTripViewModel>();
    public Home_Page(ILastTrip _LastTrip, ILogin _Login, IEmployee _Employee)
    {
        InitializeComponent();
        LastTrip = _LastTrip;
        Login = _Login;
        Employee = _Employee;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            List<EmployeeModel> employees = await Employee.GetEmployees();
            emp_id = await Login.GetLogin(1);
            trips = await GetLastTrip();
            trips = trips.OrderByDescending(o => o.date).ToList();

            string name = employees.Where(w => w.emp_id == emp_id.emp_id).FirstOrDefault().name;
            lbl_name.Text = name.Split(' ')[0];
            lbl_lastname.Text = name.Split(' ')[1];

            if (trips.Count > 0)
            {
                if (trips[0].status == true) // In Use Trip
                {
                    if (trips[0].trip_start.Date == DateTime.Now.Date)
                    {
                        if (trips[0].mode.Contains("PASSENGER PERSONAL"))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (BindingContext is ButtonTrip viewModel)
                                {
                                    viewModel.ButtonTripText = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                    Text_Active.TextColor = Color.FromArgb("#FF474C");
                                    Text_Status.Text = $"You are a passenger of \n{trips[0].driver_name} \n Do you want to drop off now?";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "passenger.png";
                                }
                                else
                                {
                                    AddTripBtn.Text = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                    Text_Active.TextColor = Color.FromArgb("#FF474C");
                                    Text_Status.Text = $"You are a passenger of \n{trips[0].driver_name} \n Do you want to drop off now?";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "passenger.png";
                                }
                            });
                        }
                        else if (trips[0].mode.Contains("PASSENGER COMPANY"))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (BindingContext is ButtonTrip viewModel)
                                {
                                    viewModel.ButtonTripText = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                    Text_Active.TextColor = Color.FromArgb("#FF474C");
                                    Text_Status.Text = $"You are a passenger of \n{trips[0].driver_name} \n Do you want to drop off now?";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "passenger.png";
                                }
                                else
                                {
                                    AddTripBtn.Text = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                    Text_Active.TextColor = Color.FromArgb("#FF474C");
                                    Text_Status.Text = $"You are a passenger of \n{trips[0].driver_name} \n Do you want to drop off now?";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "passenger.png";
                                }
                            });
                        }
                        else
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (BindingContext is ButtonTrip viewModel)
                                {
                                    viewModel.ButtonTripText = "CONTINUE";
                                    AddTripBtn.BackgroundColor = Colors.Orange;
                                    Text_Active.TextColor = Colors.Orange;
                                    Text_Status.Text = "Please press CONTINUE\nfor start trip";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "driver.png";
                                }
                                else
                                {
                                    AddTripBtn.Text = "CONTINUE";
                                    AddTripBtn.BackgroundColor = Colors.Orange;
                                    Text_Active.TextColor = Colors.Orange;
                                    Text_Status.Text = "Please press CONTINUE\n for start trip";
                                    Text_Active.Text = "IN USE";
                                    img_status.Source = "driver.png";
                                }
                            });
                        }
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (BindingContext is ButtonTrip viewModel)
                            {
                                viewModel.ButtonTripText = "STOP";
                                AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                Text_Active.TextColor = Color.FromArgb("#FF474C");
                                Text_Status.Text = "Please press STOP\nfor stop trip";
                                Text_Active.Text = "IN USE";
                                img_status.Source = "driver.png";
                            }
                            else
                            {
                                AddTripBtn.Text = "STOP";
                                AddTripBtn.BackgroundColor = Color.FromArgb("#FF474C");
                                Text_Active.TextColor = Color.FromArgb("#FF474C");
                                Text_Status.Text = "Please press STOP\nfor stop trip";
                                Text_Active.Text = "IN USE";
                                img_status.Source = "driver.png";
                            }
                        });
                    }
                    txt_last_mileage.Text = trips[0].mileage_start.ToString();
                }
                else
                {
                    txt_last_mileage.Text = trips[0].mileage_stop.ToString();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (BindingContext is ButtonTrip viewModel)
                        {
                            viewModel.ButtonTripText = "ADD TRIP";
                            AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
                            img_status.Source = "car.png";
                            Text_Active.Text = "IDLE";
                        }
                        else
                        {
                            AddTripBtn.Text = "ADD TRIP";
                            AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
                            img_status.Source = "car.png";
                            Text_Active.Text = "IDLE";
                        }
                    });
                }

                txt_last_location.Text = trips[0].location;
                txt_last_date.Text = trips[0].date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo);
                txt_last_distance.Text = trips[0].distance.ToString("#.#") + " km";              
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonTrip viewModel)
                    {
                        viewModel.ButtonTripText = "ADD TRIP";
                        AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
                        img_status.Source = "car.png";
                        Text_Active.Text = "IDLE";
                    }
                    else
                    {
                        AddTripBtn.Text = "ADD TRIP";
                        AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
                        img_status.Source = "car.png";
                        Text_Active.Text = "IDLE";
                    }
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
    }
    private async Task<List<LastTripViewModel>> GetLastTrip()
    {
        if (emp_id != null)
        {
            List<LastTripViewModel> trips = await LastTrip.GetByEmp(emp_id.emp_id);
            return trips;
        }

        return null;
    }
    private async void OnGoToLoginPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Login_Page");
    }
    private async void OnGoToModePageClicked(object sender, EventArgs e)
    {
        if (trips.Count == 0)
        {
            await Navigation.PushAsync(new ModePage());
        }
        else
        {
            if (trips[0].status == true)
            {
                if (trips[0].trip_start.Date == DateTime.Now.Date)
                {
                    if (trips[0].mode == "PERSONAL")
                    {
                        PersonalPopupStartModel personal = new PersonalPopupStartModel()
                        {
                            IsCustomer = false,
                            job_id = trips[0].job_id,
                            location = new Location(trips[0].latitude, trips[0].longitude),
                            trip = trips[0].trip,
                            location_name = trips[0].location,
                            mileage = trips[0].mileage_start,
                            distance = trips[0].distance,
                            IsContinue = true,
                            trip_start = trips[0].trip_start
                        };

                        await Navigation.PushAsync(new Personal(personal));
                    }
                    if (trips[0].mode == "COMPANY")
                    {
                        CompanyPopupStartModel company = new CompanyPopupStartModel()
                        {
                            IsCustomer = false,
                            job_id = trips[0].job_id,
                            location = new Location(trips[0].latitude, trips[0].longitude),
                            trip = trips[0].trip,
                            location_name = trips[0].location,
                            mileage = trips[0].mileage_start,
                            distance = trips[0].distance,
                            IsContinue = true,
                            trip_start = trips[0].trip_start,
                            car_id = trips[0].car_id,
                            borrower = "059197"
                        };

                        await Navigation.PushAsync(new Company(company));
                    }

                    if (trips[0].mode == "PUBLIC")
                    {

                    }

                    if (trips[0].mode == "PASSENGER PERSONAL")
                    {
                        await Navigation.PushAsync(new PassengerPersonalStopPage(trips[0]));
                    }

                    if (trips[0].mode == "PASSENGER COMPANY")
                    {

                    }
                }
                else
                {
                    await Navigation.PushAsync(new PersonalForceStop(trips[0]));
                }
            }
            else
            {
                await Navigation.PushAsync(new ModePage());
            }
        }
    }
    private async void AddTripBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MainPage");
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("", "Do you want to Logout?", "Yes", "No");
        if (result)
        {
            await Shell.Current.GoToAsync("Login_Page");
        }
    }

    private async void HistoryBtn_Clicked(object sender, EventArgs e)
    {
        List<LastTripViewModel> lastTrips = await LastTrip.GetByEmp(emp_id.emp_id);
        lastTrips = lastTrips.Where(w=>w.trip_start.Date >= DateTime.Now.AddDays(-60)).ToList();
        lastTrips = lastTrips.OrderByDescending(o=>o.trip_start).ToList();
        await Navigation.PushAsync(new HistoryPage(lastTrips));
    }
}
