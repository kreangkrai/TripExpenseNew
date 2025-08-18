namespace TripExpenseNew.Models
{
    public class LastTripModel
    {
        public int id { get; set; }
        public string emp_id { get; set; }
        public string job_id { get; set; }
        public string trip { get; set; }
        public DateTime date { get; set; }
        public string driver { get; set; }
        public string car_id { get; set; }
        public double speed { get; set; }
        public double distance { get; set; }
        public string location { get; set; }
        public int mileage { get; set; }
        public string mode { get; set; }
        public bool status { get; set; }

    }
}
