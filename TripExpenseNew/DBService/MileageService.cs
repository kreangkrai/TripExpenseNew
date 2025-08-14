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
    public class MileageService : IMileage
    {
        private readonly SQLiteAsyncConnection database;
        public MileageService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<MileageDBModel>().Wait();
            int count =  database.Table<MileageDBModel>().CountAsync().Result;
            if (count == 0)
            {
                 database.InsertAsync(new MileageDBModel()
                 {
                     Id = 1,
                     mileage = 0
                 } );
            }
        }
     
        public Task<MileageDBModel> GetMileage(int id)
        {
            return database.Table<MileageDBModel>().Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(MileageDBModel mileage)
        {
            int count = await database.Table<MileageDBModel>().CountAsync();
            if (count == 0)
            {
                return await database.InsertAsync(mileage);
            }
            else
            {
                return await database.UpdateAsync(mileage);
            }
        }
    }
}
