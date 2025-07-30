using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface ILogin
    {
        Task<int> Save (LoginModel login);
        Task<List<LoginModel>> GetLogin();
    }
}
