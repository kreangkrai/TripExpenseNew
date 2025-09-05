namespace TripExpenseNew;

public partial class Initial_Page : ContentPage
{
	public Initial_Page()
	{
        InitializeComponent();

    }
    private async void OnGoToLoginPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Login_Page");
    }

    private async void GeneralBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("General");
    }
}