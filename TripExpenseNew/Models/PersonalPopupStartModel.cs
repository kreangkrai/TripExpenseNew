using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.Models
{
    public class PersonalPopupStartModel
    {
        public bool IsCustomer {  get; set; }
        public bool IsContinue {  get; set; }
        public string location_name { get; set; }
        public string job_id {  get; set; }
        public DateTime trip_start { get; set; }
        public int mileage { get; set; }
        public Location location { get; set; }
        public string trip {  get; set; }
        public double distance { get; set; }
    }
}
