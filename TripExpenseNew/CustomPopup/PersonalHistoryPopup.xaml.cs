using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.Models;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.CustomPopup;

public partial class PersonalHistoryPopup : Popup
{
	List<PersonalViewModel> personals;
    private ObservableCollection<TripItems> tripItems = new ObservableCollection<TripItems>();
    CultureInfo cultureinfo = new CultureInfo("en-us");
    public PersonalHistoryPopup(List<PersonalViewModel> _personals)
	{
		InitializeComponent();
		personals = _personals;

	}

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Trip.Text = personals[0].trip;
        Text_Job.Text = personals[0].job_id;
        Text_Date.Text = personals[0].date.ToString("dd/MM/yyyy", cultureinfo);
        Text_Distance.Text = $"{personals[personals.Count-1].distance.ToString("#0.#")} km.";
        Text_MileageStart.Text = personals.FirstOrDefault().mileage.ToString();
        Text_MileageStop.Text  = personals.LastOrDefault().mileage.ToString();
        Text_Mode.Text = "PERSONAL";

        tripItems = new ObservableCollection<TripItems>();
       
        foreach (var ap in personals)
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