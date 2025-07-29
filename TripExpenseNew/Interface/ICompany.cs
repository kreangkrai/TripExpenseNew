using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface ICompany
    {
        Task<string> Insert(CompanyModel data);
        Task<string> Insert(List<CompanyModel> datas);
        Task<List<CompanyViewModel>> GetCompanyDriverByMonth(string driver, string month);
        Task<List<CompanyViewModel>> GetCompanyCarByMonth(string car, string month);
    }
}
