using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ILocationOther
    {
        Task<string> Insert(LocationOtherModel location);
        Task<List<LocationOtherModel>> GetByEmp(string emp_id);
    }
}
