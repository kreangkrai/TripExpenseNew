using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Models
{
    public class KalmanParameterModel
    {
        public double process_noise {  get; set; }
        public double measurement_noise { get; set; }
    }
}
