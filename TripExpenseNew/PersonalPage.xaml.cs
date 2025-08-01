namespace TripExpenseNew;

public partial class PersonalPage : ContentPage
{
	public PersonalPage()
	{
		InitializeComponent();
	}

    private async void PersonalStart_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Personal");
    }

    private async void PersonalCancel_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Home_Page());
    }
}