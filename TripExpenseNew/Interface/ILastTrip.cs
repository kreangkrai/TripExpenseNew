using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILastTrip
    {
        Task<string> Insert(LastTripModel trip);
        Task<string> Delete(string emp_id);
        Task<List<LastTripViewModel>> GetByEmp(string emp_id);
    }
}
