namespace TripExpenseNew.Models
{
    public class BorrowerViewModel
    {
        public string borrow_id { get; set; }
        public string car_id { get; set; }
        public string license_plate { get; set; }
        public string job_id { get; set; }
        public int mileage_start { get; set; }
        public string main_location { get; set; }
        public string borrower { get; set; }
        public string borrower_name { get; set; }
        public DateTime borrow_date { get; set; }
        public DateTime plan_return_date { get; set; }
        public string customer { get; set; }
    }
}
