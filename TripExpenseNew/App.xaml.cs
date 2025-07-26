using TripExpenseNew.Interface;

namespace TripExpenseNew
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}
