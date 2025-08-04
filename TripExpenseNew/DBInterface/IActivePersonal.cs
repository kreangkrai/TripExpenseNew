using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IActivePersonal
    {
        Task<int> Insert(ActivePersonalModel personal);
        Task<int> Delete(DateTime trip);
        Task<List<ActivePersonalModel>> GetByTrip(DateTime trip);
    }
}
