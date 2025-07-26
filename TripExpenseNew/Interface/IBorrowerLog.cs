using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IBorrowerLog
    {
        string Insert(BorrowerLogModel borrower);
        List<BorrowerLogViewModel> GetBorrowers();
        List<BorrowerLogViewModel> GetBorrowerByCar(string car_id);
    }
}
