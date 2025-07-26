using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPassengerPersonal
    {
        string Insert(PassengerPersonalModel data);
        string Insert(List<PassengerPersonalModel> datas);
        List<PassengerPersonalViewModel> GetPassengerPersonalByMonth(string passenger, string month);
    }
}
