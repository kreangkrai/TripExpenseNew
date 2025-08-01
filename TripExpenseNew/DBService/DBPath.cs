using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace TripExpenseNew.DBService
{
    public static class DBPath
    {
        public static string GetDatabasePath()
        {
            string dbName = "tripexpense1.db3";
            string dbPath;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), dbName);
            }
            else
            {
                dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
            }

            return dbPath;
        }
    }
}
