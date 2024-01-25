using FlightBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity;

namespace FlightBookingApp.Controllers;

public class BookingDetailController : Controller
{
    private readonly PostgresContext _db;
    private readonly ILogger<BookingDetailController> _logger;

    public BookingDetailController(PostgresContext db,ILogger<BookingDetailController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index()
    {
        String? id = HttpContext.Session.GetString("Id");
        if (id == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        ViewBag.id = id;
        ViewBag.Name = HttpContext.Session.GetString("Name");
        return View(_db.Bookingdetails.Where(bd=>bd.Customerid==Convert.ToInt32(id)));
    }

    public IActionResult Book()
    {
        String? id = HttpContext.Session.GetString("Id");
        if (id == null)
        {
            return RedirectToAction("Login", "Customer");
        }

        ViewBag.id = id;
        ViewBag.Name = HttpContext.Session.GetString("Name");
        ViewBag.SourceList = _db.Flights.Select(f => f.Source).Distinct().ToList();
        ViewBag.DestinationList = _db.Flights.Select(f => f.Destination).Distinct().ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Book(Bookingdetail newBooking)
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
                DateOnly.FromDateTime(f.Departuretime) == newBooking.DepartureDate &&
                newBooking.Noofpassengers <= f.Capacity - f.Currentlybookedseats && !f.Status.ToLower().Equals("cancelled")).ToList();
            
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
            return RedirectToAction("Index");
        }
        
    }

    [HttpPost]
    public IActionResult ConfirmBooking(int selectedFlightId,int numberOfPassengers)
    {
        try
        {
            String? id = HttpContext.Session.GetString("Id");
            if (id == null)
            {
                return RedirectToAction("Login", "Customer");
            }
            Flight? selectedFlight = _db.Flights.FirstOrDefault(f => f.Flightid == selectedFlightId);
        
            if (selectedFlightId == null)
            {
                TempData["ErrorMessage"] = "Flight not found.";
                return RedirectToAction("Book");
            }

            String generatedSeats = "";
            for (int i = 1; i <= numberOfPassengers; i++)
            {
                generatedSeats += ((i + selectedFlight.Currentlybookedseats).ToString()+",");
            }
            //* add a booking
            _db.Bookingdetails.Add(new Bookingdetail()
            {
                Flightid = selectedFlightId,
                Customerid = Convert.ToInt32(HttpContext.Session.GetString("Id")),
                Bookingdate = DateTime.Now,
                Noofpassengers = numberOfPassengers,
                Totalcost = numberOfPassengers*selectedFlight.Rate,
                Status = "Confirmed",
                Seatnumbers = generatedSeats,
            });

            selectedFlight.Currentlybookedseats += numberOfPassengers;
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Booking successfully made";
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "An error occurred while processing your request.";
            return RedirectToAction("Index");
        }
        
    }
}