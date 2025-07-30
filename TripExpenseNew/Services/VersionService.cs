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
    public class VersionService : IVersion
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public VersionService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<VersionModel> GetVersion()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Version/get");
            var content = await response.Content.ReadAsStringAsync();
            VersionModel version = JsonConvert.DeserializeObject<VersionModel>(content);
            return version;
        }

        public async Task<string> Update(VersionModel version)
        {
            var json = JsonConvert.SerializeObject(version);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync(URL + "/api/Version/update", byteContent).ConfigureAwait(false);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
