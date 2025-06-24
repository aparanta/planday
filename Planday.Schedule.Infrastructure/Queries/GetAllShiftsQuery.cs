using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class GetAllShiftsQuery : IGetAllShiftsQuery
{
    private readonly IConnectionStringProvider _connectionStringProvider;

    public GetAllShiftsQuery(IConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<IReadOnlyCollection<Shift>> QueryAsync()
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());

        var shiftDtos = await sqlConnection.QueryAsync<ShiftDto>(Sql);

        var shifts = shiftDtos.Select(x =>
            new Shift(x.Id, x.EmployeeId, DateTime.Parse(x.Start), DateTime.Parse(x.End)));

        return shifts.ToList();
    }

    public async Task<Shift> GetShiftbyId(long id)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());
        string SqlGetShiftbyId = @"SELECT Id, EmployeeId, Start, End FROM Shift where id = {0};";
        var query = string.Format(SqlGetShiftbyId, id.ToString());
        var shiftDto = await sqlConnection.QueryFirstOrDefaultAsync<ShiftDto>(query);

        if (shiftDto == null)
            return null;

        var shift = new Shift(
            shiftDto.Id,
            shiftDto.EmployeeId,
            DateTime.Parse(shiftDto.Start),
            DateTime.Parse(shiftDto.End)
        );

        return shift;
    }

    public async Task<Shift> CreateShiftAsync(Shift shift)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());
        var sql = @"INSERT INTO Shift ( Start, [End]) VALUES (@EmployeeId, @Start, @End);
                        SELECT last_insert_rowid();";
        var parameters = new
        {

            Start = shift.Start.ToString("yyyy-MM-dd HH:mm:ss"),
            End = shift.End.ToString("yyyy-MM-dd HH:mm:ss")
        };

        var id = await sqlConnection.ExecuteScalarAsync<long>(sql, parameters);

        return new Shift(id, shift.EmployeeId, shift.Start, shift.End);
    }

    public async Task AssignEmployeeToShiftAsync(long shiftId, long employeeId)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());
        var sql = $"UPDATE  Shift SET EmployeeId = {employeeId} where id = {shiftId};";
        
        var rows = await sqlConnection.ExecuteAsync(sql);

        return;
    }


    public async Task<IReadOnlyCollection<Shift>> GetShiftsByEmployeeIdAsync(long id)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());
       
        string sql = @"SELECT Id, EmployeeId, Start, End FROM Shift where EmployeeId = {0};";
        var query = string.Format(sql, id.ToString());
        var shiftDtos = await sqlConnection.QueryAsync<ShiftDto>(Sql);

        var shifts = shiftDtos.Select(x =>
            new Shift(x.Id, x.EmployeeId, DateTime.Parse(x.Start), DateTime.Parse(x.End)));

        return shifts.ToList();
    }

    private const string Sql = @"SELECT Id, EmployeeId, Start, End FROM Shift;";


    private record ShiftDto(long Id, long? EmployeeId, string Start, string End);
}

