# Aerolínea VIP

Sistema web monolítico para la compra, reserva y administración de pasajes de una aerolínea VIP.
ASP.NET MVC 5 · .NET Framework 4.8.1 · Razor · Bootstrap · jQuery · ASP.NET Identity · SQL Server.

**Autores:** Jeremy Alexander Vilaña Guamán y Amaro Benjamin Caiza Sandoval

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

## Funcionalidades

- Consulta de vuelos, disponibilidad y selección de asientos por tramo.
- Registro de pasajeros, reservas y pago simulado.
- Consulta y cancelación de reservas según sus condiciones.
- Check-in y comprobantes académicos con descarga en PDF.
- Asistencia del counter para atender reservas y comprar para clientes existentes.
- Administración de vuelos, aeronaves, rutas, frecuencias, itinerarios y pasajeros.

El Administrador atiende los vuelos cancelados desde «Cancelaciones». El counter accede a «Asistencia» y solo puede cancelar reservas pendientes vigentes. La asignación de aeronaves se realiza manualmente por el Administrador.

## Alcance académico

Los pagos y reembolsos son simulados; no se procesa dinero real. La recuperación de acceso genera un token sin enviar correo. Los comprobantes no tienen validez tributaria. Las políticas operativas DP son propuestas del grupo.

La configuración de entrega usa la instancia predeterminada de SQL Server (`Data Source=.`). Ajuste las dos conexiones de `Web.config` si utiliza otro servidor.

## Documentación y pruebas

El [manual técnico](Documentacion/Manual_Tecnico.md) contiene las rutas de las 21 pantallas, el mapa de navegación y las condiciones de uso.

Con la aplicación iniciada puede ejecutar:

- `Pruebas\FlujoCompra.Integracion.ps1 -BaseUrl http://localhost:59817`
- `Pruebas\AreaCliente.Integracion.ps1 -BaseUrl http://localhost:59817`
- `Pruebas\Navegacion.Integracion.ps1 -BaseUrl http://localhost:59817`

Ajuste la URL a su instancia local. Las pruebas de compra y área del cliente utilizan las cuentas iniciales y eliminan sus registros temporales al terminar.

El ZIP contiene las fuentes y ambos scripts de base de datos. Antes de compilar una copia extraída debe restaurar los paquetes NuGet. Los PDF se generan con PDFsharp 1.50.5147.