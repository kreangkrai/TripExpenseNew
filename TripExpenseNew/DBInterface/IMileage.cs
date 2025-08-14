using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IMileage
    {
        Task<int> Save(MileageDBModel mileage);
        Task<MileageDBModel> GetMileage(int id);
    }
}
