using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TripExpenseNew.DBModels
{
    public class ServerModel
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string server { get; set; }
    }
}
