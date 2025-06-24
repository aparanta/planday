using System.Threading.Tasks;

namespace Planday.Schedule.Infrastructure.Http
{
    public interface IEmployeeApiClient
    {
        Task<string> GetEmployeeAsync(long employeeId, string authToken);
    }
}
