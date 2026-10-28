# Proyecto Aerolínea VIP — Primera Entrega

Sistema web monolítico ASP.NET MVC 5 (.NET Framework 4.8.1) para compra, reserva y administración de pasajes de una aerolínea VIP (3 Embraer Phenom 300E, 7 asientos por vuelo).

- Autores del proyecto: **Jeremy Alexander Vilaña Guamán y Amaro Benjamin Caiza Sandoval**.
- Fecha de entrega: miércoles 28 de octubre de 2026.
- Solución: `AerolineaVIP.slnx` (formato VS 2026) · Proyecto: `AerolineaVIP\AerolineaVIP.csproj` (csproj clásico: todo archivo nuevo debe agregarse al csproj).
- Script de base de datos: `BaseDatos\Script_BD_AerolineaVIP.sql` (crea `AerolineaVIP` y `AerolineaVIP_Identity`) y luego `AerolineaVIP\Scripts\Ampliar_AspNetUsers.sql`.
- Compilar (incluye vistas Razor): `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" AerolineaVIP.slnx -t:Build -p:Configuration=Debug -p:MvcBuildViews=true -v:m`
  (Web.config quita el buildProvider `.edmx` para que la precompilación de vistas funcione; no afecta al EDMX ni a sus recursos csdl/ssdl/msl.)

## Documentos oficiales del docente (no están en el repositorio)

| Archivo | Uso |
|---|---|
| ENUNCIADO PROYECTO.pdf | Documento oficial. Clave: 11.4 (UX, mapa de navegación 11.4.2, 21 pantallas 11.4.5), 15 (reglas de negocio), 16 (roles), 18 (decisiones pendientes). No se modifica. |
| Wireframes_21_Interfaces_Aerolinea_VIP.pdf | Esqueleto funcional de las 21 pantallas, rutas sugeridas y semántica de asientos. |
| Guia_Registro_Decisiones_Pendientes_18_1.pdf | Cómo redactar las políticas DP-01 a DP-15 (PDF de documentación, no el ZIP). |
| Instrucciones_Primera_Entrega.pdf | Qué se entrega. Sección 4: los 12 entregables del ZIP. |

## REGLAS OBLIGATORIAS DEL DOCENTE

1. **Autores**: Jeremy Alexander Vilaña Guamán y Amaro Benjamin Caiza Sandoval. El pie de página muestra ambos nombres.
2. **Vistas Razor**: primero `@using Namespace` y en la línea siguiente `@model Tipo`. NUNCA `@model Namespace.Completo.Tipo` en una sola línea.
   ```cshtml
   @using AerolineaVIP.Models
   @model LoginViewModel
   ```
3. **Sin lógica en las vistas**: ningún cálculo ni lógica de negocio en `.cshtml`. La lógica va en el Modelo, en clases parciales en archivos aparte con el patrón `<Tabla>_Calculos.cs` (ej. `Models/Vuelo_Calculos.cs` → `public partial class Vuelo { ... }`).
4. **Validaciones**: DataAnnotations (en clases de metadatos parciales `[MetadataType]` para las entidades del EDMX, en `Models/Metadatos/<Tabla>_Metadatos.cs`), jQuery Validation unobtrusive en el cliente (`~/bundles/jqueryval`) y `ModelState.IsValid` en el servidor.
5. **Seguridad**: `[ValidateAntiForgeryToken]` en todo POST (y `@Html.AntiForgeryToken()` en el formulario). Autorización con `[Authorize(Roles = "...")]` en controladores/acciones; nunca solo ocultando botones.
6. **Alcance**: sin dashboard (la pantalla 17 es solo una página de accesos), sin auditoría, sin bloqueos de concurrencia (la selección de asientos es temporal y el servidor revalida), pagos solo simulados.
7. **EDMX**: no modificar `Models/AerolineaVIPModel.edmx` a mano. Si hace falta un dato, usar las vistas existentes: `vw_VueloDetalle`, `vw_EstadoAsientoVuelo`, `vw_DisponibilidadVuelo`, `vw_AsientoAeronave`, `vw_ComprobanteDetalle`. Los procedimientos almacenados se llaman con `db.Database.ExecuteSqlCommand`.
8. **Estilo de código**: en español, simple y legible, nivel de estudiante de sexto semestre. Sin comentarios excesivos.
9. **Web.config**: para la entrega ambas cadenas usan `Data Source=.;User ID=sa;Password=123;TrustServerCertificate=True`, conforme a la configuración académica autorizada por el grupo.
   - `DefaultConnection` → `AerolineaVIP_Identity`
   - `AerolineaVIPEntities` → `AerolineaVIP`

## Datos técnicos del modelo (EDMX)

- Contexto de dominio: `AerolineaVIPEntities` (namespace `AerolineaVIP.Models`). Contexto de Identity: `ApplicationDbContext` (inicializador desactivado: las tablas ya existen).
- El pluralizador generó nombres raros; usar los reales:
  `Aeronaves, Aeropuertoes, Asientoes, CheckIns, Compras, Comprobantes, DatosEmisors, EstadoAeronaves, EstadoAsignacions, EstadoCheckIns, EstadoCompras, EstadoComprobantes, EstadoItinerarios, EstadoPagoes, EstadoReservas, EstadoVueloes, Frecuencias, Itinerarios, MarcaTarjetas, MetodoPagoes, ModeloAeronaves, Monedas, Pagoes, Pasajeroes, Reservas, ReservaPasajeroVueloes, Rutas, TipoComprobantes, TipoDocumentoIdentidads, TramoItinerarios, Vueloes`, y las vistas `vw_*`.
  Navegaciones: `Vuelo.ReservaPasajeroVueloes`, `Aeronave.Vueloes`, `Compra.Pagoes`, etc.
- **Ruta**: `Ruta.Aeropuerto1` es el ORIGEN y `Ruta.Aeropuerto` el DESTINO. Usar `Ruta.AeropuertoOrigen` / `Ruta.AeropuertoDestino` (definidos en `Ruta_Calculos.cs`).
- `Reserva.UsuarioId` = `AspNetUsers.Id` (otra base, sin FK física: validar en la aplicación).
- Códigos de estado (claves naturales VARCHAR):
  - Vuelo: PROGRAMADO, CONFIRMADO, CANCELADO, COMPLETADO · Aeronave: DISPONIBLE, NO_DISPONIBLE · Itinerario: ACTIVO, INACTIVO
  - Reserva: PENDIENTE, CONFIRMADA, CANCELADA, EXPIRADA
  - Asignación (ReservaPasajeroVuelo): PENDIENTE (morado, reservado), CONFIRMADA (rojo, ocupado), CANCELADA y LIBERADA (verde, disponible)
  - Compra: PENDIENTE, PAGADA, ANULADA · Pago: PENDIENTE, APROBADO, RECHAZADO, REVERSADO · CheckIn: COMPLETADO, ANULADO
  - Comprobante: tipos CONFIRMACION, FACTURA, NOTA_CREDITO; estados EMITIDO, ANULADO
  - Documento: CEDULA, PASAPORTE · MetodoPago: TARJETA_CREDITO, TARJETA_DEBITO · Marcas: VISA, MASTERCARD, AMEX, DINERS
- Frecuencia.DiaSemana: 1 = lunes … 7 = domingo.
- La BD protege con CHECK, índices únicos filtrados y triggers (`trg_Vuelo_Validar`, `trg_ReservaPasajeroVuelo_Validar`, `trg_TramoItinerario_Validar`, `trg_CheckIn_Validar`, `trg_Pago_Validar`). La aplicación repite las validaciones y muestra los mensajes con `ErroresBaseDatos.ObtenerMensaje(ex)` (nunca pantalla de error).
- Asientos: verde = Disponible, morado = Reservado, rojo = Ocupado; los colores salen de `vw_EstadoAsientoVuelo.ColorEstado`, no fijos en el HTML. Códigos 1D, 2D, 2I, 3D, 3I, 4D, 4I (no existe 1I).

## Roles y usuarios iniciales (creados por código en Global.asax.cs)

Roles: `SuperUsuario`, `Administrador`, `EmpleadoCounter`, `Cliente` (constantes en `Models/RolesSistema.cs`, con combinaciones `AdminYSuper` y `ClienteCounterYSuper`).

| Usuario | Rol | Contraseña |
|---|---|---|
| superusuario | SuperUsuario | Super#2026 |
| admin01 | Administrador | Admin#2026 |
| counter01 | EmpleadoCounter | Counter#2026 |
| cliente01 | Cliente | Cliente#2026 |

Política de contraseñas: mínimo 8, mayúscula, minúscula, número y símbolo. Login con **nombre de usuario**. Registro público siempre crea `Cliente`; solo un SuperUsuario autenticado elige otro rol (validado en el servidor).

## Estado del proyecto

**Complemento de FASE 4 (DP-13):** Asistencia exclusiva de EmpleadoCounter, con búsqueda exacta por código o documento, gestión/pago/check-in de los resultados y nueva compra para una cuenta Cliente existente por usuario o correo. La cuenta cliente se conserva en Session asociada al operador y se revalida antes de reservar. El acceso a reservas asistidas y sus comprobantes se limita a los resultados de la búsqueda actual o a la compra que tramita ese operador; los IDs no encontrados se rechazan. El counter solo cancela pendientes vigentes y no accede a Admin. Los filtros de Mis reservas usan fecha de viaje del primer tramo. Los documentos se presentan como «Cédula»/«Pasaporte» y el vencimiento solo se muestra en PENDIENTE. Estas correcciones se incluyen en la prueba de integración de FASE 4.

**FASE 4 solicitada (pantallas 13–16):** implementadas Mis reservas (usuario autenticado, filtros y paginación), gestión y cancelación completa DP-08, check-in DP-12 y comprobantes con vista previa y PDF del servidor. El grupo probó el flujo; se agregaron las correcciones del complemento anterior. Las cancelaciones y reembolsos se guardan en una transacción: la compra conserva el importe original; al reversar, `Pago.Importe` representa el importe efectivamente reembolsado y `MensajeResultado` conserva el porcentaje y ambos importes. La NOTA_CREDITO lleva numeración `NC-xxxxxx` y usa ese importe reversado. No hay cambios de esquema ni del EDMX.

Comprobantes: se consultan desde `vw_ComprobanteDetalle`, con propiedad validada en servidor. Para PDF se usa el paquete NuGet PDFsharp 1.50.5147 (MIT, compatible con .NET Framework); generación local, sin servicios externos. Leyenda fija: «Comprobante académico de una operación simulada.». Referencia del paquete: https://www.nuget.org/packages/PDFsharp/1.50.5147.

Verificación FASE 4: `Pruebas/AreaCliente.Integracion.ps1 -BaseUrl http://localhost:59817`, con la aplicación iniciada. Cubre propiedad, cancelación pendiente, 80 % y 100 %, reversión completa ante fallo, nota de crédito y PDF, límites horarios de check-in, reenvíos y anulación por counter. Crea y elimina únicamente datos temporales; se conserva también la prueba de FASE 3.

**Actualización de trabajo:** las fases 1 y 2 y el flujo funcional de la FASE 3 fueron probados por el grupo en la máquina local. La FASE 3 solicitada agrupa las pantallas 4–12 (fases 3–5 originales y recuperación de acceso). Incluye DP-03 a DP-07 y DP-09 a DP-11; las políticas siguen siendo propuestas, no aprobaciones del docente. Se agregaron los ajustes de presentación solicitados: pasos activos, mapa adaptable, resúmenes, condiciones de reserva, dos destinos de confirmación, selección de marca por logos, vencimiento y código de seguridad solo para validación, numeración CNF/ACAD y confirmación con consulta de reserva y comprobantes. La tabla siguiente conserva la numeración original como referencia del plan previo.

Implementado en esta FASE 3: resultados y disponibilidad mediante las vistas SQL, mapa de siete asientos, selección por tramo en Session, validación de pasajeros, reserva y asignaciones en una transacción, pago simulado y comprobantes CONFIRMACION/FACTURA. Recuperación genera un token sin enviar correo y mantiene una respuesta neutral. Pantalla 18 valida preparación y continuidad geográfica al asignar aeronave, con `MinutosPreparacion=60`. No se requieren scripts SQL adicionales.

Verificación reproducible: `Pruebas/FlujoCompra.Integracion.ps1 -BaseUrl http://localhost:59817` con la aplicación en ejecución. Lee las cadenas locales de Web.config, usa las cuentas académicas iniciales, crea registros temporales y los elimina al finalizar. Cubre recuperación, tarifas, condiciones, datos opcionales, validaciones de tarjeta, reenvíos, pagos, numeración de comprobantes, expiración y venta dominical. El grupo autorizó expresamente incluir Web.config con `Data Source=.` y la cuenta académica local `sa/123` en el commit de FASE 3; no autorizó push.

| Fase solicitada | Contenido | Estado |
|---|---|---|
| 1 | Identity, login, registro, roles y usuarios iniciales | Terminada y probada por el grupo |
| 2 | Layout, navegación por rol y administración | Terminada y probada por el grupo |
| 3 | Pantallas 4–12, compra y políticas DP solicitadas | Terminada y probada por el grupo |
| 4 | Pantallas 13–16 y Asistencia del counter | Terminada y probada por el grupo |
| 5 | Revisión final, auditoría Razor, compilación, pruebas y fuentes para entrega | Revisión realizada; ver Documentacion/Revision_Final.md |
### Checklist de los 12 entregables del ZIP
1. Home/Index con diseño propio — completo.
2. Superusuario creado por código — completo.
3. Administrador creado por código con acceso limitado — completo.
4. Roles creados por código — completo.
5. Usuarios iniciales creados por código — completo.
6. Roles asignados por código — completo.
7. CRUD de las tablas administrables y catálogos de solo lectura — completo.
8. Login con nombre de usuario y contraseña — completo.
9. Registro con usuario, correo, contraseña y rol protegido en servidor — completo.
10. Las 21 interfaces — implementadas según el resumen disponible; ver tabla de rutas reales en Documentacion/Revision_Final.md.
11. Mapa de navegación por roles — revisado; corregidos retorno desde login y accesos de cliente del SuperUsuario.
12. Integración de base de datos y funcionalidades — flujos integrados y probados; DP-02 conserva asignación manual por el Administrador, sin automatización del plan propuesto.

Los PDF oficiales no están en el repositorio: no se afirma una comparación literal con sus wireframes. El informe final incluye evidencia y pendientes por entregable.
## Decisiones pendientes DP-01 a DP-15 (apartado 18.1)

Las políticas están redactadas en el PDF de documentación del grupo. Son **propuestas del grupo, pendientes de aprobación**: ninguna se presenta como aprobada. El código debe seguirlas; la última columna indica qué falta implementar.

| DP | Política propuesta del grupo | Estado en el código |
|---|---|---|
| DP-01 Aeropuerto de Galápagos | Galápagos = aeropuerto GPS, editable en la tabla Aeropuerto. | Hecho (CRUD Aeropuertos). |
| DP-02 Asignación de los jets | AV-01: Quito–Guayaquil 09:00 y 14:00 + ruta dominical. AV-02: Quito–Manta 09:00 y 14:00. AV-03: vuelos de 19:00 y reserva. | El administrador asigna en la pantalla 18; falta aplicar el plan a los vuelos generados. |
| DP-03 Tiempos de preparación | 60 min de preparación entre la llegada y la siguiente salida de la misma aeronave, que debe salir del aeropuerto de llegada (`MinutosPreparacion=60` en Web.config). | Implementado: MinutosPreparacion=60 y validación de preparación y continuidad en pantalla 18. |
| DP-04 Tarifas y moneda | USD; tarifa por pasajero y tramo desde `Ruta.TarifaReferencial`, copiada a `ImporteTramo` al confirmar; el importe se calcula en el servidor. | Implementado: tarifas en servidor e ImporteTramo persistido al reservar. |
| DP-05 Impuestos y comprobantes | Comprobantes académicos CONFIRMACION, FACTURA (`ACAD-xxxxxx`) y NOTA_CREDITO; sin impuestos. | Implementado: CNF, ACAD y NC con importes de operación o reembolso; sin impuestos. |
| DP-06 Proveedor de pagos | Pago simulado; se rechaza si la tarjeta termina en 0000; se guarda solo marca, últimos 4 dígitos, titular y el código `SIM-<CodigoReserva>-<intento>`. | Implementado: simulación, rechazo 0000, datos mínimos y código SIM con intento de dos dígitos. |
| DP-07 Duración de reserva pendiente | Retención = la menor entre creación + 24 h y salida del primer tramo − 2 h. Las vencidas pasan a EXPIRADA y sus asientos a LIBERADA al consultar. | Implementado: retención y expiración al consultar con liberación de asientos. |
| DP-08 Cancelaciones y reembolsos | Pendiente: se cancela sin costo. Pagada: hasta 24 h antes, con reembolso simulado del 80 % (compra ANULADA, pago REVERSADO, nota de crédito). Dentro de las 24 h no se cancela por web. Vuelo cancelado por la aerolínea: reembolso del 100 % por el administrador. Siempre se cancela la reserva completa. | Implementado: cancelación completa, reversión 80 % o 100 % y nota de crédito en una transacción. |
| DP-09 Venta por tramos | En domingo solo se venden el itinerario de ida (tramos 1–2) y el de regreso (tramos 3–4), sin tramos sueltos. | Implementado: domingo solo pares de tramos 1–2 o 3–4 de itinerarios activos. |
| DP-10 Conexión en Guayaquil | Conexión en GYE de mínimo 60 min y con la misma aeronave en ambos tramos. | Implementado: conexión en GYE de 60 minutos como mínimo y misma aeronave. |
| DP-11 Configuración de asientos | Asientos 1D, 2D, 2I, 3D, 3I, 4D, 4I. | Hecho (tabla Asiento y vista `vw_AsientoAeronave`). |
| DP-12 Política de check-in | Abre 24 h y cierra 1 h antes de cada tramo; solo asientos CONFIRMADA; se busca por código de reserva + documento; el counter puede anular un check-in. | Implementado: ventana 24 h–1 h, asiento confirmado, unicidad y anulación por counter. |
| DP-13 Funciones del counter | El counter busca reservas, reserva y paga en nombre de un cliente con cuenta (se guarda el `UsuarioId` del cliente), registra pasajeros, hace check-in, emite comprobantes y cancela reservas pendientes. No administra vuelos, aeronaves, tarifas, usuarios ni roles, ni cancela reservas pagadas. | Implementado: Asistencia, compra para Cliente, pago y check-in; counter solo cancela pendientes y no entra a Admin. |
| DP-14 Recuperación y disponibilidad | Fuera del alcance de la primera entrega. | — |
| DP-15 Normativa aplicable | Fuera del alcance de la primera entrega; no se afirma cumplimiento normativo. | — |

## Convenciones implementadas

- Área MVC `Areas/Admin` (rutas `/Admin/Index`, `/Admin/Vuelos`, `/Admin/Aeronaves`, `/Admin/Gestion`, `/Admin/Reportes`). Todos sus controladores llevan `[Authorize(Roles = RolesSistema.AdminYSuper)]` y heredan de `ControladorAdminBase` (`GuardarCambios()` convierte errores SQL en mensajes con `ErroresBaseDatos`).
- La ruta raíz restringe namespaces a `AerolineaVIP.Controllers` (hay un `VuelosController` público y otro en el área). En el layout y en vistas del área, los enlaces a controladores raíz llevan `area = ""`.
- Un usuario autenticado sin el rol requerido ve `Views/Shared/AccesoDenegado.cshtml` (lo resuelve `AccountController.Login` cuando llega con ReturnUrl).
- Cálculos en `Models/<Tabla>_Calculos.cs`; consultas de reportes en `Models/Reportes.cs`; listas del buscador en `Models/BusquedaVuelos.cs`; validadores propios `DebeSerVerdaderoAttribute` y `FechaNoAnteriorAHoyAttribute` (con su regla jQuery en `Scripts/validaciones-personalizadas.js`).
- Los view models que se enlazan por GET deben inicializar sus listas en el constructor (el binder evalúa las propiedades calculadas al validar).
- Web.config: `<globalization culture="en-US" uiCulture="es-EC" fileEncoding="utf-8" .../>` para que decimales y fechas se enlacen con punto; las fechas se muestran como dd/MM/yyyy.
- Vistas Razor guardadas en UTF-8 con BOM.

## Forma de trabajo

- Trabajar fase por fase y detenerse al final de cada una para que Jeremy pruebe.
- Compilar con MSBuild hasta 0 errores antes de terminar una fase.
- Si hace falta un cambio de base de datos, crear un script aparte en `AerolineaVIP\Scripts\` y pedir que se ejecute en SSMS.
