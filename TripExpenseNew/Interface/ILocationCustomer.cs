using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILocationCustomer
    {
        string Insert(LocationCustomerModel location);
        List<LocationCustomerModel> GetByEmp(string emp_id);
    }
}
