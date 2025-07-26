using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ITracking
    {
        string Update(TrackingModel tracking);
        TrackingModel GetTracking();
    }
}
