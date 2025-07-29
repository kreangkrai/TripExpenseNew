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
    public class LocationCustomerService : ILocationCustomer
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public LocationCustomerService()
        {
            API = new ConnectAPIService();
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<List<LocationCustomerModel>> GetByEmp(string emp_id)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/LocationCustomer/getbyempid?emp_id={emp_id}");
            var content = await response.Content.ReadAsStringAsync();
            List<LocationCustomerModel> customers = JsonConvert.DeserializeObject<List<LocationCustomerModel>>(content);
            return customers;
        }

        public async Task<string> Insert(LocationCustomerModel location)
        {
            var json = JsonConvert.SerializeObject(location);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/LocationCustomer/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
