using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Models
{
    public class AndroidParameterModel
    {
        public string geolocation_accuracy { get; set; }
        public int timeout { get; set; }
        public int accuracy_meter { get; set; }
        public int accuracy_course { get; set; }
    }
}
