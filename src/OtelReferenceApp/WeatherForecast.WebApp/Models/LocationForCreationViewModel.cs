namespace WeatherForecast.Infrastructure.Models;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class WeatherForecastCreateViewModel
{
    [Required(ErrorMessage = "Please select a postal code.")]
    [Display(Name = "Postal Code")]
    public int? PostalCode { get; set; }

    public IEnumerable<SelectListItem> PostalCodeOptions { get; set; } = new List<SelectListItem>();
}