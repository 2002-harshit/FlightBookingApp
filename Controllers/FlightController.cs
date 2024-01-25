using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FlightBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FlightBookingApp.Controllers;

public class FlightController : Controller
{
    private readonly PostgresContext _db;
    private readonly ILogger<FlightController> _logger;
    private readonly HttpClient client;

    public FlightController(PostgresContext db,ILogger<FlightController> logger,IHttpClientFactory clientFactory)
    {
        _db = db;
        _logger = logger;
        client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IActionResult> Index()
    {
        // HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        try
        {

            HttpResponseMessage response = await client.GetAsync("http://localhost:5044/api/Flight");

            if (response.IsSuccessStatusCode)
            {
                String jsonString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flights = JsonSerializer.Deserialize<List<Flight>>(jsonString, options);
                // foreach (var f in flights)
                // {
                //     Console.WriteLine(f);
                // }
                return View(flights.OrderBy(f => f.Name));
                // return View(_db.Flights.OrderBy(f=>f.Name));
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flights. Please try again later.";
                }

                return RedirectToAction("Admin", "Customer");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"] = "An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Admin", "Customer");
        }
    }
    
    public async Task<IActionResult> Add()
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.ToLower().Equals("admin@admin.com"))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(Flight newFlight)
    {
        
        //* repopulating
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        try
        {
            if (!ModelState.IsValid) return View(newFlight);
            
            if (newFlight.Arrivaltime <= newFlight.Departuretime)
            {
                ViewBag.ErrorMessage = "Arrival time cannot be less than departure time";
                return View(newFlight);

            }

            if (newFlight.Departuretime < DateTime.Now)
            {
                ModelState.AddModelError("Departuretime","Select a future date");
                return View(newFlight);
            }
            if (newFlight.Arrivaltime < DateTime.Now)
            {
                ModelState.AddModelError("Arrivaltime","Select a future date");
                return View(newFlight);

            }

            Flight flightToPost= new Flight
            {
                Name = newFlight.Name,
                Source = newFlight.Source,
                Destination = newFlight.Destination,
                Departuretime = newFlight.Departuretime,
                Arrivaltime = newFlight.Arrivaltime,
                Rate = newFlight.Rate,
                Capacity = newFlight.Capacity,
                Status = newFlight.Status,
                Code = newFlight.Code
            };
            
            // Console.WriteLine(flightToPost);

        // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PostAsJsonAsync("http://localhost:5044/api/Flight", flightToPost);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Flight added sucessfully";
                return RedirectToAction("Admin","Customer");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    String errorResponse = await response.Content.ReadAsStringAsync();
                    if (errorResponse.ToLower().Contains("code"))
                    {
                        ModelState.AddModelError("Code","A flight with this code already exists");
                    }
                    return View(newFlight);

                }
                else
                {
                    ViewBag.ErrorMessage="An error occurred while adding the flight. Please try again.";
                    return View(newFlight);
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            ViewBag.ErrorMessage="An error occurred while adding the flight. Please try again.";
            return View(newFlight);
        }
    }
    
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5044/api/Flight/{id}");

            if (response.IsSuccessStatusCode)
            {
                String jsonString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flight = JsonSerializer.Deserialize<Flight>(jsonString,options);
                return View(flight);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }  
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.ToLower().Equals("admin@admin.com"))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        }
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5044/api/Flight/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                String jsonString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flight = JsonSerializer.Deserialize<Flight>(jsonString,options);
                return View(flight);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
                
            }
            
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Flight upflight)
    {
        ViewBag.StatusOptions = new List<string> { "Scheduled", "Delayed", "Cancelled" };

        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response =
                await client.PutAsJsonAsync($"http://localhost:5044/api/Flight/{upflight.Flightid}", upflight);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Flight updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("Code","A flight with this code already exists");
                    return View(upflight);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    TempData["ErrorMessage"] = "Bad request";
                    return RedirectToAction("Index");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details not found";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }

    }

    public async Task<IActionResult> Cancel(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.ToLower().Equals("admin@admin.com"))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        } 
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5044/api/Flight/{id}");

            if (response.IsSuccessStatusCode)
            {
                String jsonString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flight = JsonSerializer.Deserialize<Flight>(jsonString,options);
                return View(flight);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ActionName("Cancel")]
    public async Task<IActionResult> CancelConfirmed(int id)
    {
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage get_response = await client.GetAsync($"http://localhost:5044/api/Flight/{id}");

            if (get_response.IsSuccessStatusCode)
            {
                String jsonString = await get_response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flight = JsonSerializer.Deserialize<Flight>(jsonString,options);
                flight.Status = "Cancelled";
                HttpResponseMessage put_response =
                    await client.PutAsJsonAsync($"http://localhost:5044/api/Flight/{flight.Flightid}", flight);
                
                if (put_response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Flight cancelled successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    if (put_response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        TempData["ErrorMessage"] = "Bad request";
                        return RedirectToAction("Index");
                    }
                    else if (put_response.StatusCode == HttpStatusCode.NotFound)
                    {
                        TempData["ErrorMessage"] = "Flight details not found";
                        return RedirectToAction("Index");
                    }
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                if (get_response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(get_response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        String? isSomeoneLoggedIn = HttpContext.Session.GetString("Email");
        if (isSomeoneLoggedIn == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        if (!isSomeoneLoggedIn.ToLower().Equals("admin@admin.com"))
        {
            TempData["ErrorMessage"] = "Not authorized !!";
            return RedirectToAction("Login", "Customer");
        } 
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync($"http://localhost:5044/api/Flight/{id}");

            if (response.IsSuccessStatusCode)
            {
                String jsonString = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(jsonString);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flight = JsonSerializer.Deserialize<Flight>(jsonString,options);
                return View(flight);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }
    }
    
    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            // HttpClient client = new HttpClient();
            // client.DefaultRequestHeaders.Clear();
            // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.DeleteAsync($"http://localhost:5044/api/Flight/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Flight deleted successfully";
                return RedirectToAction("Index");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Flight details could not be fetched";
                    return RedirectToAction("Index");
                }
                else if(response.StatusCode==HttpStatusCode.InternalServerError)
                {
                    TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                    return RedirectToAction("Index");
                }
                TempData["ErrorMessage"] = "Error occurred while fetching the flight. Try again later";
                return RedirectToAction("Index");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["ErrorMessage"]="An error occurred while fetching the flights. Please try again.";
            return RedirectToAction("Index");
        }
    }
    
    
    
}