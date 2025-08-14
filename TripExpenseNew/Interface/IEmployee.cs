using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IEmployee
    {
        Task<string> Insert(EmployeeModel data);
        Task<string> Insert(List<EmployeeModel> datas);
        Task<string> Update(EmployeeModel data);
        Task<List<EmployeeModel>> GetEmployees();
        Task<EmployeeModel> GetEmployeeByName(string name);
    }
}
