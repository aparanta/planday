using Microsoft.AspNetCore.Mvc;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly IGetAllShiftsQuery _getAllShiftsQuery;
        private readonly IEmployeeQuery _employeeQuery;

        public ShiftController(IGetAllShiftsQuery getAllShiftsQuery, IEmployeeQuery employeeQuery)
        {
            _getAllShiftsQuery = getAllShiftsQuery;
            _employeeQuery = employeeQuery;
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Shift>> GetShiftById(long id)
        {
            var shift = await _getAllShiftsQuery.GetShiftbyId(id);

            if (shift == null)
            {
                return NotFound();
            }

            return Ok(shift);
        }

        [HttpPost]
        public async Task<ActionResult<Shift>> CreateShift([FromBody] Shift input)
        {
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
            var employeeShifts = await _getAllShiftsQuery.GetShiftsByEmployeeIdAsync(employeeId);
            //TODO: Handle null case for employeeShifts
            bool overlaps = employeeShifts.Any(s =>
                ((shift.Start < s.End) && (shift.End > s.Start))
            );

            if (overlaps)
                return BadRequest("Employee already has a shift that overlaps with this time.");

            // Assign shift
            await _getAllShiftsQuery.AssignEmployeeToShiftAsync(shiftId, employeeId);
            
            return Ok();
        }
    }
}

