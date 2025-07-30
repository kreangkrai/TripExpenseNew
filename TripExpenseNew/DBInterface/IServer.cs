using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IServer
    {
        Task<int> Save(ServerModel server);
        Task<ServerModel> Get(int id);
    }
}
