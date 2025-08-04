using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Interface;

namespace TripExpenseNew.Services
{
    public class CurrentTimeService : ICurrentTime
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public CurrentTimeService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<DateTime> GetCuttentTime()
        {
            var response = await _httpClient.GetAsync(URL + "/api/CurrentTime/get");
            var content = await response.Content.ReadAsStringAsync();
            DateTime now = JsonConvert.DeserializeObject<DateTime>(content);
            return now;
        }
    }
}
