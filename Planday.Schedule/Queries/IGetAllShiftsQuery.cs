using System.Collections.Generic;
using System.Threading.Tasks;

namespace Planday.Schedule.Queries
{
    public interface IGetAllShiftsQuery
    {
        /// <summary>
        /// Get all shifts
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyCollection<Shift>> QueryAsync();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Shift> GetShiftbyId(long id);

       /// <summary>
       /// Creates a new shift.
       /// </summary>
       /// <param name="shift">The <see cref="Shift"/> object containing the details of the shift to be created. Cannot be null.</param>
       /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Shift"/>
       /// object.</returns>
        public Task<Shift> CreateShiftAsync(Shift shift);

        /// <summary>
        /// Retrieves a collection of shifts associated with the specified employee ID.
        /// </summary>
        /// <param name="id">The unique identifier of the employee whose shifts are to be retrieved.</param>
        /// <returns>The task result contains a read-only collection of  <see
        /// cref="Shift"/> objects associated with the specified employee. If no shifts are found, the collection will
        /// be empty.</returns>
        public Task<IReadOnlyCollection<Shift>> GetShiftsByEmployeeIdAsync(long id);
        
        /// <summary>
        /// Assigns an employee to a shift.
        /// </summary>
        /// <param name="shiftId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public Task AssignEmployeeToShiftAsync(long shiftId, long employeeId);
    }    
}

