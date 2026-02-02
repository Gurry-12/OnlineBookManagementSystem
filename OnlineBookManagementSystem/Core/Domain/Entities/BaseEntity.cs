namespace OnlineBookManagementSystem.Core.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Concurrency token for optimistic concurrency control.
        /// SQLite-compatible alternative to RowVersion using GUID.
        /// </summary>
        public Guid ConcurrencyToken { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            ConcurrencyToken = Guid.NewGuid();
            IsDeleted = false;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdateTimestamp();
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
            ConcurrencyToken = Guid.NewGuid(); // Generate new token on update
        }

        protected void SetId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be positive", nameof(id));
            Id = id;
        }
    }
}