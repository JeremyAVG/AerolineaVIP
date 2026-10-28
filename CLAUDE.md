# Proyecto Aerolínea VIP — Primera Entrega

Sistema web monolítico ASP.NET MVC 5 (.NET Framework 4.8.1) para compra, reserva y administración de pasajes de una aerolínea VIP (3 Embraer Phenom 300E, 7 asientos por vuelo).

- Autor del proyecto: **Jeremy Vilaña**.
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

1. **Autor**: Jeremy Vilaña. No debe aparecer ningún otro nombre de estudiante en código, vistas, pie de página ni documentos.
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
9. **Web.config**: durante el desarrollo ambas cadenas usan `Data Source=.\SQL2019;User ID=sa;Password=123;TrustServerCertificate=True`. Para entregar se cambian a `Data Source=.` (solo cuando Jeremy lo indique).
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

| Fase | Contenido | Estado |
|---|---|---|
| 1 | Identity: cadenas de conexión, inicializador nulo, ApplicationUser con Nombres/Apellidos, login por usuario, registro con rol seguro, siembra de roles y usuarios | **Terminada y probada** |
| 2 | Layout y menú por rol (mapa 11.4.2), Home (pantalla 1), Buscar (pantalla 5), redirección por rol tras el login, área Admin: pantallas 17–21 y CRUD de Aeropuerto, Ruta, Frecuencia, Aeronave, Vuelo, Itinerario, TramoItinerario y Pasajero; catálogos en solo lectura | **Terminada** |
| 3 | Pantalla 6 Resultados (vuelos e itinerario de Galápagos según la fecha) y pantalla 7 Disponibilidad con mapa de asientos. La acción `Vuelos/Resultados` ya valida los criterios y muestra una vista provisional | **Pendiente (siguiente)** |
| 4 | Flujo de reserva (Cliente/EmpleadoCounter): 8 Selección de asiento, 9 Pasajeros, 10 Confirmar reserva (revalida disponibilidad, máx. 7) | Pendiente |
| 5 | Compra y pago simulado: 11 Pagar (modal, sin datos sensibles), 12 Confirmación, comprobante | Pendiente |
| 6 | Área del cliente: 13 Mis reservas, 14 Gestionar/cancelar, 15 Check-in, 16 Comprobantes. Hoy `Reservas`, `CheckIn` y `Comprobantes` son controladores provisionales con `Views/Shared/EnConstruccion.cshtml` | Pendiente |
| 7 | Pantalla 4 (recuperación de acceso con mensaje neutral) y área de counter (Asistencia) | Pendiente |
| 8 | Cierre: revisión de reglas, compilación 0 errores, `Data Source=.` (cuando Jeremy lo indique), ZIP sin bin/obj/packages con el script SQL | Pendiente |

### Checklist de los 12 entregables del ZIP
1. Home/Index con diseño propio — hecho.
2. Superusuario creado por código — hecho.
3. Administrador creado por código con acceso limitado a sus CRUD — hecho.
4. Roles creados por código en Global.asax.cs — hecho.
5. Usuarios iniciales creados por código — hecho.
6. Roles asignados a los usuarios iniciales por código — hecho.
7. CRUD con scaffolding para las tablas administrables — hecho.
8. Login con nombre de usuario y contraseña — hecho.
9. Registro con nombre de usuario, correo, contraseña y selección de rol — hecho.
10. Las 21 interfaces del 11.4.5 — hechas 1, 2, 3, 5, 17, 18, 19, 20, 21; faltan 4 y 6–16.
11. Mapa de navegación del 11.4.2 — menú listo; faltan las pantallas de cliente y counter.
12. Integración de la BD, datos iniciales y funcionalidades del alcance — parcial.

## Decisiones pendientes DP-01 a DP-15 (apartado 18.1)

Las políticas del grupo **todavía no están redactadas**; van en el PDF de documentación según la guía del docente, y ninguna se presenta como aprobada. Mientras tanto el código usa estos **supuestos**, todos pendientes de aprobación:

| DP | Supuesto usado en el código |
|---|---|
| DP-01 Aeropuerto de Galápagos | Se usa GPS (Seymour), tal como viene en el script. |
| DP-02 Asignación de los jets | Los datos de ejemplo traen asignaciones; en la app el administrador asigna la aeronave en la pantalla 18. |
| DP-03 Tiempos de preparación | No hay intervalos definidos; solo se impide que una aeronave tenga vuelos superpuestos (trigger 50004). |
| DP-04 Tarifas y moneda | `Ruta.TarifaReferencial` (valores de ejemplo) en USD. |
| DP-05 Impuestos y comprobantes | Comprobantes académicos `ACAD-xxxxxx`, sin validez tributaria. |
| DP-06 Proveedor de pagos | Pago simulado, sin pasarela real. |
| DP-07 Duración de reserva pendiente | Existe `Reserva.FechaExpiracion`; el periodo de retención no está definido. |
| DP-08 Cancelaciones y reembolsos | Sin definir. Las reservas con operaciones no se eliminan (se cancelarán). |
| DP-09 Venta por tramos | Galápagos se muestra como itinerario con escala; los días en que Guayaquil es solo escala no aparecen como destino directo (`Ruta.ObtenerDestinos`). |
| DP-10 Conexión en Guayaquil | Se considera conexión válida si el segundo tramo sale el mismo día después de la llegada del primero. |
| DP-11 Configuración de asientos | Nomenclatura 1D, 2D, 2I, 3D, 3I, 4D, 4I del script. |
| DP-12 Política de check-in | Sin ventana definida; la BD exige asiento CONFIRMADA (trigger 50031). |
| DP-13 Funciones del counter | Por ahora ve lo mismo que el cliente más "Asistencia" (provisional). |
| DP-14 Recuperación y disponibilidad | Fuera del alcance de la primera entrega. |
| DP-15 Normativa aplicable | Fuera del alcance de la primera entrega; no se afirma cumplimiento normativo. |

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
