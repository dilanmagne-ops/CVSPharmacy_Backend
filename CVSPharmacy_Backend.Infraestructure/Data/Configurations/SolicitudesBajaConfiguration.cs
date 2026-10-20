
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class SolicitudesBajaConfiguration : IEntityTypeConfiguration<SolicitudesBaja>
{
    public void Configure(EntityTypeBuilder<SolicitudesBaja> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("solicitudes_baja");

        entity.HasIndex(e => new { e.LoteId, e.Estado }, "ix_solicitudes_baja_lote_estado");
        entity.HasIndex(e => e.ResueltoPor, "ix_solicitudes_baja_resuelto_por");
        entity.HasIndex(e => e.SolicitadoPor, "ix_solicitudes_baja_solicitado_por");
        entity.HasIndex(e => e.SalidaId, "uq_solicitudes_baja_salida_id").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Cantidad).HasColumnName("cantidad");

        entity.Property(e => e.Estado)
            .HasMaxLength(20)
            .HasDefaultValueSql("'Pendiente'")
            .HasColumnName("estado");

        entity.Property(e => e.FechaResolucion)
            .HasColumnType("datetime")
            .HasColumnName("fecha_resolucion");

        entity.Property(e => e.FechaSolicitud)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("fecha_solicitud");

        entity.Property(e => e.LoteId).HasColumnName("lote_id");

        entity.Property(e => e.MotivoResolucion)
            .HasMaxLength(255)
            .HasColumnName("motivo_resolucion");

        entity.Property(e => e.Observacion)
            .HasMaxLength(255)
            .HasColumnName("observacion");

        entity.Property(e => e.ResueltoPor).HasColumnName("resuelto_por");
        entity.Property(e => e.SalidaId).HasColumnName("salida_id");
        entity.Property(e => e.SolicitadoPor).HasColumnName("solicitado_por");

        entity.HasOne(d => d.Lote).WithMany(p => p.SolicitudesBajas)
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_solicitudes_baja_lotes");

        entity.HasOne(d => d.ResueltoPorNavigation).WithMany(p => p.SolicitudesBajaResueltoPorNavigations)
            .HasForeignKey(d => d.ResueltoPor)
            .HasConstraintName("fk_solicitudes_baja_resolutor");

        entity.HasOne(d => d.Salida).WithOne(p => p.SolicitudesBaja)
            .HasForeignKey<SolicitudesBaja>(d => d.SalidaId)
            .HasConstraintName("fk_solicitudes_baja_salida");

        entity.HasOne(d => d.SolicitadoPorNavigation).WithMany(p => p.SolicitudesBajaSolicitadoPorNavigations)
            .HasForeignKey(d => d.SolicitadoPor)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_solicitudes_baja_solicitante");
    }
}
