namespace TripExpenseNew;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;
using Plugin.LocalNotification;
using System;
using TripExpenseNew.CustomPopup;
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
    private IEmployee Employee;
    private IInternet Internet;
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight;

    public Login_Page(IAuthen _Authen,ILogin _Login, IServer _Server, IEmployee _Employee,IInternet _Internet)
	{
        InitializeComponent();
        Authen = _Authen;
        Login = _Login;
        Server = _Server;
        Employee = _Employee;
        Internet = _Internet;
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

        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
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
        bool internet = await CheckServerConnection($"{txt_server.Text}/api/CurrentTime/get");
        if (internet)
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
                Employee = new EmployeeService();

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
                    await DisplayAlert("", "กรุณาใส่ url server", "ตกลง");
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
    }

    public async Task<bool> CheckServerConnection(string serverUrl)
    {
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var response = await client.GetAsync(serverUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    private async void LogInBtn_Clicked(object sender, EventArgs e)
    {
        if (txt_name.Text.Trim().Length > 0 && txt_password.Text.Trim().Length > 0)
        {
            var popup = new ProgressPopup();
            this.ShowPopup(popup);
            try
            {
                AuthenModel authen = await Authen.ActiveDirectoryAuthenticate(txt_name.Text, txt_password.Text);
                if (authen.authen == true)
                {
                    List<EmployeeModel> employees = await Employee.GetEmployees();
                    LoginModel login = new LoginModel()
                    {
                        Id = 1,
                        name = txt_name.Text.Trim(),
                        password = txt_password.Text.Trim(),
                        emp_id = employees.Where(w => w.name.ToLower() == authen.user.ToLower()).Select(s=>s.emp_id).FirstOrDefault()
                    };
                    await Login.Save(login);
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
            await popup.CloseAsync();
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
