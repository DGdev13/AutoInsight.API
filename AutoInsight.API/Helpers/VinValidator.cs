using System.Text.RegularExpressions;

namespace AutoInsight.API.Helpers
{
    /// <summary>
    /// Provides static methods for validating Vehicle Identification Numbers (VINs).
    /// </summary>
    public static class VinValidator
    {
        // Disallowed characters in a VIN (I, O, Q) to prevent confusion with 1, 0.
        private static readonly char[] DisallowedVinCharacters = { 'I', 'O', 'Q' };

        /// <summary>
        /// Validates if a VIN is a standard 17-character VIN and contains no disallowed characters.
        /// Note: This does NOT perform a checksum validation, which is more complex and typically done by VIN decoding services.
        /// </summary>
        /// <param name="vin">The VIN string to validate.</param>
        /// <param name="errorMessage">Output parameter: A detailed error message if validation fails.</param>
        /// <returns>True if the VIN is valid, false otherwise.</returns>
        public static bool IsValidStandardVin(string vin, out string? errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(vin))
            {
                errorMessage = "VIN cannot be empty or whitespace.";
                return false;
            }

            // Standard VINs are exactly 17 characters long for vehicles manufactured since 1981.
            if (vin.Length != 17)
            {
                errorMessage = "VIN must be exactly 17 characters long for standard decoding.";
                return false;
            }

            // Check for disallowed characters (I, O, Q)
            foreach (char c in DisallowedVinCharacters)
            {
                if (vin.Contains(c, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = $"VIN contains disallowed character '{c}'. VINs do not use I, O, or Q.";
                    return false;
                }
            }

            // Optionally, you could add a regex for allowed alphanumeric characters
            // if (!Regex.IsMatch(vin, "^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.IgnoreCase))
            // {
            //     errorMessage = "VIN contains invalid characters. Only A-H, J-N, P-R, S-Z, 0-9 are allowed.";
            //     return false;
            // }

            return true;
        }
    }
}