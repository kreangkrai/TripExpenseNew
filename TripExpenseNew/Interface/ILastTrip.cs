using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILastTrip
    {
        Task<string> Insert(LastTripModel trip);
        Task<string> DeleteByEmp(string emp_id);
        Task<string> DeleteByTrip(string trip);
        Task<string> UpdateByTrip(LastTripModel trip);
        Task<List<LastTripViewModel>> GetByEmp(string emp_id);
        Task<List<LastTripViewModel>> GetByTrip(string trip);
        Task<List<string>> GetAvailable();
        Task<List<string>> GetInUse();
    }
}
