namespace TripExpenseNew.PersonalPage;

public partial class PersonalForceStop : ContentPage
{
	public PersonalForceStop()
	{
		InitializeComponent();
	}
    private async void PersonalCancel_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync("Home_Page");
    }
}