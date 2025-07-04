namespace AutoInsight.API.DTOs
{
    public class PriceEstimateResponse
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int EstimatedPrice { get; set; }
    }
}