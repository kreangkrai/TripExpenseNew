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
    public class ActivePublicService : IActivePublic
    {
        private readonly SQLiteAsyncConnection database;
        public ActivePublicService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ActivePublicModel>().Wait();
        }
        public async Task<int> Delete(string trip)
        {
            var itemsToDelete = await database.Table<ActivePublicModel>()
              .Where(w => w.trip == trip)
              .ToListAsync();

            int rowsDeleted = 0;
            foreach (var item in itemsToDelete)
            {
                rowsDeleted += await database.DeleteAsync(item);
            }

            return rowsDeleted;
        }

        public Task<List<ActivePublicModel>> GetByTrip(string trip)
        {
            return database.Table<ActivePublicModel>().Where(w => w.trip == trip).ToListAsync();
        }

        public Task<int> Insert(ActivePublicModel p)
        {
            return database.InsertAsync(p);
        }
    }
}
