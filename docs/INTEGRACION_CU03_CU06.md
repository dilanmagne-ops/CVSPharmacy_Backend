# Integración de CU-03, CU-04, CU-05 y CU-06

Fecha: 30 de octubre de 2026.

## Qué se integró

- De `main` (`149b88e`): `BusinessException`, `ErrorResponse`, middleware global y validación de campos obligatorios de Productos.
- De `CASO6_SEGUNESTRUCTURADOCENTE` (`b194515`): repositorio genérico utilizado directamente, `BaseEntity` y carpetas `Services/Interfaces`, `Services/Services`, `Services/Validators`.
- De `feature/cu-05-ordenes-compra` (`75cdd5d`): DTOs, perfil y reglas de órdenes. El servicio y controlador se adaptaron a las interfaces genéricas y al middleware; no se copiaron los repositorios específicos ni `ResultadoServicio`.
- Nuevo CU-06: DTOs, validadores, servicio, perfil y controlador de recepciones; confirmación transaccional y bloqueo de orden con `SELECT ... FOR UPDATE`.

La integración se aplica como cambios de trabajo en main. No crea un merge commit, no hace push y no modifica las otras ramas.

## Estructura de responsabilidades

`API/Controllers` recibe HTTP y devuelve DTOs. `Services/Interfaces` define operaciones. `Services/Services` valida y coordina reglas. `Core/Interfaces/IBaseRepository` describe acceso genérico; `Core/Interfaces/IUnidadTrabajo` describe la transacción. `Infrastructure/Repositories/BaseRepository` implementa EF. `Infrastructure/Data/UnidadTrabajo` implementa la transacción MySQL y el bloqueo de orden. Los perfiles permanecen en Infrastructure/Mappings y se descubren por ensamblado.

Se retiraron `ResultadoServicio`, los repositorios específicos de Productos/Proveedores y sus interfaces. Los errores esperados se expresan como `BusinessException` o `ValidationException`. El middleware produce 400, 401, 403, 404, 409 o 422; no expone los detalles de fallos internos.

## Conexión local

La configuración previa de main fue rechazada por MySQL. Se conservó el resto de appsettings y se utilizó únicamente la conexión de CASO6, comprobada en el servidor local. No se imprimieron contraseñas. La configuración contiene secretos locales: no publiques el archivo ni el respaldo.

## Usuario de pruebas y permisos

CU-01/login no está implementado en las ramas recibidas. No se inventó una sesión ni un header de rol.

En Development existe una opción explícita `Desarrollo:UsuarioId` en `CVSPharmacy_Backend.Api/appsettings.Development.json`. La API consulta ese usuario y su rol real en MySQL. Para emitir, cancelar, recibir o consultar recepciones exige Administrador; Operador recibe 403. Productos y Proveedores exigen usuario activo; cambiar estado exige Administrador.

En la consulta de preparación no se encontró Administrador activo en la base local. Antes de las pruebas manuales debes crear uno o utilizar el siguiente ayudante, que es **opcional y sí agrega un rol/usuario de desarrollo si no existe**. No se ejecutó automáticamente sobre tu base:

```powershell
dotnet run --project Tests/CVSPharmacy.Verificacion -- --prepare-development CVSPharmacy_Backend.Api/appsettings.json
```

El comando solo admite un servidor local. Imprime el Id que debes poner en `Desarrollo:UsuarioId`. No reutiliza ni cambia usuarios inactivos existentes. Guarda un hash de una contraseña aleatoria no compartida; este usuario técnico no implementa el login del futuro CU-01.

También puedes comprobar si ya hay un Administrador activo sin modificar nada:

```powershell
dotnet run --project Tests/CVSPharmacy.Verificacion -- --check-user CVSPharmacy_Backend.Api/appsettings.json
```

El modo Development nunca sirve como sesión en Production: las solicitudes sin identidad autenticada reciben 401. Está probado. Para desplegar hace falta completar CU-01 y configurar un esquema de autenticación real.

## Iniciar API

Desde la raíz de la solución, con MySQL disponible:

```powershell
dotnet build CVSPharmacy_Backend.slnx
dotnet run --project CVSPharmacy_Backend.Api --launch-profile https
```

El perfil https declara `https://localhost:7257` y `http://localhost:5140`. Usa las direcciones que realmente muestre tu terminal. Si Visual Studio conserva una compilación anterior, detén la ejecución, recarga la solución y vuelve a compilar. No hay cambios de esquema requeridos por BaseEntity: las tablas siguen independientes. Los índices únicos deben existir como en tus configuraciones previas.

## Probar en Postman

Importa `docs/CVSPharmacy_CU03_CU06.postman_collection.json`. Configura `base_url` con la dirección de tu API y ejecuta los 18 requests en orden. La colección crea un producto y proveedor con identificadores únicos por ejecución, una orden de 50 unidades y captura automáticamente los Ids. Los ejemplos crean datos de práctica en la base seleccionada; no se borran automáticamente.

1. Crear producto y proveedor (201).
2. Crear orden Borrador, editarla y emitirla (201/200).
3. Intentar editar Emitida (409).
4. Recibir 20 unidades (201): orden Recibida parcial y stock 20.
5. Recibir 31 adicionales (422): no se agrega recepción ni se cambia stock.
6. Repetir el lote (409) o enviar vencimiento pasado (422): sin cambios parciales.
7. Intentar cancelar una orden parcial (409).
8. Recibir las 30 restantes (201): orden Recibida y stock 50.
9. Intentar una recepción posterior (409).

El ayudante y el modo de pruebas no crean productos, proveedores ni órdenes en la base original automáticamente; la colección sí lo hace cuando tú la ejecutas.

## Contrato CU-06

```http
POST /api/purchase-orders/{id}/receptions
Content-Type: application/json
```

```json
{
  "nroRemision": "REM-PRACTICA-01",
  "observaciones": "Primera entrega",
  "items": [
    {
      "productoId": 1,
      "cantidad": 20,
      "numeroLote": "LOTE-PRACTICA-A",
      "fechaVencimiento": "2028-12-31"
    }
  ]
}
```

El Id de producto debe pertenecer a esa orden y la fecha debe ser posterior al día actual del servidor. La cantidad es un entero positivo. No se acepta `recibidoPor` desde el cuerpo: se obtiene del usuario actual. Se permiten varios lotes distintos del mismo producto y se suma su cantidad antes de compararla con lo pendiente.

```http
GET /api/purchase-orders/{id}/receptions
```

Ambas operaciones requieren Administrador. La respuesta usa `ApiResponse`, incluyendo Id, orden, responsable, fecha, remisión, observaciones y lotes. El estado actualizado se consulta con GET de la orden.

## Reglas de recepción implementadas

- 404: orden inexistente.
- 409: orden en Borrador, Cancelada o Recibida; lote duplicado para un producto, incluso dentro del mismo cuerpo.
- 422: producto ajeno a la orden, cantidad que supera lo pendiente o vencimiento que no es posterior a hoy.
- 400: campos obligatorios, cantidades no positivas, longitudes, JSON mal formado o tipos incompatibles.
- Pendiente = solicitado − suma histórica de CantidadRecibida de sus lotes. Nunca se resta CantidadDisponible, que puede disminuir por salidas.
- Se validan todos los ítems antes de preparar escrituras.
- Recepción, lotes, stock derivado y estado se confirman dentro de una transacción. Al fallar se revierte todo.
- El índice único protege colisiones entre solicitudes sobre distintos pedidos. La excepción produce 409 y rollback.
- Recepción, edición, emisión y cancelación bloquean la misma orden durante la transacción; dos recepciones no pueden superar el pedido.
- Una recepción nueva crea lotes con CantidadRecibida y CantidadDisponible iguales a la cantidad recibida. No se suma a un campo de stock redundante del producto: el stock se calcula desde sus lotes.
- La desactivación posterior del proveedor no invalida una orden ya emitida.

## Cambios de CU-05

Se conserva la numeración `OC-AÑO-0001`, calculando el máximo numérico (no lexicográfico). El índice único y un reintento limitado protegen solicitudes simultáneas. Crear/editar requiere proveedor y productos activos. Un producto no puede repetirse en dos líneas de la misma orden. Se validan cantidades y precisión de precios. Solo Borrador se edita o emite. Solo Borrador o Emitida sin recepciones se cancela. Responsable y fecha provienen del usuario actual y del servidor.

## Pruebas repetibles

Sin servidor MySQL (18 escenarios):

```powershell
dotnet run --project Tests/CVSPharmacy.Verificacion
```

Con MySQL local, 28 escenarios incluyendo HTTP real, permisos y concurrencia:

```powershell
dotnet run --project Tests/CVSPharmacy.Verificacion -- --mysql CVSPharmacy_Backend.Api/appsettings.json
```

El modo `--mysql` crea una base temporal con prefijo `cvs_test_` y un identificador aleatorio. Nunca dirige las operaciones de prueba a la base del appsettings; usa sus credenciales solo para el servidor local. Elimina exclusivamente esa base de prueba al terminar. El usuario MySQL debe poder crear y eliminar esa base temporal. El modo `--prepare-development`, en cambio, sí modifica la base configurada y solo se ejecuta de manera explícita.

Verificación realizada: 28 pruebas pasaron, incluyendo creación/edición/emisión de órdenes por HTTP, parcial 20+30, rechazo de excesos, lotes duplicados, vencimientos, rollback real ante índice único, dos solicitudes simultáneas, CRUD de Productos/Proveedores, permisos de Operador y rechazo del modo Development en Production. La compilación de aplicación y verificación terminó con 0 errores y 0 advertencias del compilador. Las dependencias y sus versiones no se actualizaron; esto no afirma ausencia de avisos de seguridad de NuGet.

## Límites de esta entrega

CU-01/CU-02 (login y gestión de usuarios), CU-07 (salidas FEFO) y CU-08 (bajas) no se implementan aquí. Sus entidades existentes no son endpoints funcionales. No se hizo push ni commit. Los datos existentes de farmacia no fueron modificados por las pruebas automáticas; se comprobó el usuario de forma solo lectura.

`DeleteAsync` genérico queda bloqueado para evitar borrar historial de farmacia. Las desactivaciones se hacen con PATCH; `RemoveRange` solo se utiliza para reemplazar detalles de órdenes Borrador dentro de su transacción. La búsqueda q/active de Productos aún se filtra en memoria; no se añadió paginación. El servicio de listados de órdenes hace consultas de detalle por orden: una optimización de lectura futura no cambia el contrato implementado.
