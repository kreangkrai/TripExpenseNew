using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IBorrowerLog
    {
        Task<string> Insert(BorrowerLogModel borrower);
        Task<List<BorrowerLogViewModel>> GetBorrowers();
        Task<List<BorrowerLogViewModel>> GetBorrowerByCar(string car_id);
    }
}
