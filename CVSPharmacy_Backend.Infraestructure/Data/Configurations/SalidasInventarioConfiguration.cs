
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class SalidasInventarioConfiguration : IEntityTypeConfiguration<SalidasInventario>
{
    public void Configure(EntityTypeBuilder<SalidasInventario> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("salidas_inventario");

        entity.HasIndex(e => e.UsuarioId, "ix_salidas_inventario_usuario_id");

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.FechaSalida)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("fecha_salida");

        entity.Property(e => e.Motivo)
            .HasMaxLength(30)
            .HasColumnName("motivo");

        entity.Property(e => e.Observaciones)
            .HasMaxLength(255)
            .HasColumnName("observaciones");

        entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

        entity.HasOne(d => d.Usuario).WithMany(p => p.SalidasInventarios)
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_salidas_inventario_usuarios");
    }
}
