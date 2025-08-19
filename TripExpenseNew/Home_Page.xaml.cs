namespace TripExpenseNew;

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using System.Collections.Generic;
using System.ComponentModel;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.PersonalPage;
using TripExpenseNew.ViewModels;

public partial class Home_Page : ContentPage
{
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight;
    private ILastTrip LastTrip;
    private ILogin Login;
    LoginModel emp_id = new LoginModel();
    List<LastTripViewModel> trips = new List<LastTripViewModel>();
    public Home_Page(ILastTrip _LastTrip, ILogin _Login)
    {
        InitializeComponent();
        LastTrip = _LastTrip;
        Login = _Login;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            emp_id = await Login.GetLogin(1);
            trips = await GetLastTrip();
            trips = trips.OrderByDescending(o=>o.trip).ToList();
            if (trips.Count > 0)
            {
                if (trips[0].status == true) // In Use Trip
                {
                    if (trips[0].trip_start.Date == DateTime.Now.Date)
                    {
                        if (trips[0].mode.Contains("PASSENGER"))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                if (BindingContext is ButtonTrip viewModel)
                                {
                                    viewModel.ButtonTripText = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Colors.Red;
                                    Button_Active.BackgroundColor = Colors.Red;
                                    Text_Status.Text = "Do you want to drop off now?";
                                    Text_Active.Text = "In Use";
                                }
                                else
                                {
                                    AddTripBtn.Text = "DROP OFF";
                                    AddTripBtn.BackgroundColor = Colors.Red;
                                    Button_Active.BackgroundColor = Colors.Red;
                                    Text_Status.Text = "Do you want to drop off now?";
                                    Text_Active.Text = "In Use";
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
                                    Button_Active.BackgroundColor = Colors.Orange;
                                    Text_Status.Text = "Please press CONTINUE for start trip";
                                    Text_Active.Text = "In Use";
                                }
                                else
                                {
                                    AddTripBtn.Text = "CONTINUE";
                                    AddTripBtn.BackgroundColor = Colors.Orange;
                                    Button_Active.BackgroundColor = Colors.Orange;
                                    Text_Status.Text = "Please press CONTINUE for start trip";
                                    Text_Active.Text = "In Use";
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
                                AddTripBtn.BackgroundColor = Colors.Red;
                                Button_Active.BackgroundColor = Colors.Red;
                                Text_Status.Text = "Please press STOP for stop trip";
                                Text_Active.Text = "In Use";
                            }
                            else
                            {
                                AddTripBtn.Text = "STOP";
                                AddTripBtn.BackgroundColor = Colors.Red;
                                Button_Active.BackgroundColor = Colors.Red;
                                Text_Status.Text = "Please press STOP for stop trip";
                                Text_Active.Text = "In Use";
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
                            viewModel.ButtonTripText = "ADD TRIP";
                        }
                        else
                        {
                            AddTripBtn.Text = "ADD TRIP";
                            AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
                        }
                    });
                }

                if (trips[0].driver_name.Length > 25)
                {
                    lbl_name.FontSize = 30;
                    lbl_lastname.FontSize = 30;
                }
                else
                {
                    lbl_name.FontSize = 34;
                    lbl_lastname.FontSize = 34;
                }

                lbl_name.Text = trips[0].emp_name.Split(' ')[0];
                lbl_lastname.Text = trips[0].emp_name.Split(' ')[1];

                txt_last_location.Text = trips[0].location;
                txt_last_date.Text = trips[0].date.ToString("dd/MM/yyyy HH:mm:ss");
                txt_last_distance.Text = trips[0].distance.ToString("#.#") + " km";
                txt_last_mileage.Text = trips[0].mileage.ToString();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonTrip viewModel)
                    {
                        viewModel.ButtonTripText = "ADD TRIP";
                    }
                    else
                    {
                        AddTripBtn.Text = "ADD TRIP";
                        AddTripBtn.BackgroundColor = Color.FromArgb("#297CC0");
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
                            mileage = trips[0].mileage,
                            distance = trips[0].distance,
                            IsContinue = true,
                            trip_start = trips[0].trip_start
                        };

                        await Navigation.PushAsync(new Personal(personal));
                    }
                    if (trips[0].mode == "COMPANY")
                    {

                    }

                    if (trips[0].mode == "OTHER")
                    {

                    }

                    if (trips[0].mode == "PASSENGER PERSONAL")
                    {
                        await Navigation.PushAsync(new PersonalForceStop());
                    }

                    if (trips[0].mode == "PASSENGER COMPANY")
                    {

                    }
                }
                else
                {
                    //Force Stop
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
}
