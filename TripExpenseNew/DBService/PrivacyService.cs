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
    public class PrivacyService : IPrivacy
    {
        private readonly SQLiteAsyncConnection database;
        public PrivacyService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<PrivacyModel>().Wait();
        }

        public Task<PrivacyModel> GetPrivacy(int id)
        {
            return database.Table<PrivacyModel>().Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(PrivacyModel privacy)
        {
            int count = await database.Table<PrivacyModel>().CountAsync();
            if (count == 0)
            {
                return await database.InsertAsync(privacy);
            }
            else
            {
                return await database.UpdateAsync(privacy);
            }
        }
    }
}
