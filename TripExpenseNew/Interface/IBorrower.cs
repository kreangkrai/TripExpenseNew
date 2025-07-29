using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IBorrower
    {
        Task<string> Insert(BorrowerModel borrower);
        Task<string> Update(BorrowerModel borrower);
        Task<List<BorrowerViewModel>> GetBorrowers();
        Task<BorrowerViewModel> GetBorrowerByCar(string car_id);
    }
}
