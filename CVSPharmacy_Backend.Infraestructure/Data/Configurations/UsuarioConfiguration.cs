
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> entity)
    {
        entity.HasKey(e => e.Id).HasName("PRIMARY");

        entity.ToTable("usuarios");

        entity.HasIndex(e => e.RolId, "ix_usuarios_rol_id");
        entity.HasIndex(e => e.Email, "uq_usuarios_email").IsUnique();

        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.Activo)
            .IsRequired()
            .HasDefaultValueSql("'1'")
            .HasColumnName("activo");

        entity.Property(e => e.CreadoEn)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnType("datetime")
            .HasColumnName("creado_en");

        entity.Property(e => e.Email).HasColumnName("email");

        entity.Property(e => e.NombreCompleto)
            .HasMaxLength(120)
            .HasColumnName("nombre_completo");

        entity.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .HasColumnName("password_hash");

        entity.Property(e => e.RolId).HasColumnName("rol_id");

        entity.Property(e => e.UltimoAcceso)
            .HasColumnType("datetime")
            .HasColumnName("ultimo_acceso");

        entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
            .HasForeignKey(d => d.RolId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("fk_usuarios_roles");
    }
}
