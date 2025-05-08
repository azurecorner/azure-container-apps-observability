namespace Healthtrackr.Api.Models
{
    public class Weight
    {
        public Guid WeightId { get; set; }
        public DateTime MeasurementDate { get; set; }
        public double BMI { get; set; }
        public double Fat { get; set; }
        public string? MeasurementSource { get; set; }
        public DateTime Time { get; set; }
        public double WeightInKG { get; set; }
    }
}
