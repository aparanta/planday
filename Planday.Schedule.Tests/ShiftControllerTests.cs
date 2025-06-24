using Microsoft.AspNetCore.Mvc;
using Moq;
using Planday.Schedule.Api.Controllers;
using Planday.Schedule.Queries;
using Planday.Schedule.Infrastructure.Http;

namespace Planday.Schedule.Tests
{
    [TestClass]
    public class ShiftControllerTests
    {
        private Mock<IGetAllShiftsQuery> _shiftsQueryMock = null!;
        private Mock<IEmployeeQuery> _employeeQueryMock = null!;
        private Mock<IEmployeeApiClient> _employeeApiClientMock = null!;

        [TestInitialize]
        public void Setup()
        {
            _shiftsQueryMock = new Mock<IGetAllShiftsQuery>();
            _employeeQueryMock = new Mock<IEmployeeQuery>();
            _employeeApiClientMock = new Mock<IEmployeeApiClient>();
        }

        private ShiftController CreateController()
            => new ShiftController(_shiftsQueryMock.Object, _employeeQueryMock.Object, _employeeApiClientMock.Object);

        [TestMethod]
        public async Task GetShiftById_ReturnsNotFound_WhenShiftDoesNotExist()
        {
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(It.IsAny<long>())).ReturnsAsync((Shift)null!);

            var controller = CreateController();
            var result = await controller.GetShiftById(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetShiftById_ReturnsShift_WhenNoEmployeeAssigned()
        {
            var shift = new Shift(1, null, DateTime.Now, DateTime.Now.AddHours(1));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);

            var controller = CreateController();
            var result = await controller.GetShiftById(1);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(shift, okResult.Value);
        }

        [TestMethod]
        public async Task GetShiftById_ReturnsShiftAndEmployee_WhenEmployeeAssigned()
        {
            var shift = new Shift(1, 2, DateTime.Now, DateTime.Now.AddHours(1));
            var employeeDto = new EmployeeDTO { Name = "John Doe", Email = "john@doe.com" };

            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeApiClientMock.Setup(c => c.GetEmployeeAsync(2, It.IsAny<string>())).ReturnsAsync(employeeDto);

            var controller = CreateController();
            var result = await controller.GetShiftById(1);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var tuple = ((Shift, string))okResult.Value!;
            Assert.AreEqual(shift, tuple.Item1);
            StringAssert.Contains(tuple.Item2, "John Doe");
            StringAssert.Contains(tuple.Item2, "john@doe.com");
        }

        [TestMethod]
        public async Task GetShiftById_ReturnsNotFound_WhenEmployeeNotFound()
        {
            var shift = new Shift(1, 2, DateTime.Now, DateTime.Now.AddHours(1));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeApiClientMock.Setup(c => c.GetEmployeeAsync(2, It.IsAny<string>())).ReturnsAsync((EmployeeDTO)null!);

            var controller = CreateController();
            var result = await controller.GetShiftById(1);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);
            Assert.AreEqual("Employee not found.", notFound.Value);
        }

        [TestMethod]
        public async Task CreateShift_ReturnsBadRequest_WhenStartAfterEnd()
        {
            var input = new Shift(0, null, DateTime.Now.AddHours(2), DateTime.Now);
            var controller = CreateController();

            var result = await controller.CreateShift(input);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Start time must not be greater than end time.", badRequest.Value);
        }

        [TestMethod]
        public async Task CreateShift_ReturnsBadRequest_WhenStartAndEndNotSameDay()
        {
            var input = new Shift(0, null, DateTime.Today, DateTime.Today.AddDays(1));
            var controller = CreateController();

            var result = await controller.CreateShift(input);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Start and end time must be on the same day.", badRequest.Value);
        }

        [TestMethod]
        public async Task CreateShift_ReturnsCreatedShift()
        {
            var input = new Shift(0, null, DateTime.Today.AddHours(8), DateTime.Today.AddHours(16));
            var created = new Shift(1, null, input.Start, input.End);
            _shiftsQueryMock.Setup(q => q.CreateShiftAsync(It.IsAny<Shift>())).ReturnsAsync(created);

            var controller = CreateController();
            var result = await controller.CreateShift(input);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(created, createdResult.Value);
        }

        [TestMethod]
        public async Task AssignShiftToEmployee_ReturnsNotFound_WhenShiftNotFound()
        {
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync((Shift)null!);

            var controller = CreateController();
            var result = await controller.AssignShiftToEmployee(1, 2);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);
            Assert.AreEqual("Shift not found.", notFound.Value);
        }

        [TestMethod]
        public async Task AssignShiftToEmployee_ReturnsNotFound_WhenEmployeeNotFound()
        {
            var shift = new Shift(1, null, DateTime.Now, DateTime.Now.AddHours(1));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeQueryMock.Setup(q => q.GetEmployeeByIdAsync(2)).ReturnsAsync((Employee)null!);

            var controller = CreateController();
            var result = await controller.AssignShiftToEmployee(1, 2);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);
            Assert.AreEqual("Employee not found.", notFound.Value);
        }

        [TestMethod]
        public async Task AssignShiftToEmployee_ReturnsBadRequest_WhenShiftAlreadyAssigned()
        {
            var shift = new Shift(1, 2, DateTime.Now, DateTime.Now.AddHours(1));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeQueryMock.Setup(q => q.GetEmployeeByIdAsync(2)).ReturnsAsync(new Employee(2, "Test"));

            var controller = CreateController();
            var result = await controller.AssignShiftToEmployee(1, 2);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("This shift is already assigned to an employee.", badRequest.Value);
        }

        [TestMethod]
        public async Task AssignShiftToEmployee_ReturnsBadRequest_WhenOverlappingShift()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, null, now, now.AddHours(2));
            var existingShift = new Shift(2, 2, now.AddHours(1), now.AddHours(3));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeQueryMock.Setup(q => q.GetEmployeeByIdAsync(2)).ReturnsAsync(new Employee(2, "Test"));
            _shiftsQueryMock.Setup(q => q.GetShiftsByEmployeeIdAsync(2)).ReturnsAsync(new List<Shift> { existingShift });

            var controller = CreateController();
            var result = await controller.AssignShiftToEmployee(1, 2);

            var badRequest = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual("Employee already has a shift that overlaps with this time.", badRequest.Value);
        }

        [TestMethod]
        public async Task AssignShiftToEmployee_ReturnsOk_WhenAssignmentIsValid()
        {
            var now = DateTime.Now;
            var shift = new Shift(1, null, now, now.AddHours(2));
            _shiftsQueryMock.Setup(q => q.GetShiftbyId(1)).ReturnsAsync(shift);
            _employeeQueryMock.Setup(q => q.GetEmployeeByIdAsync(2)).ReturnsAsync(new Employee(2, "Test"));
            _shiftsQueryMock.Setup(q => q.GetShiftsByEmployeeIdAsync(2)).ReturnsAsync(new List<Shift>());

            var controller = CreateController();
            var result = await controller.AssignShiftToEmployee(1, 2);

            Assert.IsInstanceOfType(result.Result, typeof(OkResult));
        }
    }
}