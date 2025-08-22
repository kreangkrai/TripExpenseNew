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
    public class PassengerCompanyService : IPassengerCompany
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public PassengerCompanyService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<List<PassengerCompanyViewModel>> GetPassengerCompanyByMonth(string passenger, string month)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/PassengerCompany/getbymonth?passenger={passenger}&month={month}");
            var content = await response.Content.ReadAsStringAsync();
            List<PassengerCompanyViewModel> passengers = JsonConvert.DeserializeObject<List<PassengerCompanyViewModel>>(content);
            return passengers;
        }

        public async Task<string> Insert(PassengerCompanyModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/PassengerCompany/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Insert(List<PassengerCompanyModel> datas)
        {
            var json = JsonConvert.SerializeObject(datas);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/PassengerCompany/inserts", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<PassengerCompanyViewModel>> GetPassengerCompanyByDriver(string driver, string trip)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/PassengerCompany/getbydriver?driver={driver}&trip={trip}");
            var content = await response.Content.ReadAsStringAsync();
            List<PassengerCompanyViewModel> passengers = JsonConvert.DeserializeObject<List<PassengerCompanyViewModel>>(content);
            return passengers;
        }

        public async Task<List<PassengerCompanyViewModel>> GetPassengerCompanyHistoryByTrip(string passenger, string trip)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/PassengerCompany/getpassengerhistorybytrip?passenger={passenger}&trip={trip}");
            var content = await response.Content.ReadAsStringAsync();
            List<PassengerCompanyViewModel> passengers = JsonConvert.DeserializeObject<List<PassengerCompanyViewModel>>(content);
            return passengers;
        }
    }
}
