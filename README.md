# Aerolínea VIP

Sistema web monolítico para la compra, reserva y administración de pasajes de una aerolínea VIP.
ASP.NET MVC 5 · .NET Framework 4.8.1 · Razor · Bootstrap · jQuery · ASP.NET Identity · SQL Server.

**Autor:** Jeremy Vilaña

## Requisitos

- Visual Studio con la carga de trabajo "Desarrollo de ASP.NET y web" y .NET Framework 4.8.1.
  La solución está en formato `.slnx` (Visual Studio 2026 o 2022 17.13+). Con una versión anterior, abra directamente `AerolineaVIP\AerolineaVIP.csproj`.
- SQL Server (2016 o superior).
- SQL Server Management Studio (SSMS).

## Puesta en marcha

1. **Base de datos.** En SSMS ejecute, en este orden:
   1. `BaseDatos\Script_BD_AerolineaVIP.sql` → crea `AerolineaVIP` (dominio, con datos iniciales) y `AerolineaVIP_Identity` (tablas de Identity vacías).
   2. `AerolineaVIP\Scripts\Ampliar_AspNetUsers.sql` → agrega Nombres y Apellidos a `AspNetUsers`.
2. **Cadenas de conexión.** En `AerolineaVIP\Web.config` cambie el `Data Source` (y el usuario y la contraseña si corresponde) de las dos cadenas por los de su servidor:
   - `DefaultConnection` → base `AerolineaVIP_Identity`
   - `AerolineaVIPEntities` → base `AerolineaVIP`
3. **Compilar.** Abra la solución y compile (Ctrl+Shift+B). NuGet restaura los paquetes automáticamente; si no lo hace, use clic derecho en la solución → *Restaurar paquetes NuGet*.
4. **Ejecutar (F5).** Al iniciar, `Global.asax.cs` crea por código los roles y los usuarios iniciales si no existen.

## Credenciales iniciales

| Usuario | Rol | Contraseña |
|---|---|---|
| superusuario | SuperUsuario | Super#2026 |
| admin01 | Administrador | Admin#2026 |
| counter01 | EmpleadoCounter | Counter#2026 |
| cliente01 | Cliente | Cliente#2026 |

El inicio de sesión es con **nombre de usuario**. El registro público siempre crea cuentas con rol Cliente; solo el SuperUsuario puede crear cuentas con otros roles.

## Antes de entregar

En `Web.config` las dos cadenas de conexión deben quedar con **`Data Source=.`**.

## Más información

FASE 4 (pantallas 13–16): Mis reservas, gestión y cancelación con reembolso simulado, check-in y comprobantes propios con filtros, vista previa y descarga PDF. El Administrador accede a «Cancelaciones» para vuelos cancelados por la aerolínea. La nota de crédito muestra el importe reembolsado; la compra conserva el total original y el pago reversado registra el reembolso.

Con `counter01`, «Asistencia» permite buscar por código o documento y gestionar, pagar o hacer check-in de la reserva encontrada. «Nueva compra para un cliente» acepta el usuario o correo de una cuenta Cliente existente y conserva su titularidad durante la compra. El counter cancela únicamente pendientes vigentes y no accede a Admin. Mis reservas filtra por fecha del primer tramo; el vencimiento solo aparece en pendientes y los tipos de documento se muestran con sus nombres legibles.

La generación de PDF usa [PDFsharp 1.50.5147](https://www.nuget.org/packages/PDFsharp/1.50.5147), restaurable desde NuGet. No se requieren cambios de base de datos. Prueba reproducible: `Pruebas\AreaCliente.Integracion.ps1 -BaseUrl http://localhost:59817` con la aplicación iniciada; elimina sus datos temporales al terminar.

El flujo de compra (pantallas 4–12) está implementado y probado por el grupo: búsqueda y disponibilidad, selección por tramo, pasajeros, reserva, pago simulado y confirmación con comprobantes académicos. Incluye ajustes visuales y validaciones solicitadas para los wireframes. La recuperación de acceso genera un token sin enviar correo. Las políticas DP siguen siendo propuestas del grupo.

Para ejecutar la prueba de integración con la aplicación iniciada, use `Pruebas\FlujoCompra.Integracion.ps1 -BaseUrl http://localhost:59817` (ajuste la URL a su instancia). La prueba utiliza las cadenas de Web.config y elimina sus propios registros temporales al terminar. Para el commit de FASE 3 el grupo autorizó incluir la configuración académica local de Web.config; no se realiza push.

Las reglas del docente, el estado de cada fase y las convenciones del código están en `CLAUDE.md`.
