using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IVersion
    {
        Task<VersionModel> GetVersion();
        Task<string> Update(VersionModel version);
    }
}
