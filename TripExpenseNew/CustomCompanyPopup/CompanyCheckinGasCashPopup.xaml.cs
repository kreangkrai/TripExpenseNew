using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomCompanyPopup;

public partial class CompanyCheckinGasCashPopup : Popup
{
    public CompanyCheckinGasCashPopup()
    {
        InitializeComponent();
    }
    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }
    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        string customer = Text_Gas.Text;
        double cash = Text_Cash.Text == "" ? 0 : Convert.ToDouble(Text_Cash.Text);
        int mileage = Text_Mileage.Text == "" ? 0 : Convert.ToInt32(Text_Mileage.Text);
        if (customer.Trim() != "" && Text_Gas.Text != null && mileage != 0)
        {
            Tuple<string, double,int> data = new Tuple<string, double,int>(customer, cash,mileage);
            Close(data);
        }
        else
        {
            Close(null);
        }
        OKBtn.IsEnabled = true;
    }
}