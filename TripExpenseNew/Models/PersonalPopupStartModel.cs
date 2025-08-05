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
        public string location { get; set; }
        public string job {  get; set; }
        public int mileage { get; set; }
    }
}
