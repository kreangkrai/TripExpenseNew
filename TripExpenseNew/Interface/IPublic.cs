using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPublic
    {
        Task<string> Insert(PublicModel data);
        Task<string> Inserts(List<PublicModel> datas);
        Task<List<PublicViewModel>> GetPublicByMonth(string passenger, string month);
        Task<List<PublicViewModel>> GetPublicHistoryByTrip(string passenger, string trip);
    }
}
