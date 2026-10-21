
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class OrdenesCompraConfiguration : IEntityTypeConfiguration<OrdenesCompra>
{
    public void Configure(EntityTypeBuilder<OrdenesCompra> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("ordenes_compra");

        entity.HasIndex(e => e.CreadoPor, "ix_ordenes_compra_creado_por");
        entity.HasIndex(e => e.ProveedorId, "ix_ordenes_compra_proveedor_id");
        entity.HasIndex(e => e.Numero, "uq_ordenes_compra_numero").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.CreadoPor).HasColumnName("creado_por");

        entity.Property(e => e.Estado)
            .HasMaxLength(20)
            .HasDefaultValueSql("'Borrador'")
            .HasColumnName("estado");

        entity.Property(e => e.FechaCreacion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("fecha_creacion");

        entity.Property(e => e.FechaEmision)
            .HasColumnType("datetime")
            .HasColumnName("fecha_emision");

        entity.Property(e => e.Numero)
            .HasMaxLength(20)
            .HasColumnName("numero");

        entity.Property(e => e.Observaciones)
            .HasMaxLength(255)
            .HasColumnName("observaciones");

        entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");

        entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.OrdenesCompras)
            .HasForeignKey(d => d.CreadoPor)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_ordenes_compra_usuarios");

        entity.HasOne(d => d.Proveedor).WithMany(p => p.OrdenesCompras)
            .HasForeignKey(d => d.ProveedorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_ordenes_compra_proveedores");
    }
}
