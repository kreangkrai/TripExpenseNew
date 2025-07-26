using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPersonal
    {
        string Insert(PersonalModel data);
        string Insert(List<PersonalModel> datas);
        List<PersonalViewModel> GetPersonalByMonth(string driver,string month);
    }
}
