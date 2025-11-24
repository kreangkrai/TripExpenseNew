namespace TripExpenseNew;

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System.Net.Http;
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
    private readonly ILogin Login;
    private readonly IServer Server;
    private IEmployee Employee;
    private IInternet Internet;
    private IPrivacy Privacy;
    private bool _isOpen = false;
    private double _startY;
    private double _sheetHeight = 600;

    private static readonly HttpClient _httpClient = CreateHttpClient();
    List<PrivacyModel> privacies = new List<PrivacyModel>();
    public Login_Page(ILogin _Login, IServer _Server)
    {
        InitializeComponent();
        Login = _Login;
        Server = _Server;
       
        _ = LoadInitialDataAsync();

    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            var servers = await Server.Get(1);
            if (servers == null)
            {
                LogInBtn.IsEnabled = false;
                return;
            }
            else
            {
                Employee = new EmployeeService();
                Privacy = new PrivacyService();
                Authen = new AuthenService();
                Internet = new InternetService();
            }

                var login = await Login.GetLogin(1);
            if (login != null)
            {
                txt_name.Text = login.name;
                txt_password.Text = login.password;
                txt_server.Text = servers.server;
            }

            LogInBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            });
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
           
        await RequestPermissionsAsync();
    }

    private async Task RequestPermissionsAsync()
    {
        // Location
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.LocationAlways>();

        // Notification
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
            await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    // --- Bottom Sheet ---
    private void ServerBtn_Clicked(object sender, EventArgs e) => ToggleBottomSheet();
    private void ToggleBottomSheet()
    {
        _isOpen = !_isOpen;
        BottomSheet.IsVisible = true;
        AnimateBottomSheet(_isOpen ? 0 : _sheetHeight);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = BottomSheet.TranslationY;
                break;
            case GestureStatus.Running:
                double newY = Math.Clamp(_startY + e.TotalY, 0, _sheetHeight);
                BottomSheet.TranslationY = newY;
                break;
            case GestureStatus.Completed:
                if (BottomSheet.TranslationY <= _sheetHeight / 2)
                    AnimateBottomSheet(0);
                else
                {
                    AnimateBottomSheet(_sheetHeight);
                    _isOpen = false;
                }
                break;
        }
    }

    private void AnimateBottomSheet(double targetY)
    {
        uint duration = 300;
        BottomSheet.TranslateTo(0, targetY, duration, Easing.CubicOut);
    }

    // --- เชื่อมต่อ Server ---
    private async void ConnectBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txt_server.Text))
        {
            await DisplayAlert("", "Please insert URL", "OK");
            return;
        }

        ShowLoading(true);

        string url = $"{txt_server.Text.TrimEnd('/')}/api/CurrentTime/get";
        bool isConnected = await CheckServerConnectionAsync(url);

        if (isConnected)
        {
            await Server.Save(new ServerModel { Id = 1, server = txt_server.Text.Trim() });
            LogInBtn.IsEnabled = true;
            ToggleBottomSheet();

            await DisplayAlert("", "Connect Success!", "OK");
        }
        else
        {
            await DisplayAlert("", "Cannot Connect to Server", "OK");
        }

        ShowLoading(false);
    }

    private async Task<bool> CheckServerConnectionAsync(string url)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var response = await _httpClient.GetAsync(url, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CheckServer] {ex.Message}");
            return false;
        }
    }

    // --- Login ---
    private async void LogInBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txt_name.Text) || string.IsNullOrWhiteSpace(txt_password.Text))
        {
            await DisplayAlert("", "Please Input User and Password", "OK");
            return;
        }

        Employee = new EmployeeService();
        Privacy = new PrivacyService();
        Authen = new AuthenService();
        Internet = new InternetService();

        LogInBtn.IsEnabled = false;
        ShowLoading(true);

        try
        {
            var authen = await Authen.ActiveDirectoryAuthenticate(txt_name.Text.Trim(), txt_password.Text.Trim());

            if (authen?.authen == true)
            {
                var employees = await Employee.GetEmployees();
                var emp = employees.FirstOrDefault(x =>
                    x.name.Equals(authen.user, StringComparison.OrdinalIgnoreCase));

                if (emp != null)
                {
                    var login = new LoginModel
                    {
                        Id = 1,
                        name = txt_name.Text.Trim(),
                        password = txt_password.Text.Trim(),
                        emp_id = emp.emp_id
                    };

                    await Login.Save(login);
                   

                    // Check Privacy
                    privacies = await Privacy.GetPrivacies();
                    if (privacies.Count > 0)
                    {
                        PrivacyModel privacy = privacies.Where(w=>w.emp_id == emp.emp_id).FirstOrDefault();
                        if (privacy == null)
                        {
                            var result = await this.ShowPopupAsync(new PrivacyPolicyPopup(emp.emp_id,emp.name));
                        }
                    }
                    else
                    {
                        var result = await this.ShowPopupAsync(new PrivacyPolicyPopup(emp.emp_id, emp.name));
                    }

                    await Shell.Current.GoToAsync("Home_Page");
                }
                else
                {
                    await DisplayAlert("", "Not Authorized", "OK");
                }
            }
            else
            {
                await DisplayAlert("", "user or password incorrect", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("", $"Error: {ex.Message}", "OK");
        }
        finally
        {
            ShowLoading(false);
            LogInBtn.IsEnabled = true;
        }
    }

 
    // --- ฟังก์ชันช่วย ---
    private void ShowLoading(bool show)
    {
        LoadingOverlay.IsVisible = show;
    }
}