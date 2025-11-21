using CommunityToolkit.Maui.Views;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBService;

namespace TripExpenseNew.CustomPopup;

public partial class PrivacyPolicyPopup : Popup
{
    private IPrivacy Privacy;
    private readonly TaskCompletionSource<bool> _tcs = new();
    public PrivacyPolicyPopup()
	{  
		InitializeComponent();
        Privacy = new PrivacyService();
        AcceptCheckBox.CheckedChanged += AcceptCheckBox_CheckedChanged;
    }

    private void AcceptCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        AcceptBtn.IsEnabled = e.Value;
        AcceptBtn.Opacity = e.Value ? 1.0 : 0.5;
    }

    private async void OnPrivacyPolicyLinkTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync("http://ctracking.contrologic.co.th/policy_te"); // URL
        }
        catch { }
    }

    private void OnNotNowClicked(object sender, EventArgs e)
    {
        NotNowBtn.IsEnabled = false;
        Close(null);
        NotNowBtn.IsEnabled = true;
    }

    private async void AcceptBtn_Clicked(object sender, EventArgs e)
    {
        AcceptBtn.IsEnabled = false;

        int result = await Privacy.Save(new DBModels.PrivacyModel()
        {
            accept = 1,
        });

        Close(result);
        AcceptBtn.IsEnabled = true;
    }
}