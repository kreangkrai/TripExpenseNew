using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBInterface;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBService
{
    public class ServerService : IServer
    {
        private readonly SQLiteAsyncConnection database;
        public ServerService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ServerModel>().Wait();
        }

        public Task<ServerModel> Get(int id)
        {
            return database.Table<ServerModel>().Where(w => w.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(ServerModel server)
        {
            int count = await database.Table<ServerModel>().CountAsync();
            if (count == 0)
            {
                return await database.InsertAsync(server);
            }
            else
            {
                return await database.UpdateAsync(server);
            }
        }
    }
}
