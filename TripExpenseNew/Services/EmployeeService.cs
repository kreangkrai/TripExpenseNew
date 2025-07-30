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
    public class EmployeeService : IEmployee
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public EmployeeService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<string> Insert(EmployeeModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Employee/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Insert(List<EmployeeModel> datas)
        {
            var json = JsonConvert.SerializeObject(datas);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Employee/inserts", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Update(EmployeeModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync(URL + "/api/Employee/update", byteContent).ConfigureAwait(false);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<EmployeeModel>> GetEmployees()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Employee/gets");
            var content = await response.Content.ReadAsStringAsync();
            List<EmployeeModel> employees = JsonConvert.DeserializeObject<List<EmployeeModel>>(content);
            return employees;
        }
    }
}
