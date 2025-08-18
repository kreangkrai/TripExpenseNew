namespace TripExpenseNew.CompanyPage;

public partial class CompanyPage : ContentPage
{
	public CompanyPage()
	{
		InitializeComponent();
	}
    private async void CompanyCancel_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Home_Page");
    }
}
