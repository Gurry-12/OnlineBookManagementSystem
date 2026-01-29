using OnlineBookManagementSystem.Core.Domain.Entities;
using OnlineBookManagementSystem.Presentation.ViewModels.User;
using OnlineBookManagementSystem.Presentation.ViewModels.Admin;
using OnlineBookManagementSystem.Presentation.ViewModels.SuperAdmin;

namespace OnlineBookManagementSystem.Core.Application.Mappings
{
    /// <summary>
    /// Extension methods for mapping User entities to DTOs and ViewModels
    /// </summary>
    public static class UserMappingExtensions
    {
        /// <summary>
        /// Maps User entity to UserProfileViewModel
        /// </summary>
        public static UserProfileViewModel ToProfileViewModel(this User user, decimal totalSpent = 0)
        {
            if (user == null) return null;

            return new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country,
                TotalSpent = totalSpent,
                MemberSince = user.CreatedAt,
                IsEmailConfirmed = user.EmailConfirmed,
                IsActive = !user.IsDeleted
            };
        }

        /// <summary>
        /// Maps User entity to UserViewModel for admin views
        /// </summary>
        public static UserViewModel ToUserViewModel(this User user, string role = null)
        {
            if (user == null) return null;

            return new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = role ?? "User",
                IsActive = !user.IsDeleted,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// Maps collection of Users to AdminUsersViewModel
        /// </summary>
        public static AdminUsersViewModel ToAdminUsersViewModel(this IEnumerable<User> users, 
            int currentPage, int totalPages, int totalUsers,
            string searchTerm = null, string roleFilter = null, string statusFilter = null)
        {
            return new AdminUsersViewModel
            {
                Users = users?.Select(u => u.ToUserWithRoleViewModel()).ToList() ?? new List<UserWithRoleViewModel>(),
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalUsers = totalUsers,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                StatusFilter = statusFilter
            };
        }

        /// <summary>
        /// Maps User entity to ProfileViewModel for profile editing
        /// </summary>
        public static ProfileViewModel ToEditProfileViewModel(this User user)
        {
            if (user == null) return null;

            return new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country
            };
        }

        /// <summary>
        /// Updates User entity from ProfileViewModel
        /// </summary>
        public static void UpdateFromProfileViewModel(this User user, ProfileViewModel model)
        {
            if (user == null || model == null) return;

            user.Name = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.ZipCode = model.ZipCode;
            user.Country = model.Country;
            user.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps User to anonymous object for API responses
        /// </summary>
        public static object ToAnonymousObject(this User user)
        {
            if (user == null) return null;

            return new
            {
                user.Id,
                user.Name,
                user.Email
            };
        }

        /// <summary>
        /// Maps User to UserWithRoleViewModel
        /// </summary>
        public static UserWithRoleViewModel ToUserWithRoleViewModel(this User user, string role = "")
        {
            if (user == null) return null;

            return new UserWithRoleViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = role,
                RequestedRole = user.RequestedRole,
                IsDeleted = user.IsDeleted,
                IsPendingApproval = user.IsPendingApproval,
                LockoutEnd = user.LockoutEnd,
                LastLoginDate = user.LastLoginAt,
                CreatedDate = user.CreatedAt,
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed
            };
        }
    }
}