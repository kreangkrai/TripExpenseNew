using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IEmployee
    {
        string Insert(EmployeeModel data);
        string Insert(List<EmployeeModel> datas);
        string Update(EmployeeModel data);
        List<EmployeeModel> GetEmployees();
    }
}
