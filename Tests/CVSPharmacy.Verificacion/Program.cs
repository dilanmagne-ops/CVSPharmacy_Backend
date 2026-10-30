using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using AutoMapper;
using CVSPharmacy_Backend.Api.Filters;
using CVSPharmacy_Backend.Core.Dtos.OrdenCompra;
using CVSPharmacy_Backend.Core.Dtos.Recepcion;
using CVSPharmacy_Backend.Core.Entities;
using CVSPharmacy_Backend.Core.Exceptions;
using CVSPharmacy_Backend.Core.Interfaces;
using CVSPharmacy_Backend.Infraestructure.Data;
using CVSPharmacy_Backend.Infraestructure.Mappings;
using CVSPharmacy_Backend.Infraestructure.Repositories;
using CVSPharmacy_Backend.Services.Interfaces;
using CVSPharmacy_Backend.Services.Services;
using CVSPharmacy_Backend.Services.Validators.OrdenCompra;
using CVSPharmacy_Backend.Services.Validators.Recepcion;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var mapperConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductoProfile).Assembly));
mapperConfig.AssertConfigurationIsValid();
var mapper = mapperConfig.CreateMapper();
int passed = 0;
async Task Test(string name, Func<Task> run)
{
    await run(); passed++; Console.WriteLine($"PASS {name}");
}
void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
async Task Expected(int status, Func<Task> run)
{
    try { await run(); }
    catch (BusinessException e) when ((int)e.StatusCode == status) { return; }
    throw new Exception($"Se esperaba BusinessException HTTP {status}");
}
async Task Invalid(Func<Task> run)
{
    try { await run(); } catch (ValidationException) { return; }
    throw new Exception("Se esperaba error de validación");
}
GuardarRecepcionDto Input(int quantity = 20, string number = "L-A", int product = 1, int days = 30) => new()
{
    Items = [new() { ProductoId = product, Cantidad = quantity, NumeroLote = number,
        FechaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(days)) }]
};
Fixture F() => new(mapper);
await Test("AutoMapper: todos los perfiles válidos", () => Task.CompletedTask);
await Test("Recepción parcial: 20/50, stock y responsable", async () =>
{
    var f = F(); var r = await f.Service.RegistrarAsync(1, Input());
    Check(f.Order.Estado == EstadosOrden.Parcial && r.Lotes.Single().CantidadDisponible == 20 && r.RecibidoPor == 1, "Parcial incorrecta");
});
await Test("Recepción completa acumulada: 20 + 30, exceso posterior 409", async () =>
{
    var f=F(); await f.Service.RegistrarAsync(1,Input()); await f.Service.RegistrarAsync(1,Input(30,"L-B"));
    Check(f.Order.Estado==EstadosOrden.Recibida && f.Db.All<Lote>().Sum(l=>l.CantidadRecibida)==50,"Completa incorrecta");
    await Expected(409,()=>f.Service.RegistrarAsync(1,Input(1,"L-C")));
});
await Test("Se usa recibido histórico y no disponible para pendientes", async () =>
{
    var f=F(); await f.Service.RegistrarAsync(1,Input()); f.Db.All<Lote>().Single().CantidadDisponible=0;
    await Expected(422,()=>f.Service.RegistrarAsync(1,Input(31,"L-B")));
});
await Test("Exceso 422 rechaza todo sin modificar estado o inventario", async () =>
{
    var f=F(); await Expected(422,()=>f.Service.RegistrarAsync(1,Input(51)));
    Check(f.Db.All<Recepcion>().Count==0 && f.Db.All<Lote>().Count==0 && f.Order.Estado==EstadosOrden.Emitida,"Hubo escritura parcial");
});
await Test("Suma varios lotes del mismo producto antes de validar pendiente", async () =>
{
    var f=F(); var dto=Input(30); dto.Items.Add(Input(25,"L-B").Items[0]);
    await Expected(422,()=>f.Service.RegistrarAsync(1,dto)); Check(f.Db.All<Lote>().Count==0,"Sobre-recepción");
});
await Test("Vencimiento hoy o pasado produce 422", async () =>
{
    await Expected(422,()=>F().Service.RegistrarAsync(1,Input(days:0)));
    await Expected(422,()=>F().Service.RegistrarAsync(1,Input(days:-1)));
});
await Test("Producto ajeno produce 422",()=>Expected(422,()=>F().Service.RegistrarAsync(1,Input(product:2))));
await Test("Lote existente produce 409", async () =>
{
    var f=F(); await f.Service.RegistrarAsync(1,Input()); await Expected(409,()=>f.Service.RegistrarAsync(1,Input()));
});
await Test("Lotes duplicados en la solicitud se rechazan tras normalizar espacios", async () =>
{
    var f=F(); var dto=Input(10);dto.Items.Add(Input(10," L-A ").Items[0]);
    await Expected(409,()=>f.Service.RegistrarAsync(1,dto));Check(f.Db.All<Lote>().Count==0,"Duplicación");
});
await Test("Orden inexistente 404; Borrador, Cancelada y Recibida 409", async () =>
{
    await Expected(404,()=>F().Service.RegistrarAsync(999,Input()));
    foreach(var state in new[]{EstadosOrden.Borrador,EstadosOrden.Cancelada,EstadosOrden.Recibida})
    {var f=F();f.Order.Estado=state;await Expected(409,()=>f.Service.RegistrarAsync(1,Input()));}
});
await Test("Operador no puede recibir ni consultar recepciones", async () =>
{
    var f=F();f.Actor.Role="Operador";
    await Expected(403,()=>f.Service.RegistrarAsync(1,Input()));await Expected(403,()=>f.Service.ConsultarAsync(1));
});
await Test("Validación de cuerpo nulo, lista vacía, ítem nulo y cantidad cero", async () =>
{
    await Expected(400,()=>F().Service.RegistrarAsync(1,null!));
    await Invalid(()=>F().Service.RegistrarAsync(1,new()));
    await Invalid(()=>F().Service.RegistrarAsync(1,new(){Items=[null!]}));
    await Invalid(()=>F().Service.RegistrarAsync(1,Input(0)));
});
await Test("Fallo al guardar revierte recepción, lotes y estado", async () =>
{
    var f=F();f.Db.FailSave=true;
    await Expected(409,()=>f.Service.RegistrarAsync(1,Input()));
    Check(f.Db.All<Recepcion>().Count==0 && f.Db.All<Lote>().Count==0 && f.Order.Estado==EstadosOrden.Emitida,"Rollback incompleto");
});
await Test("Dos productos: no completar hasta recibir todas las líneas", async () =>
{
    var f=F();f.Db.All<DetalleOrdenCompra>().Add(new(){Id=2,OrdenId=1,ProductoId=2,CantidadSolicitada=10});
    await f.Service.RegistrarAsync(1,Input(50));Check(f.Order.Estado==EstadosOrden.Parcial,"Se completó antes de tiempo");
    await f.Service.RegistrarAsync(1,Input(10,"L-2",2));Check(f.Order.Estado==EstadosOrden.Recibida,"No completó");
});
await Test("Middleware entrega 422 estructurado y oculta detalles internos de 500", async () =>
{
    foreach(var expected in new[]{422,500})
    {
        var context=new DefaultHttpContext();context.Response.Body=new MemoryStream();
        var middleware=new ExceptionHandlingMiddleware(_=>throw (expected==422
            ?new BusinessException("Regla de prueba",HttpStatusCode.UnprocessableEntity):new Exception("secreto interno")),NullLogger<ExceptionHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(context);context.Response.Body.Position=0;
        var json=await new StreamReader(context.Response.Body).ReadToEndAsync();
        Check(context.Response.StatusCode==expected && json.Contains("traceId") && !json.Contains("secreto interno"),"Error HTTP incorrecto");
    }
});
await Test("CU05: validación rechaza productos duplicados y nulos",async()=>
{
    var v=new GuardarOrdenCompraDtoValidator();
    var dto=new GuardarOrdenCompraDto(){ProveedorId=1,Items=[new(){ProductoId=1,CantidadSolicitada=1,PrecioUnitario=1},new(){ProductoId=1,CantidadSolicitada=1,PrecioUnitario=1}]};
    Check(!(await v.ValidateAsync(dto)).IsValid,"Producto duplicado aceptado");dto.Items=[null!];
    Check(!(await v.ValidateAsync(dto)).IsValid,"Ítem nulo aceptado");
});
await Test("Modelo EF: tablas independientes, clave heredada e índice único de lote",()=>
{
    using var db=new FarmaciaDbContext(new DbContextOptionsBuilder<FarmaciaDbContext>().UseMySql("Server=localhost;Database=test;User=test",new MySqlServerVersion(new Version(8,0,36))).Options);
    foreach(var entity in db.Model.GetEntityTypes())
        Check(entity.BaseType is null && entity.FindPrimaryKey() is not null,"Herencia relacional inesperada");
    Check(db.Model.FindEntityType(typeof(Lote))!.GetIndexes().Any(i=>i.IsUnique && i.Properties.Count==2),"Falta índice de lote");
    return Task.CompletedTask;
});
Console.WriteLine($"RESULTADO: {passed} pruebas sin servidor MySQL completadas.");

if(args.Length>0 && args[0]=="--check-user")
{
    using var config=JsonDocument.Parse(File.ReadAllText(args[1]),new JsonDocumentOptions{CommentHandling=JsonCommentHandling.Skip,AllowTrailingCommas=true});
    var connection=config.RootElement.GetProperty("ConnectionStrings").GetProperty("ConnectionMySql").GetString()!;
    using var db=new FarmaciaDbContext(new DbContextOptionsBuilder<FarmaciaDbContext>().UseMySql(connection,ServerVersion.AutoDetect(connection)).Options);
    var user=await db.Usuarios.AsNoTracking().Include(u=>u.Rol).Where(u=>u.Activo==true && u.Rol.Nombre=="Administrador").OrderBy(u=>u.Id).FirstOrDefaultAsync();
    Console.WriteLine(user is null?"No hay Administrador activo: necesita preparar un usuario de prueba.":$"Administrador activo disponible: UsuarioId={user.Id}. Consulta de solo lectura.");
}

if(args.Length>0 && args[0]=="--prepare-development")
{
    // Opt-in manual: este comando sí crea el usuario local necesario para Development.
    using var config=JsonDocument.Parse(File.ReadAllText(args[1]),new JsonDocumentOptions{CommentHandling=JsonCommentHandling.Skip,AllowTrailingCommas=true});
    var connection=config.RootElement.GetProperty("ConnectionStrings").GetProperty("ConnectionMySql").GetString()!;
    var csb=new MySqlConnectionStringBuilder(connection);
    if(csb.Server is not ("localhost" or "127.0.0.1" or "::1"))throw new Exception("Solo se admite una base local para preparar Development.");
    using var db=new FarmaciaDbContext(new DbContextOptionsBuilder<FarmaciaDbContext>().UseMySql(connection,ServerVersion.AutoDetect(connection)).Options);
    await using var transaction=await db.Database.BeginTransactionAsync();
    var admin=await db.Usuarios.Include(u=>u.Rol).Where(u=>u.Activo==true && u.Rol.Nombre=="Administrador").FirstOrDefaultAsync();
    if(admin is null)
    {
        if(await db.Usuarios.AnyAsync(u=>u.Email=="desarrollo@farmacia.test"))throw new Exception("Ya existe la cuenta reservada de desarrollo, pero no está activa como Administrador. Revísela manualmente.");
        var rol=await db.Roles.FirstOrDefaultAsync(r=>r.Nombre=="Administrador")??new Rol{Nombre="Administrador"};
        admin=new Usuario{NombreCompleto="Administrador de desarrollo",Email="desarrollo@farmacia.test",Activo=true,Rol=rol};
        // No se usa contraseña compartida ni se conserva una clave en texto plano.
        var randomPassword=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        admin.PasswordHash=new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>().HashPassword(admin,randomPassword);
        db.Add(admin);await db.SaveChangesAsync();
    }
    await transaction.CommitAsync();
    Console.WriteLine($"Development preparado: configure Desarrollo:UsuarioId={admin.Id}. No implementa login ni publica una contraseña.");
}

if(args.Length>0 && args[0]=="--mysql")
{
    // Nunca se ejecuta contra la base del usuario: se usa una base desechable con nombre único.
    var configPath=Path.GetFullPath(args[1]);
    using var config=JsonDocument.Parse(File.ReadAllText(configPath),new JsonDocumentOptions{CommentHandling=JsonCommentHandling.Skip,AllowTrailingCommas=true});
    var original=config.RootElement.GetProperty("ConnectionStrings").GetProperty("ConnectionMySql").GetString()!;
    var csb=new MySqlConnectionStringBuilder(original);
    if(csb.Server is not ("localhost" or "127.0.0.1" or "::1")) throw new Exception("Las pruebas automáticas MySQL solo admiten servidor local.");
    csb.Database="cvs_test_"+Guid.NewGuid().ToString("N");csb.ConnectionTimeout=5;
    var testDb=csb.Database;
    csb.Database=""; var serverVersion=ServerVersion.AutoDetect(csb.ConnectionString);csb.Database=testDb;
    DbContextOptions<FarmaciaDbContext> Options()=>new DbContextOptionsBuilder<FarmaciaDbContext>().UseMySql(csb.ConnectionString,serverVersion).Options;
    RecepcionService Service(FarmaciaDbContext db)=>new(new BaseRepository<OrdenesCompra>(db),new BaseRepository<DetalleOrdenCompra>(db),new BaseRepository<Recepcion>(db),new BaseRepository<Lote>(db),new UnidadTrabajo(db),new Actor(),mapper,new(),TimeProvider.System);
    try
    {
        using(var db=new FarmaciaDbContext(Options()))
        {
            await db.Database.EnsureCreatedAsync();
            var rol=new Rol{Nombre="Administrador"};var user=new Usuario{NombreCompleto="Prueba",Email="test@example.com",PasswordHash="no-es-una-clave-real",Activo=true,Rol=rol};
            var provider=new Proveedor{Nit="test-1",RazonSocial="Prueba",Activo=true};
            var product=new Producto{Codigo="TEST1",Nombre="Prueba",Presentacion="Caja",UnidadMedida="Unidad",Activo=true};
            db.AddRange(user,provider,product);await db.SaveChangesAsync();
            var operatorRole=new Rol{Nombre="Operador"};
            db.Add(new Usuario{NombreCompleto="Operador prueba",Email="operator@example.com",PasswordHash="no-es-una-clave-real",Activo=true,Rol=operatorRole});
            await db.SaveChangesAsync();
            Check(user.Id==1 && product.Id==1,"IDs de prueba inesperados");
            var order=new OrdenesCompra{Numero="OC-TEST-0001",ProveedorId=provider.Id,CreadoPor=user.Id,Estado=EstadosOrden.Emitida,FechaCreacion=DateTime.Now,
                DetalleOrdenCompras=[new(){ProductoId=product.Id,CantidadSolicitada=50,PrecioUnitario=1}]};
            db.Add(order);await db.SaveChangesAsync();
        }
        await Test("MYSQL: recepción parcial confirmada en base real",async()=>
        {
            using(var db=new FarmaciaDbContext(Options()))await Service(db).RegistrarAsync(1,Input());
            using var verify=new FarmaciaDbContext(Options());
            Check((await verify.OrdenesCompras.SingleAsync()).Estado==EstadosOrden.Parcial && await verify.Lotes.SumAsync(l=>l.CantidadDisponible)==20,"Persistencia incorrecta");
        });
        await Test("MYSQL: exceso rechazado sin filas ni cambios parciales",async()=>
        {
            using(var db=new FarmaciaDbContext(Options()))await Expected(422,()=>Service(db).RegistrarAsync(1,Input(31,"L-B")));
            using var verify=new FarmaciaDbContext(Options());Check(await verify.Recepciones.CountAsync()==1 && await verify.Lotes.CountAsync()==1,"Se guardó recepción inválida");
        });
        await Test("MYSQL: dos recepciones simultáneas no exceden el pedido",async()=>
        {
            async Task<int> Receive(string lote)
            {
                using var db=new FarmaciaDbContext(Options());
                try{await Service(db).RegistrarAsync(1,Input(30,lote));return 201;}
                catch(BusinessException ex){return (int)ex.StatusCode;}
            }
            var codes=await Task.WhenAll(Receive("L-C"),Receive("L-D"));
            Check(codes.Count(c=>c==201)==1 && codes.Count(c=>c==409)==1,"Concurrencia no protegida");
            using var verify=new FarmaciaDbContext(Options());
            Check(await verify.Lotes.SumAsync(l=>l.CantidadRecibida)==50 && (await verify.OrdenesCompras.SingleAsync()).Estado==EstadosOrden.Recibida,"Exceso concurrente");
        });
        await using(var app=CVSPharmacy_Backend.Program.CrearAplicacion([
            "--ConnectionStrings:ConnectionMySql",csb.ConnectionString,
            "--Desarrollo:UsuarioId","1","--environment","Development",
            "--urls","http://127.0.0.1:0","--Logging:LogLevel:Default","Warning"]))
        {
            await app.StartAsync();
            var url=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client=new HttpClient(){BaseAddress=new Uri(url)};
            int newOrder=0;
            await Test("HTTP CU05: crear y editar Borrador, emitir; 409 al editar emitida",async()=>
            {
                var dto=new {proveedorId=1,items=new[]{new{productoId=1,cantidadSolicitada=50,precioUnitario=2m}}};
                var create=await client.PostAsJsonAsync("/api/purchase-orders",dto);
                Check(create.StatusCode==HttpStatusCode.Created,await create.Content.ReadAsStringAsync());
                using var json=JsonDocument.Parse(await create.Content.ReadAsStringAsync());newOrder=json.RootElement.GetProperty("data").GetProperty("id").GetInt32();
                var edit=await client.PutAsJsonAsync($"/api/purchase-orders/{newOrder}",dto);
                Check(edit.StatusCode==HttpStatusCode.OK,await edit.Content.ReadAsStringAsync());
                var issue=await client.PatchAsync($"/api/purchase-orders/{newOrder}/issue",null);
                Check(issue.StatusCode==HttpStatusCode.OK,await issue.Content.ReadAsStringAsync());
                var invalid=await client.PutAsJsonAsync($"/api/purchase-orders/{newOrder}",dto);
                Check(invalid.StatusCode==HttpStatusCode.Conflict,"Se editó orden emitida");
            });
            await Test("HTTP CU06: parcial, listar, 422 exceso y recepción final",async()=>
            {
                var partial=await client.PostAsJsonAsync($"/api/purchase-orders/{newOrder}/receptions",Input(20,"HTTP-A"));
                Check(partial.StatusCode==HttpStatusCode.Created,await partial.Content.ReadAsStringAsync());
                var list=await client.GetAsync($"/api/purchase-orders/{newOrder}/receptions");Check(list.IsSuccessStatusCode,"No consulta recepciones");
                var excess=await client.PostAsJsonAsync($"/api/purchase-orders/{newOrder}/receptions",Input(31,"HTTP-B"));
                Check((int)excess.StatusCode==422,"No rechaza exceso");
                var cancel=await client.PatchAsync($"/api/purchase-orders/{newOrder}/cancel",null);Check((int)cancel.StatusCode==409,"Canceló orden parcial");
                var complete=await client.PostAsJsonAsync($"/api/purchase-orders/{newOrder}/receptions",Input(30,"HTTP-C"));
                Check((int)complete.StatusCode==201,await complete.Content.ReadAsStringAsync());
            });
            await Test("HTTP: JSON mal formado/tipo incompatible 400 y permisos Operador 403",async()=>
            {
                var bad=await client.PostAsync("/api/purchase-orders",new StringContent("{\"proveedorId\":\"no-numero\"}",System.Text.Encoding.UTF8,"application/json"));
                Check((int)bad.StatusCode==400,await bad.Content.ReadAsStringAsync());
                app.Configuration["Desarrollo:UsuarioId"]="2";
                var denied=await client.GetAsync($"/api/purchase-orders/{newOrder}/receptions");Check((int)denied.StatusCode==403,"Operador consultó recepciones");
                var deniedIssue=await client.PatchAsync($"/api/purchase-orders/{newOrder}/issue",null);Check((int)deniedIssue.StatusCode==403,"Operador emitió");
                app.Configuration["Desarrollo:UsuarioId"]="1";
            });
            await Test("HTTP CU03/CU04: CRUD, validación y duplicados",async()=>
            {
                var supplier=new{nit="HTTP-PROV",razonSocial="Proveedor HTTP",email="test@example.com"};
                var created=await client.PostAsJsonAsync("/api/suppliers",supplier);Check((int)created.StatusCode==201,await created.Content.ReadAsStringAsync());
                var dup=await client.PostAsJsonAsync("/api/suppliers",supplier);Check((int)dup.StatusCode==409,"NIT duplicado aceptado");
                var product=new{codigo="HTTP-PROD",nombre="Producto HTTP",presentacion="Caja",unidadMedida="Unidad",precioVenta=2m,stockMinimo=10};
                var p=await client.PostAsJsonAsync("/api/products",product);Check((int)p.StatusCode==201,await p.Content.ReadAsStringAsync());
                var pd=await client.PostAsJsonAsync("/api/products",product);Check((int)pd.StatusCode==409,"Código duplicado aceptado");
                using var json=JsonDocument.Parse(await p.Content.ReadAsStringAsync());var id=json.RootElement.GetProperty("data").GetProperty("id").GetInt32();
                Check((int)(await client.PutAsJsonAsync($"/api/products/{id}",product)).StatusCode==200,"No actualiza producto");
                Check((int)(await client.GetAsync("/api/products/low-stock")).StatusCode==200,"No consulta stock");
                Check((int)(await client.PostAsJsonAsync("/api/products",new{codigo="SINPRECIO",nombre="X"})).StatusCode==400,"Faltantes aceptados");
            });
            await app.StopAsync();
        }
        await Test("MYSQL: colisión real de índice revierte recepción y estado",async()=>
        {
            int id;
            using(var db=new FarmaciaDbContext(Options()))
            {
                var order=new OrdenesCompra{Numero="OC-ROLLBACK",ProveedorId=1,CreadoPor=1,Estado=EstadosOrden.Emitida,
                    FechaCreacion=DateTime.Now,DetalleOrdenCompras=[new(){ProductoId=1,CantidadSolicitada=50,PrecioUnitario=1}]};
                db.Add(order);await db.SaveChangesAsync();id=order.Id;
            }
            var dto=Input(10,"Á-PRUEBA");dto.Items.Add(Input(10,"A-PRUEBA").Items[0]);
            using(var db=new FarmaciaDbContext(Options()))await Expected(409,()=>Service(db).RegistrarAsync(id,dto));
            using var verify=new FarmaciaDbContext(Options());
            Check(!await verify.Recepciones.AnyAsync(r=>r.OrdenId==id) && (await verify.OrdenesCompras.FindAsync(id))!.Estado==EstadosOrden.Emitida,"Rollback real incompleto");
        });
        await Test("MYSQL CU05: correlativo numérico después de 9999",async()=>
        {
            using var db=new FarmaciaDbContext(Options());
            db.Add(new OrdenesCompra{Numero=$"OC-{DateTime.Today.Year}-9999",ProveedorId=1,CreadoPor=1,Estado=EstadosOrden.Borrador});
            db.Add(new OrdenesCompra{Numero=$"OC-{DateTime.Today.Year}-10000",ProveedorId=1,CreadoPor=1,Estado=EstadosOrden.Borrador});await db.SaveChangesAsync();
            var service=new OrdenCompraService(new BaseRepository<OrdenesCompra>(db),new BaseRepository<DetalleOrdenCompra>(db),new BaseRepository<Proveedor>(db),new BaseRepository<Producto>(db),new BaseRepository<Recepcion>(db),new UnidadTrabajo(db),new Actor(),mapper,new());
            var result=await service.CrearOrdenAsync(new(){ProveedorId=1,Items=[new(){ProductoId=1,CantidadSolicitada=1,PrecioUnitario=1}]});
            Check(result.Numero==$"OC-{DateTime.Today.Year}-10001","Numeración lexicográfica incorrecta");
        });
        await using(var production=CVSPharmacy_Backend.Program.CrearAplicacion([
            "--ConnectionStrings:ConnectionMySql",csb.ConnectionString,
            "--Desarrollo:UsuarioId","1","--environment","Production",
            "--urls","http://127.0.0.1:0","--Logging:LogLevel:Default","Critical"]))
        {
            await production.StartAsync();
            await Test("HTTP Producción: el usuario de Development no permite acceso",async()=>
            {
                var address=production.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                using var client=new HttpClient(){BaseAddress=new Uri(address)};
                Check((int)(await client.GetAsync("/api/suppliers")).StatusCode==401,"Bypass de Development en producción");
            });
            await production.StopAsync();
        }
        Console.WriteLine($"RESULTADO MYSQL: {passed} pruebas totales completadas.");
    }
    finally
    {
        if(!testDb.StartsWith("cvs_test_",StringComparison.Ordinal) || testDb.Length!=41) throw new Exception("Nombre de limpieza no autorizado");
        using var db=new FarmaciaDbContext(Options());await db.Database.EnsureDeletedAsync();
        Console.WriteLine("Base desechable de pruebas eliminada; base del proyecto intacta.");
    }
}

sealed class Actor : IUsuarioActual
{
    public string Role="Administrador";
    public Task<UsuarioOperacion> ObtenerAsync(bool soloAdministrador=false)
    {
        if(soloAdministrador && Role!="Administrador") throw new BusinessException("Solo Administrador",HttpStatusCode.Forbidden);
        return Task.FromResult(new UsuarioOperacion(1,Role));
    }
}
sealed class Memory
{
    private readonly Dictionary<Type,object> _tables=new();
    public bool FailSave;
    public List<T> All<T>() where T:BaseEntity
    {
        if(!_tables.TryGetValue(typeof(T),out var list))_tables[typeof(T)]=list=new List<T>();
        return (List<T>)list;
    }
}
sealed class Repo<T>(Memory db):IBaseRepository<T> where T:BaseEntity
{
    public Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T,bool>>? predicate=null,params Expression<Func<T,object>>[] includes)=>
        Task.FromResult<IReadOnlyList<T>>(predicate is null?db.All<T>().ToList():db.All<T>().Where(predicate.Compile()).ToList());
    public Task<T?> GetByIdAsync(int id,params Expression<Func<T,object>>[] includes)=>Task.FromResult(db.All<T>().FirstOrDefault(e=>e.Id==id));
    public Task<T?> GetForUpdateAsync(int id)=>GetByIdAsync(id);
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T,bool>> predicate)=>Task.FromResult(db.All<T>().FirstOrDefault(predicate.Compile()));
    public Task<bool> AnyAsync(Expression<Func<T,bool>> predicate)=>Task.FromResult(db.All<T>().Any(predicate.Compile()));
    public Task AddAsync(T entity)
    {
        entity.Id=db.All<T>().Select(e=>e.Id).DefaultIfEmpty().Max()+1;db.All<T>().Add(entity);
        if(entity is Recepcion r)foreach(var l in r.Lotes){l.Id=db.All<Lote>().Count+1;l.RecepcionId=r.Id;db.All<Lote>().Add(l);}
        return Task.CompletedTask;
    }
    public Task UpdateAsync(T entity)=>Task.CompletedTask;
    public Task DeleteAsync(int id)=>throw new NotSupportedException();
    public void RemoveRange(IEnumerable<T> entities){foreach(var e in entities.ToList())db.All<T>().Remove(e);}
    public Task SaveAsync(){if(db.FailSave)throw new DuplicateKeyException(new Exception("Fallo simulado"));return Task.CompletedTask;}
}
sealed class Unit(Memory db):IUnidadTrabajo
{
    public Task<OrdenesCompra?> BloquearOrdenAsync(int id)=>Task.FromResult(db.All<OrdenesCompra>().FirstOrDefault(o=>o.Id==id));
    public async Task<T> EjecutarAsync<T>(Func<Task<T>> operation)
    {
        var states=db.All<OrdenesCompra>().ToDictionary(o=>o.Id,o=>o.Estado);
        var receipts=db.All<Recepcion>().ToList();var lots=db.All<Lote>().ToList();
        try{return await operation();}
        catch
        {
            foreach(var o in db.All<OrdenesCompra>())o.Estado=states[o.Id];
            db.All<Recepcion>().Clear();db.All<Recepcion>().AddRange(receipts);
            db.All<Lote>().Clear();db.All<Lote>().AddRange(lots);throw;
        }
    }
}
sealed class Fixture
{
    public Memory Db=new();public Actor Actor=new();public OrdenesCompra Order;
    public RecepcionService Service;
    public Fixture(IMapper mapper)
    {
        Order=new(){Id=1,Numero="OC-TEST",Estado=EstadosOrden.Emitida};Db.All<OrdenesCompra>().Add(Order);
        Db.All<DetalleOrdenCompra>().Add(new(){Id=1,OrdenId=1,ProductoId=1,CantidadSolicitada=50});
        Service=new(new Repo<OrdenesCompra>(Db),new Repo<DetalleOrdenCompra>(Db),new Repo<Recepcion>(Db),new Repo<Lote>(Db),new Unit(Db),Actor,mapper,new(),TimeProvider.System);
    }
}
