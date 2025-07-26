namespace TripExpenseNew.Models
{
    public class EmployeeModel
    {
        public string emp_id { get; set; }
        public string name { get; set; }
        public string department {  get; set; }
        public int level { get; set; }
        public string role { get; set; }
        public bool active { get; set; }
        public DateTime last_trip { get; set; }
    }
}
