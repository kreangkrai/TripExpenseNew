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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AddTripBtn.Text = "CONTINUE";
                    });                   
                }

                _sheetHeight = LastTripBTS.HeightRequest > 0 ? LastTripBTS.HeightRequest : 300;
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
                if (trips[0].mode == "PERSONAL")
                {
                    PersonalPopupStartModel personal = new PersonalPopupStartModel()
                    {
                        IsCustomer = false,
                        job_id = trips[0].job_id,
                        location = null,
                        trip = trips[0].trip,
                        location_name = trips[0].location,
                        mileage = trips[0].mileage,
                        distance = trips[0].distance,
                        IsContinue = true
                    };

                    await Navigation.PushAsync(new Personal(personal));
                }
            }
            else
            {
                await Navigation.PushAsync(new ModePage());
            }
        }
    }

    private void OnTapOpenBottomSheet(object sender, EventArgs e)
    {
        if (!_isOpen)
        {
            _isOpen = true;
            LastTripBTS.IsVisible = true;
            AnimateBottomSheet(0); // เลื่อนขึ้นมา
        }
    }

    private void OnCloseBottomSheet(object sender, EventArgs e)
    {
        AnimateBottomSheet(_sheetHeight); // เลื่อนลงไปล่างสุด
        _isOpen = false;
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = LastTripBTS.TranslationY;
                break;
            case GestureStatus.Running:
                double newY = Math.Max(_startY + e.TotalY, 0); // จำกัดไม่ให้เลื่อนเกินขอบบน
                newY = Math.Min(newY, _sheetHeight); // จำกัดไม่ให้เลื่อนเกินล่าง
                LastTripBTS.TranslationY = newY;
                break;
            case GestureStatus.Completed:
                if (LastTripBTS.TranslationY > _sheetHeight / 2)
                {
                    AnimateBottomSheet(_sheetHeight); // ปิดถ้าเลื่อนเกินครึ่ง
                    _isOpen = false;
                }
                else
                {
                    AnimateBottomSheet(0); // เปิดถ้าเลื่อนไม่เกินครึ่ง
                }
                break;
        }
    }

    private void AnimateBottomSheet(double targetY)
    {
        uint duration = 250; // ระยะเวลาแอนิเมชัน (ms)
        LastTripBTS.TranslateTo(0, targetY, duration, Easing.CubicInOut);
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
