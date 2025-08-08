namespace TripExpenseNew;

public partial class AddPassengerPersonal_Page : ContentPage
{
	public AddPassengerPersonal_Page(string driver)
	{
		InitializeComponent();
        text_Driver.Text = driver;
	}
    private async void CancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Personal");
    }
    private void OnGoToActiveTripPageClicked(object sender, EventArgs e)
    {
        
    }
}