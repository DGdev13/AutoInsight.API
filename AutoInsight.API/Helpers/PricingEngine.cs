using System;

namespace AutoInsight.API.Helpers
{
    public static class PricingEngine
    {
        /// <summary>
        /// Returns a mock price estimate based on vehicle age.
        /// </summary>
        /// <param name="make">Make of the vehicle</param>
        /// <param name="model">Model of the vehicle</param>
        /// <param name="year">Year of the vehicle</param>
        /// <returns>Estimated resale price</returns>
        public static int EstimatePrice(string make, string model, int year)
        {
            int currentYear = DateTime.UtcNow.Year;
            int age = currentYear - year;

            // Ensure age is valid
            if (age < 0 || age > 50)
                age = 20;

            // Simple depreciation model
            int baseValue = 15000;
            int depreciationPerYear = 800;
            int estimated = baseValue - (age * depreciationPerYear);

            // Never drop below minimum threshold
            return Math.Max(estimated, 500);
        }
    }
}