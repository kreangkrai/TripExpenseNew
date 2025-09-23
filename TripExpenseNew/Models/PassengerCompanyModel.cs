namespace TripExpenseNew.Models
{
    public class PassengerCompanyModel
    {
        public int id { get; set; }
        public string car_id { get; set; }
        public string driver { get; set; }
        public string passenger { get; set; }
        public string job_id { get; set; }
        public string trip { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double accuracy { get; set; }
        public string location_mode { get; set; }
        public string location { get; set; }
        public string zipcode { get; set; }
    }
}
