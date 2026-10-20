
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class DetalleOrdenCompraConfiguration : IEntityTypeConfiguration<DetalleOrdenCompra>
{
    public void Configure(EntityTypeBuilder<DetalleOrdenCompra> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("detalle_orden_compra");

        entity.HasIndex(e => e.ProductoId, "ix_detalle_orden_compra_producto_id");

        entity.HasIndex(e => new { e.OrdenId, e.ProductoId }, "uq_detalle_orden_compra_orden_producto").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.CantidadSolicitada).HasColumnName("cantidad_solicitada");
        entity.Property(e => e.OrdenId).HasColumnName("orden_id");

        entity.Property(e => e.PrecioUnitario)
            .HasPrecision(10, 2)
            .HasColumnName("precio_unitario");

        entity.Property(e => e.ProductoId).HasColumnName("producto_id");

        entity.HasOne(d => d.Orden).WithMany(p => p.DetalleOrdenCompras)
            .HasForeignKey(d => d.OrdenId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_detalle_orden_compra_orden");

        entity.HasOne(d => d.Producto).WithMany(p => p.DetalleOrdenCompras)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_detalle_orden_compra_producto");
    }
}
