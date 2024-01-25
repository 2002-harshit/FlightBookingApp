using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingApp.Models;

public partial class Flight
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Flightid { get; set; }

    [Required(ErrorMessage = "Flight name is required")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Source is required")]
    public string Source { get; set; } = null!;

    [Required(ErrorMessage = "Destination is required")]
    public string Destination { get; set; } = null!;

    [Required(ErrorMessage = "Departure time is required")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Departure Time")]
    public DateTime Departuretime { get; set; }

    [Required(ErrorMessage = "Arrival time is required")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Arrival Time")]
    public DateTime Arrivaltime { get; set; }

    [Required(ErrorMessage = "Rate is required")]
    [DataType(DataType.Currency)]
    public decimal Rate { get; set; }

    [Required(ErrorMessage = "Capacity is required")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = null!;


    [Display(Name = "Currently Booked")]
    public int Currentlybookedseats { get; set; } = 0;

    [Required(ErrorMessage = "Flight code is required")]
    public string Code { get; set; } = null!;

    public override string ToString()
    {
        return $"Flight ID: {Flightid}, Name: {Name}, Source: {Source}, Destination: {Destination}, " +
               $"Departure Time: {Departuretime}, Arrival Time: {Arrivaltime}, Rate: {Rate}, " +
               $"Capacity: {Capacity}, Status: {Status}, Currently Booked Seats: {Currentlybookedseats}, " +
               $"Code: {Code}";
    }

    // public virtual ICollection<Bookingdetail> Bookingdetails { get; set; } = new List<Bookingdetail>();
}
