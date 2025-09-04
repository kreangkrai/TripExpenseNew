using TripExpenseNew.PersonalPage;

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
            Routing.RegisterRoute("Personal", typeof(Personal));
            Routing.RegisterRoute("PersonalPage", typeof(PersonalPage.PersonalPage));
            Routing.RegisterRoute("PersonalForceStop", typeof(PersonalPage.PersonalForceStop));
            Routing.RegisterRoute("CompanyPage", typeof(CompanyPage.CompanyPage));
            Routing.RegisterRoute("PublicPage", typeof(PublicPage.PublicPage));
            Routing.RegisterRoute("GeneralPage", typeof(GeneralPage.GeneralPage));
        }
    }
}
