using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Devices.Sensors;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralCheckinLocationPopup : Popup
{
    public GeneralCheckinLocationPopup()
    {
        InitializeComponent();
        Text_Location.Text = "";
    }
    private void OnOKButtonClicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        string location = Text_Location.Text;
        Close(location);
        OKBtn.IsEnabled = true;
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }
}