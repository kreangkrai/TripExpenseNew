using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using ZXing;

namespace TripExpenseNew.Services
{
    public class PublicService : IPublic
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public PublicService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<List<PublicViewModel>> GetPublicByMonth(string passenger, string month)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Public/getbymonth?passenger={passenger}&month={month}");
            var content = await response.Content.ReadAsStringAsync();
            List<PublicViewModel> others = JsonConvert.DeserializeObject<List<PublicViewModel>>(content);
            return others;
        }

        public async Task<string> Insert(PublicModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Public/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Inserts(List<PublicModel> datas)
        {
            var json = JsonConvert.SerializeObject(datas);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Public/inserts", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<PublicViewModel>> GetPublicHistoryByTrip(string passenger, string trip)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Public/gethistorybytrip?passenger={passenger}&trip={trip}");
            var content = await response.Content.ReadAsStringAsync();
            List<PublicViewModel> p = JsonConvert.DeserializeObject<List<PublicViewModel>>(content);
            return p;
        }
    }
}
