using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPassengerCompany
    {
        string Insert(PassengerCompanyModel data);
        string Insert(List<PassengerCompanyModel> datas);
        List<PassengerCompanyViewModel> GetPassengerCompanyByMonth(string passenger, string month);
    }
}
