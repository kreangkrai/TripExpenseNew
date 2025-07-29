using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILocationCustomer
    {
        Task<string> Insert(LocationCustomerModel location);
        Task<List<LocationCustomerModel>> GetByEmp(string emp_id);
    }
}
