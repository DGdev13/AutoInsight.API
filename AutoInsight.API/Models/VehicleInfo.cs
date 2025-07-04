namespace AutoInsight.API.Models
{
    public class VehicleInfo
    {
        // VIN is likely always expected when creating a VehicleInfo object
        public required string VIN { get; set; }

        // These properties can be null if the data source doesn't provide them
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Manufacturer { get; set; }
        // You might add other properties here if VehicleInfo is a richer internal model
        // public string? BodyStyle { get; set; }
        // public string? EngineCylinders { get; set; }
    }
}