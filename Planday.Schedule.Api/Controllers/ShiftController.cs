using Microsoft.AspNetCore.Mvc;
using Planday.Schedule.Queries;
using Planday.Schedule.Infrastructure.Http;

namespace Planday.Schedule.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly IGetAllShiftsQuery _getAllShiftsQuery;
        private readonly IEmployeeQuery _employeeQuery;
        private readonly IEmployeeApiClient _employeeApiClient;

        public ShiftController(
            IGetAllShiftsQuery getAllShiftsQuery,
            IEmployeeQuery employeeQuery,
            IEmployeeApiClient employeeApiClient)
        {
            _getAllShiftsQuery = getAllShiftsQuery;
            _employeeQuery = employeeQuery;
            _employeeApiClient = employeeApiClient;
        }

        [HttpGet("{id:long}")]
        //Should ideally use DTOs for returning data, but for simplicity returning Shift directly
        public async Task<ActionResult<(Shift, string)>> GetShiftById(long id)
        {
            var shift = await _getAllShiftsQuery.GetShiftbyId(id);

            if (shift == null)
            {
                return NotFound();
            }

            // If the shift has an assigned employee, fetch their details
            if (shift.EmployeeId.HasValue)
            {
                try
                {
                    var employee = await _employeeApiClient.GetEmployeeAsync(shift.EmployeeId.Value, "8e0ac353-5ef1-4128-9687-fb9eb8647288");
                    
                    // Return the shift along with employee details
                    return Ok((shift, $"{employee.Name} ({employee.Email})"));
                }
                // Might be more specific exceptions to catch
                catch (Exception ex)
                {
                    // Log the exception as needed
                    return StatusCode(500, $"An error occurred while retrieving the employee: {ex.Message}");
                }                
               
            }

            return Ok(shift);
        }

        [HttpPost]
        public async Task<ActionResult<Shift>> CreateShift([FromBody] Shift input)
        {
            //For simplicity validation is being done here
            // In a real-world scenario, you would likely use a validation library or framework.
            if (input.Start > input.End)
            {
                return BadRequest("Start time must not be greater than end time.");
            }

            if (input.Start.Date != input.End.Date)
            {
                return BadRequest("Start and end time must be on the same day.");
            }

            var shift = new Shift(0, null, input.Start, input.End);

            var createdShift = await _getAllShiftsQuery.CreateShiftAsync(shift);

            return CreatedAtAction(nameof(GetShiftById), new { id = createdShift.Id }, createdShift);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<Shift>>> GetAllShifts()
        {
            var shifts = await _getAllShiftsQuery.QueryAsync();
            return Ok(shifts);
        }

        [HttpPut("{shiftId:long}/assign/{employeeId:long}")]
        public async Task<ActionResult<Shift>> AssignShiftToEmployee(long shiftId, long employeeId)
        {
            // For simplicity validation is being done here
            // In a real-world scenario, you would likely use a validation library or framework.
            // Check if shift exists
            var shift = await _getAllShiftsQuery.GetShiftbyId(shiftId);
            if (shift == null)
                return NotFound("Shift not found.");

            // Check if employee exists
            var employee = await _employeeQuery.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return NotFound("Employee not found.");

            // Check if shift is already assigned
            if (shift.EmployeeId != null)
                return BadRequest("This shift is already assigned to an employee.");

            // Check for overlapping shifts
            //Maybe add filter for date to limit shifts retrieved
            var employeeShifts = await _getAllShiftsQuery.GetShiftsByEmployeeIdAsync(employeeId);
            // TODO: Overlap logic
            bool overlaps = employeeShifts.Any(s =>
                (s.Start < shift.Start) && (shift.Start < s.End)
                || (s.Start < shift.End) && (shift.End < s.End)
            );

            if (overlaps)
                return BadRequest("Employee already has a shift that overlaps with this time.");

            // Assign shift
            await _getAllShiftsQuery.AssignEmployeeToShiftAsync(shiftId, employeeId);

            return Ok();
        }
    }
}

