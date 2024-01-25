using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBookingApp.Models;

public class SourceDestinationDifferentAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var bookingDetail = (Bookingdetail)validationContext.ObjectInstance;

        if (bookingDetail.Source.Equals(bookingDetail.Destination, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("Source and Destination cannot be the same.");
        }

        return ValidationResult.Success;
    }
}

public partial class Bookingdetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Bookingid { get; set; }

    [Required]
    public int Flightid { get; set; }

    
    
    public int Customerid { get; set; }

    public DateTime Bookingdate { get; set; }

    [Required]
    [Display(Name = "Number of Passengers")]
    public int Noofpassengers { get; set; }

    [Required]
    [Display(Name = "Total Cost")]
    public decimal Totalcost { get; set; }

    public string Status { get; set; } = null!;

    public string? Seatnumbers { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Flight Flight { get; set; } = null!;
    
    [NotMapped]
    [Required(ErrorMessage = "Source is required")]
    [SourceDestinationDifferent]
    public string Source { get; set; }

    [NotMapped]
    [Required(ErrorMessage = "Destination is required")]
    public string Destination { get; set; }
    
    [NotMapped]
    [Required(ErrorMessage="Departure date is required")]
    public DateOnly DepartureDate { get; set; }

    
    public override string ToString()
    {
        return $"Booking ID: {Bookingid}, Flight ID: {Flightid}, Customer ID: {Customerid}, " +
               $"Booking Date: {Bookingdate.ToString("yyyy-MM-dd")}, Number of Passengers: {Noofpassengers}, " +
               $"Total Cost: {Totalcost}, Status: {Status}, Seat Numbers: {Seatnumbers},Source: {Source},Destination: {Destination},DepartureDate: ${DepartureDate} ";
    }

}
