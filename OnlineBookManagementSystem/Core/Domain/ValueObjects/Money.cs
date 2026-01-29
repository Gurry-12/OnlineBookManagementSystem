namespace OnlineBookManagementSystem.Core.Domain.ValueObjects
{
    public class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "USD")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");
            
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");
            
            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, Currency);
        }

        public bool IsZero => Amount == 0;
        public bool IsPositive => Amount > 0;
        public bool IsNegative => Amount < 0;

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString()
        {
            return $"{Amount:C} {Currency}";
        }

        public static bool operator ==(Money? left, Money? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Money? left, Money? right)
        {
            return !Equals(left, right);
        }

        public static Money operator +(Money left, Money right)
        {
            return left.Add(right);
        }

        public static Money operator -(Money left, Money right)
        {
            return left.Subtract(right);
        }

        public static Money operator *(Money left, decimal right)
        {
            return left.Multiply(right);
        }

        public static Money operator *(int left, Money right)
        {
            return right.Multiply(left);
        }

        // Conversion operators
        public static implicit operator decimal(Money money)
        {
            return money?.Amount ?? 0;
        }

        public static implicit operator Money(decimal amount)
        {
            return new Money(amount);
        }

        public static implicit operator Money(int amount)
        {
            return new Money(amount);
        }

        public static implicit operator Money(long amount)
        {
            return new Money(amount);
        }

        public static implicit operator Money(double amount)
        {
            return new Money((decimal)amount);
        }

        public static Money FromDecimal(decimal amount, string currency = "USD")
        {
            return new Money(amount, currency);
        }

        // Comparison operators
        public static bool operator >(Money left, decimal right)
        {
            return left.Amount > right;
        }

        public static bool operator <(Money left, decimal right)
        {
            return left.Amount < right;
        }

        public static bool operator >=(Money left, decimal right)
        {
            return left.Amount >= right;
        }

        public static bool operator <=(Money left, decimal right)
        {
            return left.Amount <= right;
        }
    }
}