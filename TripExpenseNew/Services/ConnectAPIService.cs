using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;
using TripExpenseNew.Interface;

namespace TripExpenseNew.Services
{
    public class ConnectAPIService : IConnectAPI
    {
        private readonly IServer Server;
        public ConnectAPIService(IServer _Server)
        {
            Server = _Server;
        }
        public string ConnectAPI()
        {
            ServerModel server = Server.Get(1).Result;
            if (server != null)
            {
                return server.server;
            }
            return null;
            //return "http://192.168.15.12/tripexpenseapi";
        }
    }
}
