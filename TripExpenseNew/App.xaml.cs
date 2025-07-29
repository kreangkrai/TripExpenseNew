using TripExpenseNew.Interface;

namespace TripExpenseNew
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = mainPage;
        }
    }
}
