using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ICompany
    {
        string Insert(CompanyModel data);
        string Insert(List<CompanyModel> datas);
        List<CompanyViewModel> GetCompanyDriverByMonth(string driver, string month);
        List<CompanyViewModel> GetCompanyCarByMonth(string car, string month);
    }
}
