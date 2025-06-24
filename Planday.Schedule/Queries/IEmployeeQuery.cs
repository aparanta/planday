namespace Planday.Schedule.Queries;

public interface IEmployeeQuery
{
    Task<Employee> GetEmployeeByIdAsync(long id);
}
