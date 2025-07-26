using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IOther
    {
        string Insert(OtherModel data);
        string Insert(List<OtherModel> datas);
        List<OtherViewModel> GetOtherByMonth(string passenger, string month);
    }
}
