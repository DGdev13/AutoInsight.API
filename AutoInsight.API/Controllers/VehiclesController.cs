using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoInsight.API.Services;
using AutoInsight.API.DTOs;
using Microsoft.Extensions.Logging;
using AutoInsight.API.Helpers; // Added for VinValidator

namespace AutoInsight.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        /// <summary>
        /// Decode vehicle information from a VIN.
        /// </summary>
        /// <param name="vin">The 17-character VIN to decode.</param>
        /// <returns>Decoded vehicle information or an error message.</returns>
        [HttpGet("decode-vin/{vin}")]
        [ProducesResponseType(typeof(VinDecodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Use ApiErrorResponse
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Use ApiErrorResponse
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)] // Use ApiErrorResponse
        public async Task<IActionResult> DecodeVin(string vin)
        {
            // Perform basic VIN validation before calling the service
            if (!VinValidator.IsValidStandardVin(vin, out string? validationError))
            {
                _logger.LogWarning("Invalid VIN format provided: {VIN}. Error: {Error}", vin, validationError);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "INVALID_VIN_FORMAT",
                    Message = validationError ?? "The provided VIN is not in a valid format."
                });
            }

            var result = await _vehicleService.DecodeVinAsync(vin);

            // The service now guarantees a non-null VinDecodeResponse object.
            // We only need to check its ErrorMessage property.
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                _logger.LogWarning("VIN decoding failed for {VIN}: {ErrorMessage}", vin, result.ErrorMessage);

                // Differentiate error types for better client feedback using ApiErrorResponse
                if (result.ErrorMessage.Contains("no decodable data", StringComparison.OrdinalIgnoreCase) ||
                    result.ErrorMessage.Contains("insufficient Make/Model details", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Code = "VIN_DATA_NOT_FOUND",
                        Message = result.ErrorMessage
                    });
                }
                else if (result.ErrorMessage.Contains("External API HTTP error", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
                    {
                        Code = "EXTERNAL_API_ERROR",
                        Message = "Failed to retrieve VIN data from external source. Please try again later."
                    });
                }
                else if (result.ErrorMessage.Contains("Failed to parse NHTSA API response", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
                    {
                        Code = "EXTERNAL_API_PARSE_ERROR",
                        Message = "The external VIN decoding service returned an unparseable response."
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
                    {
                        Code = "INTERNAL_SERVER_ERROR",
                        Message = "An unexpected error occurred during VIN decoding. Please contact support."
                    });
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Get a mock price estimate for a vehicle.
        /// </summary>
        /// <param name="make">The vehicle's make (e.g., "Toyota").</param>
        /// <param name="model">The vehicle's model (e.g., "Camry").</param>
        /// <param name="year">The vehicle's model year (e.g., 2020).</param>
        /// <returns>A mock price estimate.</returns>
        [HttpGet("pricing")]
        [ProducesResponseType(typeof(PriceEstimateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Use ApiErrorResponse
        public async Task<IActionResult> GetPriceEstimate([FromQuery] string make, [FromQuery] string model, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model) || year < 1900 || year > DateTime.UtcNow.Year + 2)
            {
                _logger.LogWarning("Invalid input parameters for pricing: Make='{Make}', Model='{Model}', Year='{Year}'", make, model, year);
                return BadRequest(new ApiErrorResponse
                {
                    Code = "INVALID_PRICING_INPUT",
                    Message = "Invalid input parameters. Make, Model, and a valid Year (e.g., 1900-current year + 2) are required."
                });
            }

            var result = await _vehicleService.GetPriceEstimateAsync(make, model, year);

            return Ok(result);
        }
    }
}
