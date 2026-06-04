using MyOS.Core.Domain.Entities;
using MyOS.Core.Domain.Enums;

namespace MyOS.Identity.Domain.Users
{
    public class User : Entity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        public bool IsActive { get; private set; }
        public Language Language { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }


        public static User Create(string firstName, string lastName, string email, string passwordHash,
            Language language = Language.English)
        {
            return new User(firstName, lastName, email, passwordHash, language);
        }

        internal User(string firstName, string lastName, string email, string passwordHash,
            Language language = Language.English)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            Language = language;

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

        public void ChangeLanguage(Language language)
        {
            Language = language;
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
