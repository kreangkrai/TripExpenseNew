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
    public class PersonalService : IPersonal
    {
        private readonly SQLiteAsyncConnection database;
        public PersonalService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<PersonalDBModel>().Wait();
        }
        public async Task<int> Delete(string trip)
        {
            var itemsToDelete = await database.Table<PersonalDBModel>()
               .Where(w => w.trip == trip)
               .ToListAsync();

            int rowsDeleted = 0;
            foreach (var item in itemsToDelete)
            {
                rowsDeleted += await database.DeleteAsync(item);
            }

            return rowsDeleted;
        }

        public Task<List<PersonalDBModel>> GetByTrip(string trip)
        {
            return database.Table<PersonalDBModel>().Where(w=>w.trip == trip).ToListAsync();
        }

        public Task<int> Insert(PersonalDBModel personal)
        {
            return database.InsertAsync(personal);
        }
    }
}
