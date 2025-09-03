using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IActivePublic
    {
        Task<int> Insert(ActivePublicModel p);
        Task<int> Delete(string trip);
        Task<List<ActivePublicModel>> GetByTrip(string trip);
    }
}
