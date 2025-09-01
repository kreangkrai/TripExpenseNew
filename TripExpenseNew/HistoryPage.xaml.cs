using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;
using System.Globalization;
using TripExpenseNew.CustomPopup;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;
using TripExpenseNew.ViewModels;
namespace TripExpenseNew;

public partial class HistoryPage : ContentPage
{
    private IPersonal Personal;
    private ICompany Company;
    private IPassengerPersonal PassengerPersonal;
    private IPassengerCompany PassengerCompany;
    CultureInfo cultureinfo = new CultureInfo("en-us");
    List<LastTripViewModel> lastTrips = new List<LastTripViewModel>();
    ObservableCollection<HistoryItems>  tripItems = new ObservableCollection<HistoryItems>();
    public HistoryPage(List<LastTripViewModel> _lastTrips)
	{
		InitializeComponent();
        lastTrips = _lastTrips;
        Personal = new PersonalService();
        Company = new CompanyService();
        PassengerPersonal = new PassengerPersonalService();
        PassengerCompany = new PassengerCompanyService();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        for (int i = 0; i < lastTrips.Count; i++)
        {
            HistoryItems item = new HistoryItems()
            {
                TextTrip = lastTrips[i].trip_start.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo),
                IconMileageSource = "tachometer.png",
                IconDistanceSource = "route.png",
                IconLocationSource = "map_pin.png",
                TextDistance = $"{lastTrips[i].distance.ToString("#0.#")} km.",
                TextLocation = lastTrips[i].location,
                TextMileage = $"{lastTrips[i].mileage_start} - {lastTrips[i].mileage_stop}"              
            };
            tripItems.Add(item);
        }
        
        HistoryTripCollectionView.ItemsSource = tripItems;
    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Home_Page");
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is Frame frame && frame.BindingContext is HistoryItems selectedTrip)
            {
                LastTripViewModel lastTrip = lastTrips.Where(w => w.trip_start.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo) == selectedTrip.TextTrip).FirstOrDefault();
                if (lastTrip.mode == "PERSONAL")
                {
                    List<PersonalViewModel> datas = await Personal.GetPersonalHistoryByTrip(lastTrip.emp_id, lastTrip.trip);
                    datas = datas.OrderBy(o => o.date).ToList();
                    await this.ShowPopupAsync(new PersonalHistoryPopup(datas));
                }
                if (lastTrip.mode == "COMPANY")
                {
                    List<CompanyViewModel> datas = await Company.GetCompanyDriverHistoryByTrip(lastTrip.emp_id, lastTrip.trip);
                    datas = datas.OrderBy(o => o.date).ToList();
                    await this.ShowPopupAsync(new CompanyHistoryPopup(datas));
                }
                if (lastTrip.mode == "PASSENGER PERSONAL")
                {
                    List<PassengerPersonalViewModel> datas = await PassengerPersonal.GetPassengerPersonalHistoryByTrip(lastTrip.emp_id, lastTrip.trip);
                    datas = datas.OrderBy(o => o.date).ToList();
                    await this.ShowPopupAsync(new PassengerPersonalHistoryPopup(datas));
                }
                if (lastTrip.mode == "PASSENGER COMPANY")
                {
                    List<PassengerCompanyViewModel> datas = await PassengerCompany.GetPassengerCompanyHistoryByTrip(lastTrip.emp_id, lastTrip.trip);
                    datas = datas.OrderBy(o => o.date).ToList();
                    await this.ShowPopupAsync(new PassengerCompanyHistoryPopup(datas));
                }
                if (lastTrip.mode == "PUBLIC")
                {

                }
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("", ex.Message, "OK");
            });
        }
    }
    private void HistoryTripCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //if (e.CurrentSelection.FirstOrDefault() is HistoryItems selectedTrip)
        //{
        //    LastTripViewModel lastTrip = lastTrips.Where(w=>w.trip_start.ToString("dd/MM/yyyy HH:mm:ss", cultureinfo) == selectedTrip.TextTrip).FirstOrDefault();
        //    if (lastTrip.mode == "PERSONAL")
        //    {
        //        List<PersonalViewModel> datas = await Personal.GetPersonalHistoryByTrip(lastTrip.driver, lastTrip.trip);
        //        datas = datas.OrderBy(o=>o.date).ToList();
        //        await this.ShowPopupAsync(new PersonalHistoryPopup(datas));
        //    }
        //    if (lastTrip.mode == "COMPANY")
        //    {
        //        List<CompanyViewModel> datas = await Company.GetCompanyDriverHistoryByTrip(lastTrip.driver, lastTrip.trip);
                
        //    }
        //    if (lastTrip.mode == "PASSENGER PERSONAL")
        //    {
        //        List<PassengerPersonalViewModel> datas = await PassengerPersonal.GetPassengerPersonalHistoryByTrip(lastTrip.driver, lastTrip.trip);
        //        datas = datas.OrderBy(o => o.date).ToList();
        //        await this.ShowPopupAsync(new PassengerPersonalHistoryPopup(datas));
        //    }
        //    if (lastTrip.mode == "PASSENGER COMPANY")
        //    {
        //        List<PassengerCompanyViewModel> datas = await PassengerCompany.GetPassengerCompanyHistoryByTrip(lastTrip.driver, lastTrip.trip);
        //    }

        //    //await Shell.Current.GoToAsync($"//TripDetailPage?tripId={selectedTrip.Id}");
        //}

        //HistoryTripCollectionView.SelectedItem = null;
    }
}