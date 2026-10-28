param([string]$BaseUrl = 'http://localhost:59817')
$ErrorActionPreference = 'Stop'
function Verificar([bool]$condicion, [string]$mensaje) { if (-not $condicion) { throw $mensaje }; Write-Output "OK: $mensaje" }
function Obtener([string]$ruta) { return Invoke-WebRequest ($BaseUrl + $ruta) -WebSession $script:sesion -UseBasicParsing }
function Entrar([string]$usuario, [string]$clave, [string]$retorno = '') {
    $pagina = Invoke-WebRequest ($BaseUrl + '/Account/Login') -SessionVariable nueva -UseBasicParsing
    $script:sesion = $nueva
    $token = [regex]::Match($pagina.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    return Invoke-WebRequest ($BaseUrl + '/Account/Login') -Method Post -WebSession $script:sesion -UseBasicParsing -Body @{__RequestVerificationToken=$token;UserName=$usuario;Password=$clave;RememberMe='false';returnUrl=$retorno}
}
$script:sesion = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
foreach ($ruta in @('/', '/Account/Login', '/Account/Register', '/Account/ForgotPassword', '/Vuelos/Buscar')) {
    $pagina = Obtener $ruta
    Verificar ($pagina.StatusCode -eq 200) "Ruta pública $ruta"
}
[xml]$config = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) 'AerolineaVIP/Web.config')
$entidad = ($config.configuration.connectionStrings.add | Where-Object name -eq 'AerolineaVIPEntities').connectionString
Add-Type -AssemblyName System.Data
$conexion = [Data.SqlClient.SqlConnection]::new([regex]::Match($entidad, 'provider connection string="([^"]+)"').Groups[1].Value)
$conexion.Open()
try {
    $comando = $conexion.CreateCommand()
    $comando.CommandText = "SELECT TOP(1) v.VueloId FROM vw_VueloDetalle v JOIN vw_DisponibilidadVuelo d ON d.VueloId=v.VueloId WHERE v.SalidaProgramada>DATEADD(hour,2,GETDATE()) AND d.Disponibles>0 AND v.EstadoVueloCodigo IN('PROGRAMADO','CONFIRMADO') AND DATEDIFF(day,'19000101',v.FechaOperacion)%7<>6 ORDER BY v.FechaOperacion,v.HoraSalida"
    $vuelo = $comando.ExecuteScalar()
    if ($null -eq $vuelo -or $vuelo -is [DBNull]) { throw 'Se necesita un vuelo futuro disponible para verificar el retorno de login.' }
    $retorno = "/Vuelos/Disponibilidad?clave=$vuelo&cantidad=1"
    $disponibilidad = Obtener $retorno
    Verificar ($disponibilidad.Content.Contains('Iniciar sesi') -and -not $disponibilidad.Content.Contains('action="/Compra/Iniciar"')) 'Visitante continúa mediante login con retorno GET válido'
    $cliente = Entrar 'cliente01' 'Cliente#2026' $retorno
    Verificar ($cliente.Content.Contains('action="/Compra/Iniciar"') -and $cliente.BaseResponse.ResponseUri.AbsolutePath -eq '/Vuelos/Disponibilidad') 'Login devuelve al mapa seleccionado con formulario autenticado'
}
finally { $conexion.Dispose() }
foreach ($ruta in @('/Reservas/MisReservas', '/CheckIn', '/Comprobantes', '/Manage/ManageLogins')) { Verificar ((Obtener $ruta).StatusCode -eq 200) "Ruta de cliente $ruta" }
$super = Entrar 'superusuario' 'Super#2026'
Verificar ($super.Content.Contains('Mis reservas') -and $super.Content.Contains('Administraci')) 'SuperUsuario tiene navegación administrativa y de cliente'
$registro = Obtener '/Account/Register'
Verificar ($registro.Content.Contains('SuperUsuario') -and $registro.Content.Contains('EmpleadoCounter')) 'Registro privilegiado con selección de rol'
[void](Entrar 'admin01' 'Admin#2026')
foreach ($ruta in @('/Admin/Index', '/Admin/Vuelos', '/Admin/Itinerarios', '/Admin/TramosItinerario', '/Admin/Aeronaves', '/Admin/Gestion', '/Admin/Gestion?pestana=pasajeros', '/Admin/Gestion?pestana=reservas', '/Admin/Reportes', '/Admin/Reportes/Reservas', '/Admin/Reportes/Compras', '/Admin/Aeropuertos', '/Admin/Rutas', '/Admin/Frecuencias', '/Admin/Pasajeros', '/Admin/Catalogos')) {
    Verificar ((Obtener $ruta).StatusCode -eq 200) "Ruta administrativa $ruta"
}
[void](Entrar 'counter01' 'Counter#2026')
Verificar ((Obtener '/Reservas/Asistencia').Content.Contains('Nueva compra para un cliente')) 'Asistencia del counter y acceso a nueva compra'
Write-Output 'Las pantallas 8–12 y los detalles por ID se verifican con las dos pruebas de integración de flujo y área del cliente.'
