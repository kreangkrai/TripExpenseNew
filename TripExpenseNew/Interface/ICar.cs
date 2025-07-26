using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ICar
    {
        string Insert(CarModel car);
        List<CarModel> GetCars();
        CarModel GetByCar(string car_id);
    }
}
