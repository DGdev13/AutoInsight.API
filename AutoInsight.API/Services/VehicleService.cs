using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoInsight.API.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq; // Added for .Any() and .FirstOrDefault()

namespace AutoInsight.API.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(HttpClient httpClient, ILogger<VehicleService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<VinDecodeResponse> DecodeVinAsync(string vin)
        {
            string? jsonString = null;

            try
            {
                var apiUrl = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevinvaluesextended/{vin}?format=json";
                _logger.LogInformation("Calling NHTSA API: {ApiUrl}", apiUrl);

                var httpResponse = await _httpClient.GetAsync(apiUrl);
                httpResponse.EnsureSuccessStatusCode();

                jsonString = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("NHTSA API raw response for VIN {VIN}: {Json}", vin, jsonString);

                var responseJson = JObject.Parse(jsonString);

                var resultsArray = responseJson?["Results"] as JArray;

                if (resultsArray == null || !resultsArray.Any())
                {
                    _logger.LogWarning("NHTSA API returned no decodable results for VIN: {VIN}. Raw JSON: {Json}", vin, jsonString);
                    return new VinDecodeResponse
                    {
                        VIN = vin,
                        ErrorMessage = "NHTSA API returned no decodable data for this VIN. It might be invalid, too old, or not a US-market vehicle."
                    };
                }

                var firstResultObject = resultsArray.FirstOrDefault() as JObject;

                if (firstResultObject == null)
                {
                    _logger.LogWarning("NHTSA API 'Results' array contained no valid JSON objects for VIN: {VIN}. Raw JSON: {Json}", vin, jsonString);
                    return new VinDecodeResponse
                    {
                        VIN = vin,
                        ErrorMessage = "NHTSA API 'Results' array contained unexpected data format."
                    };
                }

                // Extract core properties
                string? make = firstResultObject["Make"]?.ToString();
                string? model = firstResultObject["Model"]?.ToString();
                string? year = firstResultObject["ModelYear"]?.ToString(); // Note: It's "ModelYear", not "Model Year" in this endpoint
                string? manufacturer = firstResultObject["Manufacturer"]?.ToString();

                // --- Extracting New Properties ---
                string? series = firstResultObject["Series"]?.ToString();
                string? trim = firstResultObject["Trim"]?.ToString();
                string? gvwr = firstResultObject["GVWR"]?.ToString(); // Gross Vehicle Weight Rating
                string? driveType = firstResultObject["DriveType"]?.ToString();
                string? cylinders = firstResultObject["EngineCylinders"]?.ToString(); // "EngineCylinders"
                string? primaryFuelType = firstResultObject["FuelTypePrimary"]?.ToString();
                string? secondaryFuelType = firstResultObject["FuelTypeSecondary"]?.ToString();
                string? electrificationLevel = firstResultObject["ElectrificationLevel"]?.ToString();
                string? engineModel = firstResultObject["EngineModel"]?.ToString();
                string? engineHorsepower = firstResultObject["EngineHP"]?.ToString(); // "EngineHP"
                string? engineManufacturer = firstResultObject["EngineManufacturer"]?.ToString();
                string? engineDisplacementL = firstResultObject["DisplacementL"]?.ToString(); // "DisplacementL"
                string? transmissionSpeeds = firstResultObject["TransmissionSpeeds"]?.ToString();
                string? transmissionStyle = firstResultObject["TransmissionStyle"]?.ToString();
                // --- End New Properties Extraction ---


                _logger.LogInformation("Extracted values for VIN {VIN}: Make='{Make}', Model='{Model}', Year='{Year}', Manufacturer='{Manufacturer}'",
                                       vin, make ?? "null", model ?? "null", year ?? "null", manufacturer ?? "null");
                _logger.LogInformation("Checking condition: (IsNullOrEmpty(Make) && IsNullOrEmpty(Model)) for VIN {VIN}. Make is null/empty: {IsMakeEmpty}, Model is null/empty: {IsModelEmpty}",
                                       vin, string.IsNullOrEmpty(make), string.IsNullOrEmpty(model));

                if (string.IsNullOrEmpty(make) && string.IsNullOrEmpty(model))
                {
                    _logger.LogWarning("NHTSA API returned incomplete Make/Model data for VIN: {VIN}. Raw JSON: {Json}", vin, jsonString);
                    return new VinDecodeResponse
                    {
                        VIN = vin,
                        ErrorMessage = "Could not extract sufficient Make/Model details from VIN. Data might be incomplete."
                    };
                }

                return new VinDecodeResponse
                {
                    VIN = vin,
                    Make = make,
                    Model = model,
                    Year = year,
                    Manufacturer = manufacturer,
                    // --- Assign New Properties ---
                    Series = series,
                    Trim = trim,
                    GrossVehicleWeightRating = gvwr,
                    DriveType = driveType,
                    Cylinders = cylinders,
                    PrimaryFuelType = primaryFuelType,
                    SecondaryFuelType = secondaryFuelType,
                    ElectrificationLevel = electrificationLevel,
                    EngineModel = engineModel,
                    EngineHorsepower = engineHorsepower,
                    EngineManufacturer = engineManufacturer,
                    EngineDisplacementL = engineDisplacementL,
                    TransmissionSpeeds = transmissionSpeeds,
                    TransmissionStyle = transmissionStyle
                    // --- End New Properties Assignment ---
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error calling NHTSA API for VIN: {VIN}. Status Code: {StatusCode}", vin, ex.StatusCode);
                return new VinDecodeResponse
                {
                    VIN = vin,
                    ErrorMessage = $"External API HTTP error: {ex.Message}. Status Code: {ex.StatusCode}. Please try again later."
                };
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for VIN {VIN}. Raw JSON: {Json}", vin, jsonString);
                return new VinDecodeResponse
                {
                    VIN = vin,
                    ErrorMessage = $"Failed to parse NHTSA API response: {ex.Message}. The response format might have changed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during VIN decoding for VIN: {VIN}", vin);
                return new VinDecodeResponse
                {
                    VIN = vin,
                    ErrorMessage = $"An unexpected internal error occurred: {ex.Message}. Please contact support."
                };
            }
        }

        public Task<PriceEstimateResponse> GetPriceEstimateAsync(string make, string model, int year)
        {
            int estimatedPrice = AutoInsight.API.Helpers.PricingEngine.EstimatePrice(make, model, year);

            return Task.FromResult(new PriceEstimateResponse
            {
                Make = make,
                Model = model,
                Year = year,
                EstimatedPrice = estimatedPrice
            });
        }
    }
}