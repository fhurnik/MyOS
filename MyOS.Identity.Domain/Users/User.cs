using MyOS.Core.Domain.Entities;

namespace MyOS.Identity.Domain.Users
{
    public class User : Entity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public string Login { get; private set; }
        public string PasswordHash { get; private set; }

        public bool IsActive { get; private set; }

        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset? UpdatedAtUtc { get; private set; }


        public static User Create(string firstName, string lastName, string login, string passwordHash)
        {
            return new User(firstName, lastName, login, passwordHash);
        }

        internal User(string firstName, string lastName, string login, string passwordHash)
        {
            FirstName = firstName;
            LastName = lastName;
            Login = login;
            PasswordHash = passwordHash;

            IsActive = true;
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }

        internal void Update(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        internal void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        internal void ChangeActiveStatus(bool isActive)
        {
            IsActive = isActive;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        private User()
        {
            // for EF Core
        }
    }
}
