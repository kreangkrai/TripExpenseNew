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
		 await Navigation.PushAsync(new PersonalPage());
    }
}