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

Las reglas del docente, el estado de cada fase y las convenciones del código están en `CLAUDE.md`.
