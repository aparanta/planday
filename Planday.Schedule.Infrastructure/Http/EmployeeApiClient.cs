using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Planday.Schedule.Infrastructure.Http
{
    public class EmployeeApiClient: IEmployeeApiClient
    {
        private readonly HttpClient _httpClient;

        public EmployeeApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetEmployeeAsync(long employeeId, string authToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://planday-employee-api-techtest.westeurope.azurecontainer.io:5000/employee/{employeeId}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.Add("Authorization", authToken);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}