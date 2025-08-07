using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IPersonal
    {
        Task<string> Insert(PersonalModel data);
        Task<string> Inserts(List<PersonalModel> datas);
        Task<List<PersonalViewModel>> GetPersonalByMonth(string driver,string month);
    }
}
