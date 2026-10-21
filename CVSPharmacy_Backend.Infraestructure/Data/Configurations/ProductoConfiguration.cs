
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CVSPharmacy_Backend.Infraestructure.Data.Configurations
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos");

            entity.HasIndex(e => e.Codigo, "uq_productos_codigo")
                .IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");

            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .HasColumnName("codigo");

            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");

            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");

            entity.Property(e => e.PrecioVenta)
                .HasPrecision(10, 2)
                .HasColumnName("precio_venta");

            entity.Property(e => e.Presentacion)
                .HasMaxLength(100)
                .HasColumnName("presentacion");

            entity.Property(e => e.PrincipioActivo)
                .HasMaxLength(150)
                .HasColumnName("principio_activo");

            entity.Property(e => e.StockMinimo)
                .HasColumnName("stock_minimo");

            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasColumnName("unidad_medida");
        }
    }
}
