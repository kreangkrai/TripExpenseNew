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
    public class LastTripService : ILastTrip
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public LastTripService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<string> Delete(string emp_id)
        {
            var response = await _httpClient.DeleteAsync(URL + $"/api/LastTrip/delete?emp_id={emp_id}");
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<LastTripViewModel>> GetByEmp(string emp_id)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/LastTrip/get?emp_id={emp_id}");
            var content = await response.Content.ReadAsStringAsync();
            List<LastTripViewModel> trips = JsonConvert.DeserializeObject<List<LastTripViewModel>>(content);
            return trips;
        }

        public async Task<string> Insert(LastTripModel trip)
        {
            var json = JsonConvert.SerializeObject(trip);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/LastTrip/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
