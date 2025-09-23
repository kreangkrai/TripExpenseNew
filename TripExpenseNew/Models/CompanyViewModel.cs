namespace TripExpenseNew.Models
{
    public class CompanyViewModel
    {
        public int id { get; set; }
        public string car_id { get; set; }
        public string license_plate { get; set; }
        public string driver { get; set; }
        public string driver_name { get; set; }
        public string job_id { get; set; }
        public string trip { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public double distance { get; set; }
        public double speed { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double accuracy { get; set; }
        public string location_mode { get; set; }
        public string location { get; set; }
        public string zipcode { get; set; }
        public int mileage { get; set; }
        public double cash { get; set; }
        public double fleetcard { get; set; }
        public string borrower { get; set; }
        public string borrower_name { get; set; }
    }
}
