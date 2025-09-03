using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.CustomPublicPopup;

public partial class PublicHistoryPopup : Popup
{
    List<PublicViewModel> publics;
    private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
    CultureInfo cultureinfo = new CultureInfo("en-us");
    public PublicHistoryPopup(List<PublicViewModel> _publics)
    {
        InitializeComponent();
        publics = _publics;

    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Trip.Text = publics[0].trip;
        Text_Job.Text = publics[0].job_id;
        Text_Date.Text = publics[0].date.ToString("dd/MM/yyyy", cultureinfo);
        Text_Distance.Text = $"{publics[publics.Count - 1].distance.ToString("#0.#")} km.";
        Text_Mode.Text = "PUBLIC";

        tripItems = new ObservableCollection<TripItems>();

        foreach (var ap in publics)
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