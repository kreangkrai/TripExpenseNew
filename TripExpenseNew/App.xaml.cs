using Plugin.LocalNotification;
using TripExpenseNew.Interface;

namespace TripExpenseNew
{
    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();
            // UserAppTheme = AppTheme.Unspecified;  // ตามระบบ (Light หรือ Dark)
            UserAppTheme = AppTheme.Light;  // บังคับ Light Mode
                                                  // หรือ UserAppTheme = AppTheme.Dark;   // บังคับ Dark Mode
            MainPage = new AppShell();
            //MainPage = mainPage;
        }
    }
}
