using CommunityToolkit.Maui.Views;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;

namespace TripExpenseNew.CustomPopup;

public partial class PrivacyPolicyPopup : Popup
{
    private IPrivacy Privacy;
    private readonly TaskCompletionSource<bool> _tcs = new();
    string emp_d = string.Empty;
    string name = string.Empty;
    public PrivacyPolicyPopup(string _emp_id,string _name)
	{  
		InitializeComponent();
        Privacy = new PrivacyService();
        emp_d = _emp_id;
        name = _name;
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

        string result = await Privacy.Insert(new PrivacyModel()
        {
            emp_id = emp_d,
            name = name,
            date = DateTime.Now,

        });

        Close(result);
        AcceptBtn.IsEnabled = true;
    }
}