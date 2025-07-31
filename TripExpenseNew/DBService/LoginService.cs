using Microsoft.Maui.Controls;
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
    public class LoginService : ILogin
    {
        private readonly SQLiteAsyncConnection database;
        public LoginService()
        {
            string dbPath = DBPath.GetDatabasePath();
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<LoginModel>().Wait();
        }

        public Task<LoginModel> GetLogin(int id)
        {
            return database.Table<LoginModel>().Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> Save(LoginModel login)
        {
            int count = await database.Table<LoginModel>().CountAsync();
            if (count == 0)
            {
                return await database.InsertAsync(login);
            }
            else
            {
                return await database.UpdateAsync(login);
            }           
        }
    }
}
