using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;
using System.Threading.Tasks;
namespace Planday.Schedule.Infrastructure.Queries;

public class EmployeeQuery : IEmployeeQuery

{
    private readonly IConnectionStringProvider _connectionStringProvider;

    public EmployeeQuery(IConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async  Task<Employee> GetEmployeeByIdAsync(long id)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());
        string sql = $"SELECT Id, Name FROM Employee where id = {id};";

        var employeeDTO = await sqlConnection.QueryFirstOrDefaultAsync<EmployeeDto>(sql);

        if (employeeDTO == null)
            return null;

        var employee = new Employee(
            employeeDTO.Id,
            employeeDTO.Name
                   );

        return employee;
    }



    private record EmployeeDto(long Id, string Name);

}
