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
    public class BorrowerService : IBorrower
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public BorrowerService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<string> Insert(BorrowerModel borrower)
        {
            var json = JsonConvert.SerializeObject(borrower);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Borrower/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<string> Update(BorrowerModel borrower)
        {
            var json = JsonConvert.SerializeObject(borrower);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync(URL + "/api/Borrower/update", byteContent).ConfigureAwait(false);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<List<BorrowerViewModel>> GetBorrowers()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Borrower/gets");
            var content = await response.Content.ReadAsStringAsync();
            List<BorrowerViewModel> borrowers = JsonConvert.DeserializeObject<List<BorrowerViewModel>>(content);
            return borrowers;
        }

        public async Task<BorrowerViewModel> GetBorrowerByCar(string car_id)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Borrower/getbycar?car_id={car_id}");
            var content = await response.Content.ReadAsStringAsync();
            BorrowerViewModel borrower = JsonConvert.DeserializeObject<BorrowerViewModel>(content);
            return borrower;
        }
    }
}
