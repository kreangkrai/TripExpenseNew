using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IBorrower
    {
        string Insert(BorrowerModel borrower);
        string Update(BorrowerModel borrower);
        List<BorrowerViewModel> GetBorrowers();
        BorrowerViewModel GetBorrowerByCar(string car_id);
    }
}
