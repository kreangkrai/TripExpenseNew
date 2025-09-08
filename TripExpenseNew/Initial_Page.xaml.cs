namespace TripExpenseNew;

public partial class Initial_Page : ContentPage
{
	public Initial_Page()
	{
        InitializeComponent();

    }
    private async void OnGoToLoginPageClicked(object sender, EventArgs e)
    {
        EnterpriseBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("Login_Page");
        EnterpriseBtn.IsEnabled = true;
    }

    private async void GeneralBtn_Clicked(object sender, EventArgs e)
    {
        GeneralBtn.IsEnabled = false;
        await Shell.Current.GoToAsync("General");
        GeneralBtn.IsEnabled = true;
    }
}