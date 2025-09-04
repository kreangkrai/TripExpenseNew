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
        string location = Text_Location.Text;
        Close(location);
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        Close(null);
    }
}