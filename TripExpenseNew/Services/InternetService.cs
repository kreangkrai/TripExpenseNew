using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Interface;

namespace TripExpenseNew.Services
{
    public class InternetService : IInternet
    {
        private IConnectAPI API;
        private string URL;
        public InternetService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
        }
        public async Task<bool> CheckServerConnection(string serverUrl)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                var response = await client.GetAsync(URL + serverUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
