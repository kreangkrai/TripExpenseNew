using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Models
{
    public class GeneralViewModel
    {
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public string trip { get; set; }
        public string date { get; set; }
        public double distance { get; set; }       
        public string location { get; set; }
        public int mileage_start { get; set; }
        public int mileage_stop { get; set; }
    }
}
