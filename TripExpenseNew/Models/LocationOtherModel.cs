using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Models
{
    public class LocationOtherModel
    {
        public string location_id { get; set; }
        public string location { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string zipcode { get; set; }
        public string emp_id { get; set; }
    }
}
