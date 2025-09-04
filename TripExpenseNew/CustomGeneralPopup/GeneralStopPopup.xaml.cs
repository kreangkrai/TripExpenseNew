using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralStopPopup : Popup
{
    public GeneralStopPopup(int mileage)
    {
        InitializeComponent();
        Text_Location.Text = "";      
        Text_mileage.Text = $"Mileage Start: {mileage}";     
    }
    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        GeneralPopupStopModel g = new GeneralPopupStopModel()
        {
            location = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0
        };
        Close(g);
    }
}