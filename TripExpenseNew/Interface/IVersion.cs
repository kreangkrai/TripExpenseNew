using TripExpenseNew.Models;

namespace TripExpenseNew.Interface
{
    public interface IVersion
    {
        VersionModel GetVersion();
        string Update(VersionModel version);
    }
}
