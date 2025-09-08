using System.Threading.Tasks;
using TripExpenseNew.CompanyPage;
namespace TripExpenseNew;

public partial class ModePage : ContentPage
{
	public ModePage()
	{
		InitializeComponent();
	}

    private async void PersonalCar_Clicked(object sender, EventArgs e)
    {
        PersonalCarBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("PersonalPage");
        //await Navigation.PushAsync(new PersonalPage());
        PersonalCarBtn.IsEnabled = true;
    }
    private async void CompanyCar_Clicked(object sender, EventArgs e)
    {
        CompanyCarBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("CompanyPage");
        //await Navigation.PushAsync(new CompanyPage());
        CompanyCarBtn.IsEnabled = true;
    }
    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("Home_Page");
        CancelBtn.IsEnabled = true;
    }

    private async void PublicBtn_Clicked(object sender, EventArgs e)
    {
        PublicBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("PublicPage");
        PublicBtn.IsEnabled = true;
    }
}