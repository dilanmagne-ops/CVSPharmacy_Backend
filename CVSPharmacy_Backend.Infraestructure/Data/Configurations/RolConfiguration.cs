
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("roles");

        entity.HasIndex(e => e.Nombre, "uq_roles_nombre").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.Nombre)
            .HasMaxLength(30)
            .HasColumnName("nombre");
    }
}
