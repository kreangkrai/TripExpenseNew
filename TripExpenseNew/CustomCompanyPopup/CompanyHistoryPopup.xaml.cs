using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.CustomCompanyPopup;

public partial class CompanyHistoryPopup : Popup
{
    List<CompanyViewModel> companies;
    private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
    CultureInfo cultureinfo = new CultureInfo("en-us");
    public CompanyHistoryPopup(List<CompanyViewModel> _companies)
    {
        InitializeComponent();
        companies = _companies;

    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Trip.Text = companies[0].trip;
        Text_Job.Text = companies[0].job_id;
        Text_Date.Text = companies[0].date.ToString("dd/MM/yyyy", cultureinfo);
        Text_Car.Text = companies[0].car_id;
        Text_Distance.Text = $"{companies[companies.Count - 1].distance.ToString("#0.#")} km.";
        Text_MileageStart.Text = companies.FirstOrDefault().mileage.ToString();
        Text_MileageStop.Text = companies.LastOrDefault().mileage.ToString();
        Text_Mode.Text = "COMPANY";

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

    private void CloseBtn_Clicked(object sender, System.EventArgs e)
    {
        Close(null);
    }
}