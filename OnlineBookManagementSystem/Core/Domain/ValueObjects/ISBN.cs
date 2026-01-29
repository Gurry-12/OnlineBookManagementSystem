using System.Text.RegularExpressions;

namespace OnlineBookManagementSystem.Core.Domain.ValueObjects
{
    public class ISBN : IEquatable<ISBN>
    {
        private static readonly Regex IsbnRegex = new(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$", RegexOptions.Compiled);

        public string Value { get; }

        public ISBN(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ISBN cannot be null or empty", nameof(value));

            var cleanValue = CleanIsbn(value);
            
            if (!IsValidIsbn(cleanValue))
                throw new ArgumentException("Invalid ISBN format", nameof(value));

            Value = cleanValue;
        }

        private static string CleanIsbn(string isbn)
        {
            return isbn.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        }

        private static bool IsValidIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn))
                return false;

            // Remove any remaining formatting
            isbn = isbn.Replace("-", "").Replace(" ", "");

            // Check ISBN-10
            if (isbn.Length == 10)
            {
                return IsValidIsbn10(isbn);
            }

            // Check ISBN-13
            if (isbn.Length == 13)
            {
                return IsValidIsbn13(isbn);
            }

            return false;
        }

        private static bool IsValidIsbn10(string isbn)
        {
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn[i]))
                    return false;
                sum += (isbn[i] - '0') * (10 - i);
            }

            char checkDigit = isbn[9];
            if (checkDigit == 'X')
                sum += 10;
            else if (char.IsDigit(checkDigit))
                sum += checkDigit - '0';
            else
                return false;

            return sum % 11 == 0;
        }

        private static bool IsValidIsbn13(string isbn)
        {
            if (!isbn.StartsWith("978") && !isbn.StartsWith("979"))
                return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                if (!char.IsDigit(isbn[i]))
                    return false;
                sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);
            }

            if (!char.IsDigit(isbn[12]))
                return false;

            int checkDigit = isbn[12] - '0';
            int calculatedCheckDigit = (10 - (sum % 10)) % 10;

            return checkDigit == calculatedCheckDigit;
        }

        public string GetFormattedValue()
        {
            if (Value.Length == 10)
            {
                return $"{Value[..1]}-{Value[1..6]}-{Value[6..9]}-{Value[9]}";
            }
            else if (Value.Length == 13)
            {
                return $"{Value[..3]}-{Value[3..4]}-{Value[4..9]}-{Value[9..12]}-{Value[12]}";
            }
            return Value;
        }

        public bool Equals(ISBN? other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ISBN);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return GetFormattedValue();
        }

        public string ToRawString()
        {
            return Value;
        }

        public static bool operator ==(ISBN? left, ISBN? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ISBN? left, ISBN? right)
        {
            return !Equals(left, right);
        }

        // Conversion operators
        public static implicit operator string(ISBN isbn)
        {
            return isbn?.Value ?? string.Empty;
        }

        public static implicit operator ISBN(string value)
        {
            return new ISBN(value);
        }

        public static ISBN Create(string value)
        {
            return new ISBN(value);
        }
    }
}