using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IPublic
    {
        Task<int> Insert(PublicDBModel p);
        Task<int> Delete(string trip);
        Task<List<PublicDBModel>> GetByTrip(string trip);
    }
}
