using System;
using System.Collections.Generic;
using CVSPharmacy_Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infraestructure.Data;

public partial class FarmaciaDbContext : DbContext
{
    public FarmaciaDbContext(DbContextOptions<FarmaciaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DetalleOrdenCompra> DetalleOrdenCompras { get; set; }

    public virtual DbSet<DetalleSalida> DetalleSalida { get; set; }

    public virtual DbSet<Lote> Lotes { get; set; }

    public virtual DbSet<OrdenesCompra> OrdenesCompras { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedores { get; set; }

    public virtual DbSet<Recepcion> Recepciones { get; set; }

    public virtual DbSet<Rol> Roles { get; set; }

    public virtual DbSet<SalidasInventario> SalidasInventarios { get; set; }

    public virtual DbSet<SolicitudesBaja> SolicitudesBajas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<DetalleOrdenCompra>(entity =>
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
        });

        modelBuilder.Entity<DetalleSalida>(entity =>
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
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lotes");

            entity.HasIndex(e => e.DetalleOrdenId, "ix_lotes_detalle_orden_id");

            entity.HasIndex(e => new { e.ProductoId, e.FechaVencimiento }, "ix_lotes_producto_vencimiento");

            entity.HasIndex(e => e.RecepcionId, "ix_lotes_recepcion_id");

            entity.HasIndex(e => new { e.ProductoId, e.NumeroLote }, "uq_lotes_producto_numero").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CantidadDisponible).HasColumnName("cantidad_disponible");
            entity.Property(e => e.CantidadRecibida).HasColumnName("cantidad_recibida");
            entity.Property(e => e.DetalleOrdenId).HasColumnName("detalle_orden_id");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.NumeroLote)
                .HasMaxLength(40)
                .HasColumnName("numero_lote");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.RecepcionId).HasColumnName("recepcion_id");

            entity.HasOne(d => d.DetalleOrden).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.DetalleOrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_detalle_orden");

            entity.HasOne(d => d.Producto).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_productos");

            entity.HasOne(d => d.Recepcion).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.RecepcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lotes_recepciones");
        });

        modelBuilder.Entity<OrdenesCompra>(entity =>
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
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos");

            entity.HasIndex(e => e.Codigo, "uq_productos_codigo").IsUnique();

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
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasColumnName("unidad_medida");
        });

        modelBuilder.Entity<Proveedor>(entity =>
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
        });

        modelBuilder.Entity<Recepcion>(entity =>
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
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "uq_roles_nombre").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<SalidasInventario>(entity =>
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
        });

        modelBuilder.Entity<SolicitudesBaja>(entity =>
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
        });

        modelBuilder.Entity<Usuario>(entity =>
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
