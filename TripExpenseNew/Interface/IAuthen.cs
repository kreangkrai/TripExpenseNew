using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IAuthen
    {
        Task<AuthenModel> ActiveDirectoryAuthenticate(string username, string password);
    }
}
