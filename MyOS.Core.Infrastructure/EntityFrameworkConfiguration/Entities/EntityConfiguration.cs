
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOS.Core.Domain.Entities;

namespace MyOS.Core.Infrastructure.EntityFrameworkConfiguration.Entities
{
    public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : Entity
    {
        public abstract void Configure(EntityTypeBuilder<TEntity> builder);
    }
}
