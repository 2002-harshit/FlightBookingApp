using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FlightBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingApp.Controllers;

public class CustomerController : Controller
{
    private readonly PostgresContext _db;
    private readonly ILogger<CustomerController> _logger;
    private readonly HttpClient client;

    // private readonly PasswordHasher<Customer> _passwordHasher;

    public CustomerController(PostgresContext db,ILogger<CustomerController> logger,IHttpClientFactory clientFactory)
    {
        _db = db;
        _logger = logger;
        client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // _passwordHasher = new PasswordHasher<Customer>();
    }
    // public CustomerController(ILogger<CustomerController> logger)
    // {
    //     _logger = logger;
    //     _passwordHasher = new PasswordHasher<Customer>();
    // }
    // GET
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(Customer newCust)
    {
        try
        {
            if (!ModelState.IsValid) return View(newCust);

            Customer customerToPost = new Customer
            {
                Name = newCust.Name,
                Email = newCust.Email,
                Phone = newCust.Phone,
                Address = newCust.Address,
                Password = newCust.Password
            };
            
            HttpResponseMessage response = await client.PostAsJsonAsync("http://localhost:5044/api/Customer", customerToPost);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account created sucessfully";
                return RedirectToAction("Login");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    String errorResponse = await response.Content.ReadAsStringAsync();
                    if (errorResponse.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("Email","An account with this email already exists");
                    }
                    else if(errorResponse.Contains("phone",StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("Phone","An account with this phone already exists");
                    }

                    return View(newCust);

                }
                else
                {
                    ViewBag.ErrorMessage="An error occurred while creating the account. Please try again.";
                    return View(newCust);
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            ViewBag.ErrorMessage="An error occurred while creating the account. Please try again.";
            return View(newCust);
        }
        
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(Customer cust)
    {
        try
        {
            // Customer? existingCust= _db.Customers.FirstOrDefault(c=> c.Email.ToLower().Equals(cust.Email.ToLower()));

            var response = await client.GetAsync($"http://localhost:5044/api/Customer/{cust.Email}");
            if (response.StatusCode==HttpStatusCode.NotFound)
            {
                ViewBag.ErrorMessage="Either username or password is wrong!!";
                return View(cust);
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            String jsonString = await response.Content.ReadAsStringAsync();
            var existingCust= JsonSerializer.Deserialize<Customer>(jsonString, options);
            
            if(existingCust!=null && !existingCust.Password.Equals(cust.Password))
            {
                ViewBag.ErrorMessage="Either username or password is wrong!!";
                return View(cust);
            }
            //* if login is successfull then
            HttpContext.Session.SetString("Id",existingCust.Customerid.ToString());
            HttpContext.Session.SetString("Name",existingCust.Name);
            HttpContext.Session.SetString("Email",existingCust.Email);

            if (existingCust.Email.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                //* if he is the admin, show his dashboard
                TempData["Name"] = "Admin";
                return RedirectToAction("Admin");
            }
            
            return RedirectToAction("Index","BookingDetail");

        }
        catch (Exception e)
        {
            ViewBag.ErrorMessage="An error occurred while logging in. Please try again.";
            return View(cust);
        }
        
    }

    public IActionResult Admin()
    {
        if (!HttpContext.Session.GetString("Email").Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Login");
        }

        return View();

    }
    
    public IActionResult Search()
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }

        ViewBag.id = HttpContext.Session.GetString("Id");
        ViewBag.Name = HttpContext.Session.GetString("Name");
        ViewBag.SourceList = _db.Flights.Select(f => f.Source).Distinct().ToList();
        ViewBag.DestinationList = _db.Flights.Select(f => f.Destination).Distinct().ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Search(Bookingdetail newBooking)
    {
        try
        {
            ViewBag.SourceList = _db.Flights.Select(f => f.Source).Distinct().ToList();
            ViewBag.DestinationList = _db.Flights.Select(f => f.Destination).Distinct().ToList();
            // Flight? existingFlight = _db.Flights.Where(f => newBooking.Source.ToLower().Equals(f.Source.ToLower()) && newBooking.Destination.ToLower().Equals(f.Destination.ToLower()) && DateOnly.FromDateTime(f.Departuretime) == newBooking.DepartureDate && newBooking.Noofpassengers <= f.Capacity-f.Currentlybookedseats
            // );
            List<Flight> existingFlights = _db.Flights.Where(f =>
                newBooking.Source.ToLower().Equals(f.Source.ToLower()) &&
                newBooking.Destination.ToLower().Equals(f.Destination.ToLower()) &&
                DateOnly.FromDateTime(f.Departuretime) == newBooking.DepartureDate).ToList();
            
            if (!existingFlights.Any())
            {
                ViewBag.FlightList = existingFlights;
                // TempData["ErrorMessage"] = "No flights found matching the criteria!!";
                return View(newBooking);
            }

            ViewBag.FlightList = existingFlights;
            return View(newBooking);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "An error occurred while processing your request.";
            return RedirectToAction("Admin");
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Add()
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Add(Customer newCust)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(newCust);
            }
            Customer? existingCustomer = _db.Customers.FirstOrDefault(c => c.Email.ToLower().Equals(newCust.Email.ToLower()));
            if (existingCustomer != null)
            {
                ModelState.AddModelError("Email","An account with this email already exists.");
                return View(newCust);
            }

            // newCust.Password = _passwordHasher.HashPassword(newCust, newCust.Password);
            _db.Customers.Add(newCust);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Account created successfully.";
            return RedirectToAction("Admin");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            ViewBag.ErrorMessage="An error occurred while creating the account. Please try again.";
            return View(newCust);
        }
    }

    public IActionResult AllUsers()
    {
        return View(_db.Customers.OrderBy(f=>f.Name));
    }
    
    public IActionResult Edit(int id)
    {
        
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        
        try
        {
            Customer? c = _db.Customers.Find(id);

            if (c == null)
            {
                TempData["ErrorMessage"] = "Customer does not exist";
                return RedirectToAction("Admin");
            }
            else
            {
                return View(c);
            }
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the customer details. Please try again.";
            return RedirectToAction("Admin");
        }
    }
    
    [HttpPost]
    public IActionResult Edit(Customer upCust)
    {
        try
        {
            Customer? existingCustomer =
                _db.Customers.AsNoTracking().FirstOrDefault(c => c.Customerid == upCust.Customerid);
            if (existingCustomer == null)
            {
                TempData["ErrorMessage"] = "Customer does not exist";
                return RedirectToAction("Admin");
            }
            
            Customer? existingCustomerWithSameEmail = _db.Customers.FirstOrDefault(c => c.Email.ToLower().Equals(upCust.Email.ToLower()) && c.Customerid!=upCust.Customerid);
            if (existingCustomerWithSameEmail != null)
            {
                ModelState.AddModelError("Email","A customer with this email already exists");
                return View(upCust);
            }

            upCust.Password = existingCustomer.Password;
            _db.Customers.Update(upCust);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Customer Information updated successfully.";
            return RedirectToAction("Admin");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            TempData["ErrorMessage"]="An error occurred while fetching the customer details. Please try again.";
            return RedirectToAction("Admin");
        }
        
    }

    public IActionResult Delete(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        
        try
        {
            Customer? c= _db.Customers.Find(id);

            if (c == null)
            {
                TempData["ErrorMessage"] = "Admin does not exist";
                return RedirectToAction("Admin");
            }
            else
            {
                return View(c);
            }
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the Customer details. Please try again.";
            return RedirectToAction("Admin");
        }
    }
    
    
    [HttpPost]
    [ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.Equals("admin@admin.com",StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        try
        {
            Customer? c = _db.Customers.Find(id);
    
            if (c == null)
            {
                TempData["ErrorMessage"] = "Customer does not exist";
                return RedirectToAction("Admin");
            }

            _db.Customers.Remove(c);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Customer deleted Sucessfully";
            return RedirectToAction("Admin");
            
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Error: {e.Message}");
            TempData["ErrorMessage"]="An error occurred while fetching the account details. Please try again.";
            return RedirectToAction("Admin");
        }
    }
    
    
}