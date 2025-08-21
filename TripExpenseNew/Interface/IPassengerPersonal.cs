using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPassengerPersonal
    {
        Task<string> Insert(PassengerPersonalModel data);
        Task<string> Insert(List<PassengerPersonalModel> datas);
        Task<List<PassengerPersonalViewModel>> GetPassengerPersonalByMonth(string passenger, string month);
        Task<List<PassengerPersonalViewModel>> GetPassengerPersonalByDriver(string driver, string trip);
        Task<List<PassengerPersonalViewModel>> GetPassengerPersonalHistoryByTrip(string passenger, string trip);
    }
}
