using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IKalman
    {
        Task<KalmanParameterModel> GetParameter();
    }
}
