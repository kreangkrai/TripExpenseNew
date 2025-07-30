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
    public class AuthenService : IAuthen
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public AuthenService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<AuthenModel> ActiveDirectoryAuthenticate(string username, string password)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(URL + $"/api/Authen/get?username={username}&password={password}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            AuthenModel authen = JsonConvert.DeserializeObject<AuthenModel>(content);
            return authen;
        }
    }
}
