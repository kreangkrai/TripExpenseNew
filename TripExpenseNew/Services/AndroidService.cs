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
    public class AndroidService : IAndroid
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public AndroidService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<AndroidParameterModel> GetParameter()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Android/get");
            var content = await response.Content.ReadAsStringAsync();
            AndroidParameterModel android = JsonConvert.DeserializeObject<AndroidParameterModel>(content);
            return android;
        }
    }
}
