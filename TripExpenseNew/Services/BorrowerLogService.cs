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
    public class BorrowerLogService : IBorrowerLog
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public BorrowerLogService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<List<BorrowerLogViewModel>> GetBorrowerByCar(string car_id)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/BorrowerLog/getbycar?car_id={car_id}");
            var content = await response.Content.ReadAsStringAsync();
            List<BorrowerLogViewModel> borrowers = JsonConvert.DeserializeObject<List<BorrowerLogViewModel>>(content);
            return borrowers;
        }

        public async Task<List<BorrowerLogViewModel>> GetBorrowers()
        {
            var response = await _httpClient.GetAsync(URL + "/api/BorrowerLog/gets");
            var content = await response.Content.ReadAsStringAsync();
            List<BorrowerLogViewModel> borrowers = JsonConvert.DeserializeObject<List<BorrowerLogViewModel>>(content);
            return borrowers;
        }

        public async Task<string> Insert(BorrowerLogModel borrower)
        {
            var json = JsonConvert.SerializeObject(borrower);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/BorrowerLog/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
