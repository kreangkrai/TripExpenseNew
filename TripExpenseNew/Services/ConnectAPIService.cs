using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Interface;

namespace TripExpenseNew.Services
{
    public class ConnectAPIService : IConnectAPI
    {
        public string ConnectAPI()
        {
            return "http://192.168.15.12/tripexpenseapi";
        }
    }
}
