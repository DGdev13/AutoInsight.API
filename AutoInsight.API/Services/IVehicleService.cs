using System.Threading.Tasks;
using AutoInsight.API.DTOs;

namespace AutoInsight.API.Services
{
    public interface IVehicleService
    {
        Task<VinDecodeResponse> DecodeVinAsync(string vin);
        Task<PriceEstimateResponse> GetPriceEstimateAsync(string make, string model, int year);
    }
}
