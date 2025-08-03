using System;
using System.Text.RegularExpressions;

namespace ParticlesPlus
{
    public class RegexValidator
    {
        /// <summary>
        /// Validates if a regex pattern is syntactically correct
        /// </summary>
        /// <param name="pattern">The regex pattern to validate</param>
        /// <returns>True if the regex is valid, false otherwise</returns>
        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            try
            {
                // Attempt to create a Regex object with the pattern
                // This will throw an exception if the pattern is invalid
                var regex = new Regex(pattern);
                return true;
            }
            catch (ArgumentException)
            {
                // ArgumentException is thrown for invalid regex patterns
                return false;
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions
                return false;
            }
        }

        /// <summary>
        /// Validates a regex pattern and returns detailed error information
        /// </summary>
        /// <param name="pattern">The regex pattern to validate</param>
        /// <param name="errorMessage">Output parameter containing error details if validation fails</param>
        /// <returns>True if the regex is valid, false otherwise</returns>
        public static bool IsValidRegex(string pattern, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(pattern))
            {
                errorMessage = "Pattern cannot be null or empty";
                return false;
            }

            try
            {
                var regex = new Regex(pattern);
                return true;
            }
            catch (ArgumentException ex)
            {
                errorMessage = $"Invalid regex pattern: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Unexpected error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Validates a regex pattern with timeout to prevent catastrophic backtracking
        /// </summary>
        /// <param name="pattern">The regex pattern to validate</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default: 1000ms)</param>
        /// <returns>True if the regex is valid, false otherwise</returns>
        public static bool IsValidRegexWithTimeout(string pattern, int timeoutMs = 1000)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            try
            {
                var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(timeoutMs));
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Example usage and test method
        public static void TestRegexValidation()
        {
            string[] testPatterns = {
            @"(.*)",      // Valid
            @"(*\.)",     // Invalid - quantifier without preceding element
            @"[a-z]+",    // Valid
            @"[a-z",      // Invalid - unclosed character class
            @"\d{2,5}",   // Valid
            @"\d{2,}",    // Valid
            @"\d{,5}",    // Valid
            @"(?<name>\w+)", // Valid - named group
            @"(?<>\w+)",  // Invalid - empty group name
            @"abc|def",   // Valid - alternation
            @"|abc",      // Valid but questionable - empty alternative
        };

            Console.WriteLine("Regex Validation Results:");
            Console.WriteLine("========================");

            foreach (string pattern in testPatterns)
            {
                bool isValid = IsValidRegex(pattern, out string errorMessage);
                string status = isValid ? "✓ VALID" : "✗ INVALID";

                Console.WriteLine($"{status,-10} | {pattern,-15} | {errorMessage}");
            }
        }
    }
}