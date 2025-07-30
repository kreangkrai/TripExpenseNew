using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IPersonal
    {
        Task<int> Insert(PersonalDBModel personal);
        Task<int> Delete(DateTime trip);
        Task<List<PersonalDBModel>> GetByTrip(DateTime trip);
    }
}
