
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("proveedores");

        entity.HasIndex(e => e.Nit, "uq_proveedores_nit").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValueSql("'1'")
            .HasColumnName("activo");

        entity.Property(e => e.Contacto)
            .HasMaxLength(100)
            .HasColumnName("contacto");

        entity.Property(e => e.CreadoEn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("creado_en");

        entity.Property(e => e.Direccion)
            .HasMaxLength(200)
            .HasColumnName("direccion");

        entity.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        entity.Property(e => e.Nit)
            .HasMaxLength(20)
            .HasColumnName("nit");

        entity.Property(e => e.RazonSocial)
            .HasMaxLength(150)
            .HasColumnName("razon_social");

        entity.Property(e => e.Telefono)
            .HasMaxLength(30)
            .HasColumnName("telefono");
    }
}
