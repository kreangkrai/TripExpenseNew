using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;

namespace TripExpenseNew.Services
{
    public class KalmanService : IKalman
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public KalmanService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<KalmanParameterModel> GetParameter()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Kalman/gets");
            var content = await response.Content.ReadAsStringAsync();
            KalmanParameterModel kalman = JsonConvert.DeserializeObject<KalmanParameterModel>(content);
            return kalman;
        }
    }
}
