namespace OnlineBookManagementSystem.Core.Domain.ValueObjects
{
    public class Address : IEquatable<Address>
    {
        public string FullName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Parameterless constructor for EF Core
        public Address() { }

        public Address(
            string fullName,
            string street,
            string city,
            string state,
            string zipCode,
            string country,
            string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be null or empty", nameof(fullName));
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be null or empty", nameof(street));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be null or empty", nameof(city));
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State cannot be null or empty", nameof(state));
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Zip code cannot be null or empty", nameof(zipCode));
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be null or empty", nameof(country));

            FullName = fullName.Trim();
            Street = street.Trim();
            City = city.Trim();
            State = state.Trim();
            ZipCode = zipCode.Trim();
            Country = country.Trim();
            PhoneNumber = phoneNumber?.Trim();
        }

        public string GetFormattedAddress()
        {
            return $"{Street}, {City}, {State} {ZipCode}, {Country}";
        }

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            return FullName == other.FullName &&
                   Street == other.Street &&
                   City == other.City &&
                   State == other.State &&
                   ZipCode == other.ZipCode &&
                   Country == other.Country &&
                   PhoneNumber == other.PhoneNumber;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Address);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FullName, Street, City, State, ZipCode, Country, PhoneNumber);
        }

        public override string ToString()
        {
            return $"{FullName}, {GetFormattedAddress()}";
        }

        public static bool operator ==(Address? left, Address? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address? left, Address? right)
        {
            return !Equals(left, right);
        }

        // Conversion operators
        public static implicit operator string(Address? address)
        {
            return address?.ToString() ?? string.Empty;
        }

        public static implicit operator Address(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new Address(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            // Simple parsing - in production you'd want more sophisticated parsing
            var parts = value.Split(',').Select(p => p.Trim()).ToArray();
            return new Address(
                parts.Length > 0 ? parts[0] : string.Empty,
                parts.Length > 1 ? parts[1] : string.Empty,
                parts.Length > 2 ? parts[2] : string.Empty,
                parts.Length > 3 ? parts[3] : string.Empty,
                parts.Length > 4 ? parts[4] : string.Empty,
                parts.Length > 5 ? parts[5] : string.Empty
            );
        }
    }
}