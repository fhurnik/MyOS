namespace MyOS.Core.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
    }
}
