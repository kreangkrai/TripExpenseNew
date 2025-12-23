using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPopup;

public partial class PolicyPopup : Popup
{
	public PolicyPopup()
	{
		InitializeComponent();
        AcceptCheckBox.CheckedChanged += AcceptCheckBox_CheckedChanged;
    }
    private void AcceptCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        AcceptBtn.IsEnabled = e.Value;
        AcceptBtn.Opacity = e.Value ? 1.0 : 0.5;
    }
    private void AcceptBtn_Clicked(object sender, EventArgs e)
    {
        AcceptBtn.IsEnabled = false;
        Close(true);
        AcceptBtn.IsEnabled = true;
    }

    //private void NotNowBtn_Clicked(object sender, EventArgs e)
    //{
    //    NotNowBtn.IsEnabled = false;
    //    Close(null);
    //    NotNowBtn.IsEnabled = true;
    //}

    private async void OnPrivacyPolicyLinkTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync("https://sites.google.com/view/ctl-te-privacy-policy"); // URL
        }
        catch { }
    }
}