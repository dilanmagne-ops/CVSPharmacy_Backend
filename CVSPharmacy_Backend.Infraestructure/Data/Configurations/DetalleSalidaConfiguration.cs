
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class DetalleSalidaConfiguration : IEntityTypeConfiguration<DetalleSalida>
{
    public void Configure(EntityTypeBuilder<DetalleSalida> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("detalle_salida");

        entity.HasIndex(e => e.LoteId, "ix_detalle_salida_lote_id");

        entity.HasIndex(e => new { e.SalidaId, e.LoteId }, "uq_detalle_salida_salida_lote").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Cantidad).HasColumnName("cantidad");
        entity.Property(e => e.LoteId).HasColumnName("lote_id");
        entity.Property(e => e.SalidaId).HasColumnName("salida_id");

        entity.HasOne(d => d.Lote).WithMany(p => p.DetalleSalida)
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_detalle_salida_lotes");

        entity.HasOne(d => d.Salida).WithMany(p => p.DetalleSalida)
            .HasForeignKey(d => d.SalidaId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_detalle_salida_salidas_inventario");
    }
}
