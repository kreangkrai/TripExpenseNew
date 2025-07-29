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
    public class TrackingService : ITracking
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public TrackingService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<TrackingModel> GetTracking()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Tracking/get");
            var content = await response.Content.ReadAsStringAsync();
            TrackingModel tracking = JsonConvert.DeserializeObject<TrackingModel>(content);
            return tracking;
        }

        public async Task<string> Update(TrackingModel tracking)
        {
            var json = JsonConvert.SerializeObject(tracking);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync(URL + "/api/Tracking/update", byteContent).ConfigureAwait(false);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
