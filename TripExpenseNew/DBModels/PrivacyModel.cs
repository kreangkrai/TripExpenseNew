using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.DBModels
{
    public class PrivacyModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int accept { get; set; }
    }
}
