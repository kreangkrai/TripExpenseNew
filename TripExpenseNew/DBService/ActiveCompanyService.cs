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
    public class ActiveCompanyService : IActiveCompany
    {
        private readonly SQLiteAsyncConnection database;
        public ActiveCompanyService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ActiveCompanyModel>().Wait();
        }
        public async Task<int> Delete(string trip)
        {
            var itemsToDelete = await database.Table<ActiveCompanyModel>()
              .Where(w => w.trip == trip)
              .ToListAsync();

            int rowsDeleted = 0;
            foreach (var item in itemsToDelete)
            {
                rowsDeleted += await database.DeleteAsync(item);
            }

            return rowsDeleted;
        }

        public Task<List<ActiveCompanyModel>> GetByTrip(string trip)
        {
            return database.Table<ActiveCompanyModel>().Where(w => w.trip == trip).ToListAsync();
        }

        public Task<int> Insert(ActiveCompanyModel company)
        {
            return database.InsertAsync(company);
        }
    }
}
