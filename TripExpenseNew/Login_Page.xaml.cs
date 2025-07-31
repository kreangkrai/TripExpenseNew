namespace TripExpenseNew;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.DBService;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;

public partial class Login_Page : ContentPage
{
    private IAuthen Authen;
    private ILogin Login;
    private IServer Server;
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight;

    public Login_Page(IAuthen _Authen,ILogin _Login, IServer _Server)
	{
        InitializeComponent();
        Authen = _Authen;
        Login = _Login;
        Server = _Server;
        ServerModel servers = Server.Get(1).Result;
        if (servers == null)
        {
            LogInBtn.IsEnabled = false;
        }
        else
        {
            LoginModel login = Login.GetLogin(1).Result;
            if (login != null)
            {
                txt_name.Text = login.name;
                txt_password.Text = login.password;
                txt_server.Text = servers.server;
            }
        }
            _sheetHeight = BottomSheet.HeightRequest > 0 ? BottomSheet.HeightRequest : 300; // ใช้ HeightRequest หรือตั้งค่าเริ่มต้น
    }
    
    private void OnTapOpenBottomSheet(object sender, EventArgs e)
    {
        if (!_isOpen)
        {
            _isOpen = true;
            BottomSheet.IsVisible = true;
            AnimateBottomSheet(0); // เลื่อนขึ้นมา
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = BottomSheet.TranslationY;
                break;
            case GestureStatus.Running:
                double newY = Math.Max(_startY + e.TotalY, 0); // จำกัดไม่ให้เลื่อนเกินขอบบน
                newY = Math.Min(newY, _sheetHeight); // จำกัดไม่ให้เลื่อนเกินล่าง
                BottomSheet.TranslationY = newY;
                break;
            case GestureStatus.Completed:
                if (BottomSheet.TranslationY <= _sheetHeight / 2)
                {
                    AnimateBottomSheet(0); // เปิดถ้าเลื่อนไม่เกินครึ่ง
                }
                else
                {
                    AnimateBottomSheet(_sheetHeight); // ปิดถ้าเลื่อนเกินครึ่ง
                    _isOpen = false;
                }
                break;
        }
    }

    private void AnimateBottomSheet(double targetY)
    {
        uint duration = 250; // ระยะเวลาแอนิเมชัน (ms)
        BottomSheet.TranslateTo(0, targetY, duration, Easing.CubicOut);
    }

    private void ServerBtn_Clicked(object sender, EventArgs e)
    {
        if (!_isOpen)
        {
            _isOpen = true;
            BottomSheet.IsVisible = true;
            AnimateBottomSheet(0); // เลื่อนขึ้นมา
        }
        else
        {
            AnimateBottomSheet(_sheetHeight); // เลื่อนลงไปล่างสุด
            _isOpen = false;
        }
    }

    private async void ConnectBtn_Clicked(object sender, EventArgs e)
    {
        if (txt_server.Text.Trim() != "")
        {
            int message = await Server.Save(new ServerModel()
            {
                Id = 1,
                server = txt_server.Text.Trim()
            });

            Server = new ServerService();
            Authen = new AuthenService();

            ServerModel servers = Server.Get(1).Result;
            if (servers != null)
            {
                LogInBtn.IsEnabled = true;
                AnimateBottomSheet(_sheetHeight); // เลื่อนลงไปล่างสุด
                _isOpen = false;
            }

        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", "กรุณาใส่ url server","ตกลง");
            });
        }
    }

    private async void LogInBtn_Clicked(object sender, EventArgs e)
    {
        if (txt_name.Text.Trim().Length > 0 && txt_password.Text.Trim().Length > 0)
        {
            try
            {
                AuthenModel authen = await Authen.ActiveDirectoryAuthenticate(txt_name.Text, txt_password.Text);
                if (authen.authen == true)
                {
                    await Login.Save(new LoginModel()
                    {
                        name = txt_name.Text.Trim(),
                        password = txt_password.Text.Trim()
                    });
                    await Shell.Current.GoToAsync("Home_Page");
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", "ชื่อหรือรหัสผ่านไม่ถูกต้อง", "ตกลง");
                    });
                }
            }
            catch (Exception ex) 
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", ex.Message, "ตกลง");
                });
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", "กรุณากรอกให้ครบ", "ตกลง");
            });
        }
    }
}
