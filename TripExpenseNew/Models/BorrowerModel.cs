namespace TripExpenseNew.Models
{
    public class BorrowerModel
    {
        public string borrow_id { get; set; }
        public string car_id { get; set; }
        public string job_id { get; set; }
        public int mileage_start { get; set; }
        public string main_location { get; set; }
        public string borrower {  get; set; }
        public DateTime borrower_date { get; set; }
        public DateTime plan_return_date { get; set; }
        public string customer {  get; set; }
    }
}
