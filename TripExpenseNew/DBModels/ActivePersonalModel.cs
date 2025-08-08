using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.DBModels
{
    public class ActivePersonalModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string driver { get; set; }
        public DateTime trip { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public double distance { get; set; }       
        public string location { get; set; }
        public int mileage { get; set; }
       
    }
}
