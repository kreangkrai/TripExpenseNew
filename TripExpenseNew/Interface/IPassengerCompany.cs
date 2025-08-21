using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPassengerCompany
    {
        Task<string> Insert(PassengerCompanyModel data);
        Task<string> Insert(List<PassengerCompanyModel> datas);
        Task<List<PassengerCompanyViewModel>> GetPassengerCompanyByMonth(string passenger, string month);
        Task<List<PassengerCompanyViewModel>> GetPassengerCompanyByDriver(string driver, string trip);
        Task<List<PassengerCompanyViewModel>> GetPassengerCompanyHistoryByTrip(string passenger, string trip);
    }
}
