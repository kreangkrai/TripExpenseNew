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
    public class CarService : ICar
    {
        private IConnectAPI API;
        private readonly string URL;
        private readonly HttpClient _httpClient;
        public CarService(IConnectAPI _API)
        {
            API = _API;
            URL = API.ConnectAPI();
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }
        public async Task<CarModel> GetByCar(string car_id)
        {
            var response = await _httpClient.GetAsync(URL + $"/api/Car/getbycar?car_id={car_id}");
            var content = await response.Content.ReadAsStringAsync();
            CarModel car = JsonConvert.DeserializeObject<CarModel>(content);
            return car;
        }

        public async Task<List<CarModel>> GetCars()
        {
            var response = await _httpClient.GetAsync(URL + "/api/Car/gets");
            var content = await response.Content.ReadAsStringAsync();
            List<CarModel> cars = JsonConvert.DeserializeObject<List<CarModel>>(content);
            return cars;
        }

        public async Task<string> Insert(CarModel car)
        {
            var json = JsonConvert.SerializeObject(car);
            var buffer = Encoding.UTF8.GetBytes(json);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync(URL + "/api/Car/insert", byteContent);
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }
    }
}
