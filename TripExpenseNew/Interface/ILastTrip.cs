using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILastTrip
    {
        string Insert(LastTripModel trip);
        string Delete(string emp_id);
        List<LastTripViewModel> GetByEmp(string emp_id);
    }
}
