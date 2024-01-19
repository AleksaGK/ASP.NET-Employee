using Employees.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Employees.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string apiEndpoint = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

        public async Task<IActionResult> Index()
        {
            List<Employee> employees = await GetEmployees();
            List<SimpledEmployee> simpledEmployees = SimplifyEmployees(employees);
            return View(simpledEmployees);
        }

        private async Task<List<Employee>> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiEndpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonString = await response.Content.ReadAsStringAsync();
                        employees = JsonSerializer.Deserialize<List<Employee>>(jsonString);
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Exception: {ex.Message}");
                }
            }

            return employees;
        }

        private List<SimpledEmployee> SimplifyEmployees(List<Employee> employees)
        {
            List<SimpledEmployee> simpledEmployees = new List<SimpledEmployee>();

            foreach (var employee in employees)
            {
                if (string.IsNullOrEmpty(employee.EmployeeName))
                {
                    continue;
                }


                var foundEmployee = simpledEmployees.FirstOrDefault(e => e.Name == employee.EmployeeName);

                if (foundEmployee == null)
                {
                    simpledEmployees.Add(new SimpledEmployee
                    {
                        Name = employee.EmployeeName,
                        Hours = TimeInHours(employee.StarTimeUtc.ToString(), employee.EndTimeUtc.ToString())
                    });
                }
                else
                {
                    foundEmployee.Hours += TimeInHours(employee.StarTimeUtc.ToString(), employee.EndTimeUtc.ToString());
                }
            }

            return simpledEmployees
                .OrderByDescending(e => e.Hours)
                .ToList();
        }


        private double TimeInHours(string start, string end)
        {
            DateTime startTimeUtc = DateTime.Parse(start);
            DateTime endTimeUtc = DateTime.Parse(end);

            TimeSpan timeDifference = endTimeUtc - startTimeUtc;

            return timeDifference.TotalHours;
        }

    }
}
