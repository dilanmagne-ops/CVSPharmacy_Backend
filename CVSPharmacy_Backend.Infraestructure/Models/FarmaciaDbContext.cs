using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CVSPharmacy_Backend.Infraestructure.Models;

public partial class FarmaciaDbContext : DbContext
{
    public FarmaciaDbContext(DbContextOptions<FarmaciaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Detalleordencompra> Detalleordencompras { get; set; }

    public virtual DbSet<Detallesalidum> Detallesalida { get; set; }

    public virtual DbSet<Lote> Lotes { get; set; }

    public virtual DbSet<Ordencompra> Ordencompras { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proovedor> Proovedors { get; set; }

    public virtual DbSet<Recepcion> Recepcions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Salidasinventario> Salidasinventarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Detalleordencompra>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.OrdenCompraId, e.OrdenCompraProovedorId, e.OrdenCompraUsuarioId, e.OrdenCompraUsuarioRolId, e.ProductoId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0, 0 });

            entity.ToTable("detalleordencompra");

            entity.HasIndex(e => new { e.OrdenCompraId, e.OrdenCompraProovedorId, e.OrdenCompraUsuarioId, e.OrdenCompraUsuarioRolId }, "fk_DetalleOrdenCompra_OrdenCompra1_idx");

            entity.HasIndex(e => e.ProductoId, "fk_DetalleOrdenCompra_Producto1_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.OrdenCompraId).HasColumnName("OrdenCompra_ID");
            entity.Property(e => e.OrdenCompraProovedorId).HasColumnName("OrdenCompra_Proovedor_ID");
            entity.Property(e => e.OrdenCompraUsuarioId).HasColumnName("OrdenCompra_Usuario_ID");
            entity.Property(e => e.OrdenCompraUsuarioRolId).HasColumnName("OrdenCompra_Usuario_Rol_ID");
            entity.Property(e => e.ProductoId).HasColumnName("Producto_ID");
            entity.Property(e => e.PrecioUsuario).HasPrecision(10, 2);

            entity.HasOne(d => d.Producto).WithMany(p => p.Detalleordencompras)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_DetalleOrdenCompra_Producto1");
        });

        modelBuilder.Entity<Detallesalidum>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.SalidasInventarioId, e.SalidasInventarioUsuarioId, e.SalidasInventarioUsuarioRolId, e.LoteId, e.LoteRecepcionId, e.LoteRecepcionOrdenCompraId, e.LoteRecepcionOrdenCompraProovedorId, e.LoteRecepcionOrdenCompraUsuarioId, e.LoteRecepcionOrdenCompraUsuarioRolId, e.LoteRecepcionOrdenCompraProductoId, e.LoteRecepcionUsuarioId, e.LoteRecepcionUsuarioRolId, e.LoteProductoId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            entity.ToTable("detallesalida");

            entity.HasIndex(e => new { e.LoteId, e.LoteRecepcionId, e.LoteRecepcionOrdenCompraId, e.LoteRecepcionOrdenCompraProovedorId, e.LoteRecepcionOrdenCompraUsuarioId, e.LoteRecepcionOrdenCompraUsuarioRolId, e.LoteRecepcionOrdenCompraProductoId, e.LoteRecepcionUsuarioId, e.LoteRecepcionUsuarioRolId, e.LoteProductoId }, "fk_DetalleSalida_Lote1_idx");

            entity.HasIndex(e => new { e.SalidasInventarioId, e.SalidasInventarioUsuarioId, e.SalidasInventarioUsuarioRolId }, "fk_DetalleSalida_SalidasInventario1_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.SalidasInventarioId).HasColumnName("SalidasInventario_ID");
            entity.Property(e => e.SalidasInventarioUsuarioId).HasColumnName("SalidasInventario_Usuario_ID");
            entity.Property(e => e.SalidasInventarioUsuarioRolId).HasColumnName("SalidasInventario_Usuario_Rol_ID");
            entity.Property(e => e.LoteId).HasColumnName("Lote_ID");
            entity.Property(e => e.LoteRecepcionId).HasColumnName("Lote_Recepcion_ID");
            entity.Property(e => e.LoteRecepcionOrdenCompraId).HasColumnName("Lote_Recepcion_OrdenCompra_ID");
            entity.Property(e => e.LoteRecepcionOrdenCompraProovedorId).HasColumnName("Lote_Recepcion_OrdenCompra_Proovedor_ID");
            entity.Property(e => e.LoteRecepcionOrdenCompraUsuarioId).HasColumnName("Lote_Recepcion_OrdenCompra_Usuario_ID");
            entity.Property(e => e.LoteRecepcionOrdenCompraUsuarioRolId).HasColumnName("Lote_Recepcion_OrdenCompra_Usuario_Rol_ID");
            entity.Property(e => e.LoteRecepcionOrdenCompraProductoId).HasColumnName("Lote_Recepcion_OrdenCompra_Producto_ID");
            entity.Property(e => e.LoteRecepcionUsuarioId).HasColumnName("Lote_Recepcion_Usuario_ID");
            entity.Property(e => e.LoteRecepcionUsuarioRolId).HasColumnName("Lote_Recepcion_Usuario_Rol_ID");
            entity.Property(e => e.LoteProductoId).HasColumnName("Lote_Producto_ID");

            entity.HasOne(d => d.Salidasinventario).WithMany(p => p.Detallesalida)
                .HasForeignKey(d => new { d.SalidasInventarioId, d.SalidasInventarioUsuarioId, d.SalidasInventarioUsuarioRolId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_DetalleSalida_SalidasInventario1");

            entity.HasOne(d => d.Lote).WithMany(p => p.Detallesalida)
                .HasForeignKey(d => new { d.LoteId, d.LoteRecepcionId, d.LoteRecepcionOrdenCompraId, d.LoteRecepcionOrdenCompraProovedorId, d.LoteRecepcionOrdenCompraUsuarioId, d.LoteRecepcionOrdenCompraUsuarioRolId, d.LoteRecepcionOrdenCompraProductoId, d.LoteRecepcionUsuarioId, d.LoteRecepcionUsuarioRolId, d.LoteProductoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_DetalleSalida_Lote1");
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.RecepcionId, e.RecepcionOrdenCompraId, e.RecepcionOrdenCompraProovedorId, e.RecepcionOrdenCompraUsuarioId, e.RecepcionOrdenCompraUsuarioRolId, e.RecepcionOrdenCompraProductoId, e.RecepcionUsuarioId, e.RecepcionUsuarioRolId, e.ProductoId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            entity.ToTable("lote");

            entity.HasIndex(e => e.ProductoId, "fk_Lote_Producto1_idx");

            entity.HasIndex(e => new { e.RecepcionId, e.RecepcionOrdenCompraId, e.RecepcionOrdenCompraProovedorId, e.RecepcionOrdenCompraUsuarioId, e.RecepcionOrdenCompraUsuarioRolId, e.RecepcionOrdenCompraProductoId, e.RecepcionUsuarioId, e.RecepcionUsuarioRolId }, "fk_Lote_Recepcion1_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.RecepcionId).HasColumnName("Recepcion_ID");
            entity.Property(e => e.RecepcionOrdenCompraId).HasColumnName("Recepcion_OrdenCompra_ID");
            entity.Property(e => e.RecepcionOrdenCompraProovedorId).HasColumnName("Recepcion_OrdenCompra_Proovedor_ID");
            entity.Property(e => e.RecepcionOrdenCompraUsuarioId).HasColumnName("Recepcion_OrdenCompra_Usuario_ID");
            entity.Property(e => e.RecepcionOrdenCompraUsuarioRolId).HasColumnName("Recepcion_OrdenCompra_Usuario_Rol_ID");
            entity.Property(e => e.RecepcionOrdenCompraProductoId).HasColumnName("Recepcion_OrdenCompra_Producto_ID");
            entity.Property(e => e.RecepcionUsuarioId).HasColumnName("Recepcion_Usuario_ID");
            entity.Property(e => e.RecepcionUsuarioRolId).HasColumnName("Recepcion_Usuario_Rol_ID");
            entity.Property(e => e.ProductoId).HasColumnName("Producto_ID");
            entity.Property(e => e.CantidadRecibida).HasMaxLength(45);
            entity.Property(e => e.FechaVencimiento).HasColumnType("datetime");
            entity.Property(e => e.NumeroLote).HasMaxLength(200);

            entity.HasOne(d => d.Producto).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Lote_Producto1");

            entity.HasOne(d => d.Recepcion).WithMany(p => p.Lotes)
                .HasForeignKey(d => new { d.RecepcionId, d.RecepcionOrdenCompraId, d.RecepcionOrdenCompraProovedorId, d.RecepcionOrdenCompraUsuarioId, d.RecepcionOrdenCompraUsuarioRolId, d.RecepcionOrdenCompraProductoId, d.RecepcionUsuarioId, d.RecepcionUsuarioRolId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Lote_Recepcion1");
        });

        modelBuilder.Entity<Ordencompra>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.ProovedorId, e.UsuarioId, e.UsuarioRolId, e.ProductoId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0 });

            entity.ToTable("ordencompra");

            entity.HasIndex(e => e.ProductoId, "fk_OrdenCompra_Producto1_idx");

            entity.HasIndex(e => e.ProovedorId, "fk_OrdenCompra_Proovedor1_idx");

            entity.HasIndex(e => new { e.UsuarioId, e.UsuarioRolId }, "fk_OrdenCompra_Usuario1_idx");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProovedorId).HasColumnName("Proovedor_ID");
            entity.Property(e => e.UsuarioId).HasColumnName("Usuario_ID");
            entity.Property(e => e.UsuarioRolId).HasColumnName("Usuario_Rol_ID");
            entity.Property(e => e.ProductoId).HasColumnName("Producto_ID");
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaEmision).HasColumnType("datetime");
            entity.Property(e => e.Numero).HasMaxLength(45);
            entity.Property(e => e.Observaciones).HasMaxLength(255);

            entity.HasOne(d => d.Producto).WithMany(p => p.Ordencompras)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_OrdenCompra_Producto1");

            entity.HasOne(d => d.Proovedor).WithMany(p => p.Ordencompras)
                .HasForeignKey(d => d.ProovedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_OrdenCompra_Proovedor1");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Ordencompras)
                .HasForeignKey(d => new { d.UsuarioId, d.UsuarioRolId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_OrdenCompra_Usuario1");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("producto");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Codigo).HasMaxLength(30);
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Creado_en");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.PrecioVenta)
                .HasPrecision(10, 2)
                .HasColumnName("Precio_Venta");
            entity.Property(e => e.Presentacion).HasMaxLength(100);
            entity.Property(e => e.PrincipioActivo)
                .HasMaxLength(150)
                .HasColumnName("Principio_Activo");
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(45)
                .HasColumnName("Unidad_Medida");
        });

        modelBuilder.Entity<Proovedor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("proovedor");

            entity.HasIndex(e => e.Contacto, "Contacto_UNIQUE").IsUnique();

            entity.HasIndex(e => e.Email, "Email_UNIQUE").IsUnique();

            entity.HasIndex(e => e.Telefono, "Telefono_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Contacto).HasMaxLength(100);
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Creado_en");
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Nit)
                .HasMaxLength(45)
                .HasColumnName("NIT");
            entity.Property(e => e.RazonSocial).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(45);
        });

        modelBuilder.Entity<Recepcion>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.OrdenCompraId, e.OrdenCompraProovedorId, e.OrdenCompraUsuarioId, e.OrdenCompraUsuarioRolId, e.OrdenCompraProductoId, e.UsuarioId, e.UsuarioRolId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            entity.ToTable("recepcion");

            entity.HasIndex(e => new { e.OrdenCompraId, e.OrdenCompraProovedorId, e.OrdenCompraUsuarioId, e.OrdenCompraUsuarioRolId, e.OrdenCompraProductoId }, "fk_Recepcion_OrdenCompra1_idx");

            entity.HasIndex(e => new { e.UsuarioId, e.UsuarioRolId }, "fk_Recepcion_Usuario1_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.OrdenCompraId).HasColumnName("OrdenCompra_ID");
            entity.Property(e => e.OrdenCompraProovedorId).HasColumnName("OrdenCompra_Proovedor_ID");
            entity.Property(e => e.OrdenCompraUsuarioId).HasColumnName("OrdenCompra_Usuario_ID");
            entity.Property(e => e.OrdenCompraUsuarioRolId).HasColumnName("OrdenCompra_Usuario_Rol_ID");
            entity.Property(e => e.OrdenCompraProductoId).HasColumnName("OrdenCompra_Producto_ID");
            entity.Property(e => e.UsuarioId).HasColumnName("Usuario_ID");
            entity.Property(e => e.UsuarioRolId).HasColumnName("Usuario_Rol_ID");
            entity.Property(e => e.FechaRecepcion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Recepcion");
            entity.Property(e => e.NroRemision).HasMaxLength(45);
            entity.Property(e => e.Observaciones).HasMaxLength(200);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Recepcions)
                .HasForeignKey(d => new { d.UsuarioId, d.UsuarioRolId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recepcion_Usuario1");

            entity.HasOne(d => d.Ordencompra).WithMany(p => p.Recepcions)
                .HasForeignKey(d => new { d.OrdenCompraId, d.OrdenCompraProovedorId, d.OrdenCompraUsuarioId, d.OrdenCompraUsuarioRolId, d.OrdenCompraProductoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recepcion_OrdenCompra1");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rol");

            entity.HasIndex(e => e.Id, "ID_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<Salidasinventario>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.UsuarioId, e.UsuarioRolId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("salidasinventario");

            entity.HasIndex(e => new { e.UsuarioId, e.UsuarioRolId }, "fk_SalidasInventario_Usuario1_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.UsuarioId).HasColumnName("Usuario_ID");
            entity.Property(e => e.UsuarioRolId).HasColumnName("Usuario_Rol_ID");
            entity.Property(e => e.FechaSalida).HasColumnType("datetime");
            entity.Property(e => e.Motivo).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(255);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Salidasinventarios)
                .HasForeignKey(d => new { d.UsuarioId, d.UsuarioRolId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_SalidasInventario_Usuario1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.RolId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("usuario");

            entity.HasIndex(e => e.RolId, "fk_Usuario_Rol_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.RolId).HasColumnName("Rol_ID");
            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Apellidos).HasMaxLength(120);
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Creado_en");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Nombres).HasMaxLength(120);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UltimoAcceso)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("Ultimo_Acceso");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
