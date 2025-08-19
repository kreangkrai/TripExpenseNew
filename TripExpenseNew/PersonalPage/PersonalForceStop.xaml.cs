namespace TripExpenseNew.PersonalPage;

public partial class PersonalForceStop : ContentPage
{
	public PersonalForceStop()
	{
		InitializeComponent();

        timePicker.Time = new TimeSpan(17, 30, 0); // ตั้งเวลาเริ่มต้นเป็น 17:30
    }

    private async void PersonalForceStopCancel_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Home_Page");
    }
}