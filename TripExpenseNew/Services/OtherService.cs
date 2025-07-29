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
    public class OtherService : IOther
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public OtherService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<List<OtherViewModel>> GetOtherByMonth(string passenger, string month)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Other/getbymonth?passenger={passenger}&month={month}");
            var content = await response.Content.ReadAsStringAsync();
            List<OtherViewModel> others = JsonConvert.DeserializeObject<List<OtherViewModel>>(content);
            return others;
        }

        public async Task<string> Insert(OtherModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Other/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Insert(List<OtherModel> datas)
        {
            var json = JsonConvert.SerializeObject(datas);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Other/inserts", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
