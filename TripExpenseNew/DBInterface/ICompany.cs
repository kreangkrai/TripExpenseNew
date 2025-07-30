using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface ICompany
    {
        Task<int> Insert(CompanyDBModel company);
        Task<int> Delete(DateTime trip);
        Task<List<CompanyDBModel>> GetByTrip(DateTime trip);
    }
}
