using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ICar
    {
        Task<string> Insert(CarModel car);
        Task<List<CarModel>> GetCars();
        Task<CarModel> GetByCar(string car_id);
    }
}
