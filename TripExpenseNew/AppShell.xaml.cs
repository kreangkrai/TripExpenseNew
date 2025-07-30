namespace TripExpenseNew
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Initial_Page", typeof(Initial_Page));
            Routing.RegisterRoute("Login_Page", typeof(Login_Page));
            Routing.RegisterRoute("Home_Page", typeof(Home_Page));
        }
    }
}
