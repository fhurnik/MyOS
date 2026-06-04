using MyOS.Core.Domain.Entities;

namespace MyOS.Identity.Domain.Users
{
    public class User : Entity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }


        public static User Create(string firstName, string lastName, string email, string passwordHash)
        {
            return new User(firstName, lastName, email, passwordHash);
        }

        internal User(string firstName, string lastName, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;

            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Update(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        internal void ChangeActiveStatus(bool isActive)
        {
            IsActive = isActive;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private User()
        {
            // for EF Core
            FirstName = null!;
            LastName = null!;
            Email = null!;
            PasswordHash = null!;
        }
    }
}
