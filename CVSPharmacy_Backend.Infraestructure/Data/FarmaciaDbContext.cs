using System;
using System.Reflection;
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

        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
