using System.Threading.Tasks;

namespace TripExpenseNew;

public partial class ModePage : ContentPage
{
	public ModePage()
	{
		InitializeComponent();
	}

    private async void PersonalCar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PersonalPage");
        //await Navigation.PushAsync(new PersonalPage());
    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Home_Page");
    }
}