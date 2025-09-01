using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.CustomPopup;

public partial class PassengerCompanyHistoryPopup : Popup
{
    List<PassengerCompanyViewModel> companies;
    private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
    CultureInfo cultureinfo = new CultureInfo("en-us");
    public PassengerCompanyHistoryPopup(List<PassengerCompanyViewModel> _companies)
    {
        InitializeComponent();
        companies = _companies;
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Trip.Text = companies[0].trip;
        Text_Job.Text = companies[0].job_id;
        Text_Car.Text = companies[0].car_id;
        Text_Driver.Text = companies[0].driver_name;
        Text_Date.Text = companies[0].date.ToString("dd/MM/yyyy", cultureinfo);
        Text_Mode.Text = "PASSENGER COMPANY";

        tripItems = new ObservableCollection<TripItems>();

        foreach (var ap in companies)
        {
            Color color = new Color();
            if (ap.status == "START")
            {
                color = Color.FromRgb(255, 255, 255);
            }
            else
            {
                color = Color.FromRgb(255, 255, 255);
            }
            TripItems trip_item = new TripItems()
            {
                FrameColor = color,
                TextStatus = ap.status,
                IconLocationSource = "route.png",
                TextLocation = $"Location: {ap.location}",
                IconDateSource = "clock.png",
                TextDate = $"Date: {ap.date.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo)}"
            };

            tripItems.Add(trip_item);
        }

        TripCollectionView.ItemsSource = tripItems;
    }

    private void CloseBtn_Clicked(object sender, EventArgs e)
    {
        Close(null);
    }
}