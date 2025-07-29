using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ITracking
    {
        Task<string> Update(TrackingModel tracking);
        Task<TrackingModel> GetTracking();
    }
}
