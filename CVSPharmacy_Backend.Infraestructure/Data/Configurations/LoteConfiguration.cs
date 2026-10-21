
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations
{
    public class LoteConfiguration : IEntityTypeConfiguration<Lote>
    {
        public void Configure(EntityTypeBuilder<Lote> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lotes");

            entity.HasIndex(e => e.DetalleOrdenId,
                "ix_lotes_detalle_orden_id");

            entity.HasIndex(
                e => new { e.ProductoId, e.FechaVencimiento },
                "ix_lotes_producto_vencimiento");

            entity.HasIndex(e => e.RecepcionId,
                "ix_lotes_recepcion_id");

            entity.HasIndex(
                e => new { e.ProductoId, e.NumeroLote },
                "uq_lotes_producto_numero").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CantidadDisponible)
                .HasColumnName("cantidad_disponible");

            entity.Property(e => e.CantidadRecibida)
                .HasColumnName("cantidad_recibida");

            entity.Property(e => e.DetalleOrdenId)
                .HasColumnName("detalle_orden_id");

            entity.Property(e => e.FechaVencimiento)
                .HasColumnName("fecha_vencimiento");

            entity.Property(e => e.NumeroLote)
                .HasMaxLength(40)
                .HasColumnName("numero_lote");

            entity.Property(e => e.ProductoId)
                .HasColumnName("producto_id");

            entity.Property(e => e.RecepcionId)
                .HasColumnName("recepcion_id");

            entity.HasOne(d => d.DetalleOrden)
                .WithMany(p => p.Lotes)
                .HasForeignKey(d => d.DetalleOrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_detalle_orden");

            entity.HasOne(d => d.Producto)
                .WithMany(p => p.Lotes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_productos");

            entity.HasOne(d => d.Recepcion)
                .WithMany(p => p.Lotes)
                .HasForeignKey(d => d.RecepcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_recepciones");
        }
    }
}
