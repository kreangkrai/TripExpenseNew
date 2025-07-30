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
    public class CompanyService : ICompany
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public CompanyService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<string> Insert(CompanyModel data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Company/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Insert(List<CompanyModel> datas)
        {
            var json = JsonConvert.SerializeObject(datas);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Company/inserts", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<CompanyViewModel>> GetCompanyDriverByMonth(string driver, string month)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Company/getdriverbymonth?driver={driver}&month={month}");
            var content = await response.Content.ReadAsStringAsync();
            List<CompanyViewModel> companies = JsonConvert.DeserializeObject<List<CompanyViewModel>>(content);
            return companies;
        }

        public async Task<List<CompanyViewModel>> GetCompanyCarByMonth(string car, string month)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Company/getcarbymonth?car={car}&month={month}");
            var content = await response.Content.ReadAsStringAsync();
            List<CompanyViewModel> companies = JsonConvert.DeserializeObject<List<CompanyViewModel>>(content);
            return companies;
        }
    }
}
