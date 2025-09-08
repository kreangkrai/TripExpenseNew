using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralStartPopup : Popup
{
    public GeneralStartPopup()
    {
        InitializeComponent();
        Text_Location.Text = "";
        Text_Mileage.Text = "0";
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }

    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        GeneralPopupStartModel g = new GeneralPopupStartModel()
        {
            location_name = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0
        };
        Close(g);
        OKBtn.IsEnabled = true;
    }
}