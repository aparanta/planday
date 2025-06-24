using System.Threading.Tasks;

namespace Planday.Schedule.Infrastructure.Http
{
    public interface IEmployeeApiClient
    {
        Task<EmployeeDTO> GetEmployeeAsync(long employeeId, string authToken);
    }
}
