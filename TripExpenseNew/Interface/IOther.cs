using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IOther
    {
        Task<string> Insert(OtherModel data);
        Task<string> Insert(List<OtherModel> datas);
        Task<List<OtherViewModel>> GetOtherByMonth(string passenger, string month);
    }
}
