using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralStopPopup : Popup
{
    public GeneralStopPopup(int mileage)
    {
        InitializeComponent();
        Text_Location.Text = "";  
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
        GeneralPopupStopModel g = new GeneralPopupStopModel()
        {
            location = Text_Location.Text,
        };
        Close(g);
        OKBtn.IsEnabled = true;
    }
}