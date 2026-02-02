namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        private string _token = string.Empty;

        public int UserId { get; set; }

        public string Token
        {
            get => _token;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Token cannot be null or empty", nameof(value));
                if (value.Length > 450)
                    throw new ArgumentException("Token cannot exceed 450 characters", nameof(value));
                _token = value;
            }
        }

        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? ReplacedByToken { get; set; }
        public string? CreatedByIp { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;

        // Private constructor for EF Core
        public RefreshToken() { }

        public RefreshToken(int userId, string token, DateTime expiryDate, string? createdByIp = null)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive", nameof(userId));
            if (expiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future", nameof(expiryDate));

            UserId = userId;
            Token = token;
            ExpiryDate = expiryDate;
            CreatedByIp = createdByIp?.Length > 45 ? createdByIp[..45] : createdByIp;
            Created = DateTime.UtcNow;
        }

        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            ReplacedByToken = replacedByToken?.Length > 450 ? replacedByToken[..450] : replacedByToken;
            UpdateTimestamp();
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiryDate;
        }

        public bool IsActive()
        {
            return !IsRevoked && !IsExpired();
        }
    }
}