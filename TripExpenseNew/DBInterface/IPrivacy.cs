using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripExpenseNew.DBModels;

namespace TripExpenseNew.DBInterface
{
    public interface IPrivacy
    {
        Task<int> Save(PrivacyModel privacy);
        Task<PrivacyModel> GetPrivacy(int id);
    }
}
