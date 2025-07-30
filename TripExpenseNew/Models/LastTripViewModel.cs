namespace TripExpenseNew.Models
{
    public class LastTripViewModel
    {
        public int id { get; set; }
        public string emp_id { get; set; }
        public string emp_name { get; set; }
        public DateTime trip { get; set; }
        public string driver { get; set; }
        public string driver_name { get; set; }
        public string car_id { get; set; }
        public string license_plate { get; set; }
        public double speed { get; set; }
        public double distance { get; set; }
        public string mode { get; set; }
    }
}
