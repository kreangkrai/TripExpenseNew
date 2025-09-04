using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralHistoryPopup : Popup
{
    GeneralViewModel data;
    public GeneralHistoryPopup(GeneralViewModel _data)
    {
        InitializeComponent();
        data = _data;
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Trip.Text = data.trip;
        Text_Date.Text = data.date;
        Text_Distance.Text = $"{data.distance.ToString("#0.#")} km.";
        Text_MileageStart.Text = data.mileage_start.ToString();
        Text_MileageStop.Text = data.mileage_stop.ToString();
        Text_Mode.Text = "GENERAL";
    }

    private void CloseBtn_Clicked(object sender, System.EventArgs e)
    {
        Close(null);
    }
}