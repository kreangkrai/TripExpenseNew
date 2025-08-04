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

public partial class Home_Page : ContentPage
{
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight;
    private ILastTrip LastTrip;
    private ILogin Login;
    LoginModel emp_id = new LoginModel();
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
            List<LastTripViewModel> trips = await GetLastTrip();
            if (trips != null)
            {
                _sheetHeight = LastTripBTS.HeightRequest > 0 ? LastTripBTS.HeightRequest : 300;
                if (trips[trips.Count - 1].driver_name.Length > 25)
                {
                    lbl_name.FontSize = 30;
                    lbl_lastname.FontSize = 30;
                }
                else
                {
                    lbl_name.FontSize = 34;
                    lbl_lastname.FontSize = 34;
                }

                lbl_name.Text = trips[trips.Count - 1].driver_name.Split(' ')[0];
                lbl_lastname.Text = trips[trips.Count - 1].driver_name.Split(' ')[1];

                txt_last_location.Text = trips[trips.Count - 1].location;
                txt_last_date.Text = trips[trips.Count - 1].trip.ToString("dd/MM/yyyy");
                txt_last_distance.Text = trips[trips.Count - 1].distance.ToString() + " km";
                txt_last_mileage.Text = trips[trips.Count - 1].mileage.ToString();
}
            else
            {
                await DisplayAlert("Error", "No trips found", "OK");
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
        await Navigation.PushAsync(new ModePage());
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
}
