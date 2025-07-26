namespace TripExpenseNew.Models
{
    public class PersonalModel
    {
        public int id { get; set; }
        public string driver { get; set; }
        public string job_id { get; set; }
        public DateTime trip { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public double distance { get; set; }
        public double speed { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string location_mode { get; set; }
        public string location { get; set; }
        public string zipcode { get; set; }
        public int mileage { get; set; }
    }
}
