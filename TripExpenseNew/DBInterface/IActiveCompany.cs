using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IActiveCompany
    {
        Task<int> Insert(ActiveCompanyModel company);
        Task<int> Delete(string trip);
        Task<List<ActiveCompanyModel>> GetByTrip(string trip);
    }
}
