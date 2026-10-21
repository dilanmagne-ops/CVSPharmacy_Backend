
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class RecepcionConfiguration : IEntityTypeConfiguration<Recepcion>
{
    public void Configure(EntityTypeBuilder<Recepcion> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("recepciones");

        entity.HasIndex(e => e.OrdenId, "ix_recepciones_orden_id");
        entity.HasIndex(e => e.RecibidoPor, "ix_recepciones_recibido_por");

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.FechaRecepcion)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("fecha_recepcion");

        entity.Property(e => e.NroRemision)
            .HasMaxLength(40)
            .HasColumnName("nro_remision");

        entity.Property(e => e.Observaciones)
            .HasMaxLength(255)
            .HasColumnName("observaciones");

        entity.Property(e => e.OrdenId).HasColumnName("orden_id");
        entity.Property(e => e.RecibidoPor).HasColumnName("recibido_por");

        entity.HasOne(d => d.Orden).WithMany(p => p.Recepciones)
            .HasForeignKey(d => d.OrdenId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_recepciones_ordenes_compra");

        entity.HasOne(d => d.RecibidoPorNavigation).WithMany(p => p.Recepciones)
            .HasForeignKey(d => d.RecibidoPor)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_recepciones_usuarios");
    }
}
