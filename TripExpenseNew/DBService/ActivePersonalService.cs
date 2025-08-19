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
    public class ActivePersonalService : IActivePersonal
    {
        private readonly SQLiteAsyncConnection database;
        public ActivePersonalService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ActivePersonalModel>().Wait();
        }
        public async Task<int> Delete(string trip)
        {
            var itemsToDelete = await database.Table<ActivePersonalModel>()
              .Where(w => w.trip == trip)
              .ToListAsync();

            int rowsDeleted = 0;
            foreach (var item in itemsToDelete)
            {
                rowsDeleted += await database.DeleteAsync(item);
            }

            return rowsDeleted;
        }

        public Task<List<ActivePersonalModel>> GetByTrip(string trip)
        {
            return database.Table<ActivePersonalModel>().Where(w => w.trip == trip).ToListAsync();
        }

        public Task<int> Insert(ActivePersonalModel personal)
        {
            return database.InsertAsync(personal);
        }
    }
}
