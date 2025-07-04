namespace AutoInsight.API.DTOs
{
    public class VinDecodeResponse
    {
        public required string VIN { get; set; }

        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Manufacturer { get; set; }

        // --- New Properties ---
        public string? Series { get; set; }
        public string? Trim { get; set; }
        public string? GrossVehicleWeightRating { get; set; }
        public string? DriveType { get; set; }
        public string? Cylinders { get; set; } // NHTSA returns this as a string
        public string? PrimaryFuelType { get; set; }
        public string? SecondaryFuelType { get; set; }
        public string? ElectrificationLevel { get; set; }
        public string? EngineModel { get; set; }
        public string? EngineHorsepower { get; set; } // Using "EngineHP" from NHTSA
        public string? EngineManufacturer { get; set; }
        public string? EngineDisplacementL { get; set; } // Using "DisplacementL" from NHTSA
        public string? TransmissionSpeeds { get; set; }
        public string? TransmissionStyle { get; set; }
        // --- End New Properties ---

        public string? ErrorMessage { get; set; }
    }
}
