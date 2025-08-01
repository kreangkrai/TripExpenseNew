namespace TripExpenseNew;

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using static System.Net.Mime.MediaTypeNames;

public partial class Home_Page : ContentPage
{
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight;
    public Home_Page()
    {
        InitializeComponent();
        _sheetHeight = LastTripBTS.HeightRequest > 0 ? LastTripBTS.HeightRequest : 300;
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
