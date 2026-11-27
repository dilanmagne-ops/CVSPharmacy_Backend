# Fase 6D · Verificación integral de autenticación y permisos

El 27 de noviembre de 2026 se verificó la API del commit `00e9c94` con 87 solicitudes HTTP y una comprobación final en MySQL. Se utilizó una instancia aislada con el esquema generado a partir del modelo EF Core y cuentas de prueba. La API se ejecutó en modo Production. La compilación Release finalizó sin errores y con advertencias.

## CU-01 y CU-02

| Prueba del documento | Resultado observado |
|---|---|
| PR-01 · Login válido | El Administrador obtuvo 200 y un JWT con su rol. Ambos roles consultaron su identidad en `/api/auth/me`. MySQL registró el último acceso de las cuatro cuentas que iniciaron sesión durante la prueba. |
| PR-02 · Contraseña incorrecta | El login devolvió 401. Un correo inexistente también obtuvo 401. |
| PR-03 · Cuenta desactivada | El login devolvió 403. Un token emitido antes de la desactivación también obtuvo 403 al consultar productos. |
| PR-04 · Alta de Operador | El Administrador obtuvo 201. El nuevo Operador estaba activo, pudo iniciar sesión con la contraseña inicial y su registro contenía un hash BCrypt. |
| PR-05 · Correo duplicado | El alta con un correo existente en otra combinación de mayúsculas y minúsculas devolvió 409. |
| PR-06 · Último Administrador | La autodesactivación y la retirada del rol al único Administrador activo devolvieron 409. Con dos administradores que intentaron degradarse al mismo tiempo, una petición obtuvo 200 y la otra 409. SQL confirmó que quedó uno activo. |

## CU-03 a CU-06

Las 19 rutas de productos, proveedores, órdenes y recepciones devolvieron 401 al invocarlas sin token. Con un token de Operador, la API devolvió 403 al cambiar el estado de productos o proveedores, emitir o cancelar órdenes y consultar o registrar recepciones.

El Operador creó un producto y un proveedor y registró una orden de tres unidades. El Administrador emitió la orden y recibió dos unidades. Un intento de registrar otras dos devolvió 422 por superar la unidad pendiente (RN-12). Tras recibir la última unidad, la orden pasó a `Recibida`.

| Verificación final en MySQL | Valor |
|---|---|
| Responsable de crear la orden | Operador, ID 2 |
| Recepciones guardadas | 2, ambas por el Administrador, ID 1 |
| Existencias generadas | 3 unidades disponibles en 2 lotes |

Las peticiones de creación incluyeron deliberadamente un ID de responsable distinto al de la sesión. Los registros guardados correspondieron a la identidad del JWT. La prueba recorrió los controladores, servicios, repositorios y base de datos mediante las rutas públicas de la API.
