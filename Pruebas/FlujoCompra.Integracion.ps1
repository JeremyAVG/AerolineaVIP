param([string]$BaseUrl = 'http://localhost:59817')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$raiz = Split-Path $PSScriptRoot -Parent
[xml]$configuracion = Get-Content (Join-Path $raiz 'AerolineaVIP/Web.config')
$cadenaEntidad = ($configuracion.configuration.connectionStrings.add | Where-Object name -eq 'AerolineaVIPEntities').connectionString
$cadena = [regex]::Match($cadenaEntidad, 'provider connection string="([^"]+)"').Groups[1].Value
$conexion = [System.Data.SqlClient.SqlConnection]::new($cadena)
$conexion.Open()
$documento = 'P' + [guid]::NewGuid().ToString('N').Substring(0, 18)
function SqlValor([string]$consulta) {
    $comando = $conexion.CreateCommand()
    $comando.CommandText = $consulta
    [void]$comando.Parameters.AddWithValue('@doc', $documento)
    try { return $comando.ExecuteScalar() } finally { $comando.Dispose() }
}
function Verificar([bool]$condicion, [string]$nombre) {
    if (-not $condicion) { throw "FALLO: $nombre" }
    Write-Output "OK: $nombre"
}
function Token($pagina) {
    $valor = [regex]::Match($pagina.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    if (-not $valor) { throw 'No se encontró token antifalsificación.' }
    return $valor
}
function Obtener([string]$ruta) { return Invoke-WebRequest ($BaseUrl + $ruta) -WebSession $script:sesion -UseBasicParsing }
function Enviar([string]$ruta, $pagina, [hashtable]$datos) {
    if ($ruta -eq '/Compra/Pagar') {
        if (-not $datos.ContainsKey('Vencimiento')) { $datos.Vencimiento = (Get-Date).AddYears(1).ToString('MM/yy') }
        if (-not $datos.ContainsKey('CodigoSeguridad')) { $datos.CodigoSeguridad = if ($datos.Marca -eq 'AMEX') { '1234' } else { '123' } }
    }
    $datos['__RequestVerificationToken'] = Token $pagina
    return Invoke-WebRequest ($BaseUrl + $ruta) -Method Post -WebSession $script:sesion -Body $datos -UseBasicParsing
}
try {
    Verificar ((SqlValor 'SELECT COUNT(*) FROM Pasajero WHERE NumeroDocumento=@doc') -eq 0) 'Documento temporal nuevo'
    $login = Invoke-WebRequest ($BaseUrl + '/Account/Login') -SessionVariable sesion -UseBasicParsing
    $inicio = Enviar '/Account/Login' $login @{ UserName = 'cliente01'; Password = 'Cliente#2026'; RememberMe = 'false' }
    Verificar ($inicio.Content.Contains('cliente01')) 'Inicio de sesión Cliente'
    $recuperar = Obtener '/Account/ForgotPassword'
    $neutral = Enviar '/Account/ForgotPassword' $recuperar @{ Email = 'inexistente@ejemplo.invalid' }
    Verificar ($neutral.Content.Contains('Si existe una cuenta')) 'Recuperación neutral sin correo'
    $existente = Enviar '/Account/ForgotPassword' $recuperar @{ Email = 'cliente01@aerolineavip.test' }
    Verificar ($existente.Content.Contains('Si existe una cuenta')) 'Recuperación de cuenta existente con respuesta neutral'
    $fecha = SqlValor "SELECT MIN(FechaOperacion) FROM Vuelo WHERE FechaOperacion > CONVERT(date,GETDATE()) AND AeronaveId IS NOT NULL AND EstadoVueloCodigo IN ('PROGRAMADO','CONFIRMADO') AND DATEDIFF(day,'19000101',FechaOperacion)%7<>6"
    if ($fecha -is [DBNull]) { throw 'Se necesita un vuelo futuro con aeronave para probar.' }
    $dia = ([datetime]$fecha).ToString('yyyy-MM-dd')
    $vuelo = SqlValor "SELECT TOP(1) v.VueloId FROM Vuelo v JOIN vw_DisponibilidadVuelo d ON d.VueloId=v.VueloId WHERE v.FechaOperacion='$dia' AND d.Disponibles>0 ORDER BY v.VueloId"
    $origen = SqlValor "SELECT r.AeropuertoOrigenId FROM Vuelo v JOIN Frecuencia f ON f.FrecuenciaId=v.FrecuenciaId JOIN Ruta r ON r.RutaId=f.RutaId WHERE v.VueloId=$vuelo"
    $destino = SqlValor "SELECT r.AeropuertoDestinoId FROM Vuelo v JOIN Frecuencia f ON f.FrecuenciaId=v.FrecuenciaId JOIN Ruta r ON r.RutaId=f.RutaId WHERE v.VueloId=$vuelo"
    $resultados = Obtener "/Vuelos/Resultados?OrigenId=$origen&DestinoId=$destino&Fecha=$dia&Pasajeros=1"
    Verificar ($resultados.Content.Contains('name="clave"')) 'Resultados con tarjetas y disponibilidad'
    $disponibilidad = Obtener "/Vuelos/Disponibilidad?clave=$vuelo&cantidad=1"
    Verificar ($disponibilidad.Content.Contains('Hora de c') -and $disponibilidad.Content.Contains('Actualizar disponibilidad') -and -not $disponibilidad.Content.Contains('por pasajero')) 'Disponibilidad sin tarjeta repetida, con resumen y actualización'
    $asientos = Enviar '/Compra/Iniciar' $resultados @{ clave = "$vuelo"; cantidad = '1' }
    Verificar ([regex]::Matches($asientos.Content, 'data-numero=').Count -eq 7) 'Mapa con siete asientos'
    Verificar ($asientos.Content.Contains('<strong>1D</strong>') -and -not $asientos.Content.Contains('<strong>1I</strong>')) 'Distribución Phenom 300E'
    Verificar ($asientos.Content.Contains('aria-current="step"') -and $asientos.Content.Contains('aria-label="Asiento 3D, fila 3, lado derecho,')) 'Pasos activos y nombres accesibles de asientos'
    $rechazo = Enviar '/Compra/Asientos' $asientos @{ asientos = '0' }
    Verificar ($rechazo.Content.Contains('ya no est')) 'Revalidación de asiento manipulado'
    $libre = [regex]::Match($asientos.Content, 'data-numero="([0-9]+)" aria-pressed=').Groups[1].Value
    if (-not $libre) { throw 'No hay un asiento libre para probar.' }
    $pasajeros = Enviar '/Compra/Asientos' $rechazo @{ asientos = $libre }
    Verificar ($pasajeros.Content.Contains('Personas[0].Documento')) 'Paso de pasajeros'
    $invalido = Enviar '/Compra/Pasajeros' $pasajeros @{ 'Personas[0].TipoDocumento'='CEDULA'; 'Personas[0].Documento'='12345'; 'Personas[0].Nombres'='Prueba'; 'Personas[0].Apellidos'='Temporal' }
    Verificar ($invalido.Content.Contains('10 d')) 'Cédula inválida rechazada en servidor'
    $resumen = Enviar '/Compra/Pasajeros' $invalido @{ 'Personas[0].TipoDocumento'='PASAPORTE'; 'Personas[0].Documento'=$documento; 'Personas[0].Nombres'='Prueba'; 'Personas[0].Apellidos'='Temporal'; 'Personas[0].FechaNacimiento'='2000-01-02'; 'Personas[0].Correo'='prueba@ejemplo.invalid'; 'Personas[0].Telefono'='0999999999' }
    Verificar ($resumen.Content.Contains('3. Confirme su reserva')) 'Resumen calculado en servidor'
    $clave = [regex]::Match($resumen.Content, 'name="clave" value="([^"]+)"').Groups[1].Value
    $sinAceptar = Enviar '/Compra/Confirmar' $resumen @{ clave = $clave; Destino = 'pagar' }
    Verificar ($sinAceptar.Content.Contains('Debe aceptar las condiciones')) 'Condiciones obligatorias en servidor'
    $pagar = Enviar '/Compra/Confirmar' $resumen @{ clave = $clave; AceptaCondiciones='true'; Destino='pagar' }
    Verificar ($pagar.Content.Contains('SIMULACI')) 'Reserva registrada y pantalla de pago'
    $id = [int](SqlValor 'SELECT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento=@doc')
    Verificar ($id -gt 0) 'Reserva y asignación persistidas'
    Verificar ((SqlValor "SELECT COUNT(*) FROM Pasajero WHERE NumeroDocumento=@doc AND FechaNacimiento='20000102' AND Correo='prueba@ejemplo.invalid' AND Telefono='0999999999'") -eq 1) 'Datos opcionales de pasajero persistidos'
    $tarifaCorrecta = SqlValor "SELECT COUNT(*) FROM ReservaPasajeroVuelo a JOIN Vuelo v ON v.VueloId=a.VueloId JOIN Frecuencia f ON f.FrecuenciaId=v.FrecuenciaId JOIN Ruta r ON r.RutaId=f.RutaId WHERE a.ReservaId=$id AND a.ImporteTramo=r.TarifaReferencial"
    Verificar ($tarifaCorrecta -eq 1) 'Tarifa copiada de Ruta'
    $retencion = SqlValor "SELECT COUNT(*) FROM Reserva WHERE ReservaId=$id AND FechaExpiracion>FechaCreacion AND FechaExpiracion<=DATEADD(hour,24,FechaCreacion)"
    Verificar ($retencion -eq 1) 'Retención máxima de 24 horas'
    $repetida = Enviar '/Compra/Confirmar' $resumen @{ clave = $clave; AceptaCondiciones='true'; Destino='pagar' }
    Verificar ((SqlValor 'SELECT COUNT(DISTINCT a.ReservaId) FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento=@doc') -eq 1) 'Confirmación repetida sin duplicar reserva'
    $pagoInvalido = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='AMEX'; Numero='1234567890123456'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO' }
    Verificar ($pagoInvalido.Content.Contains('longitud no corresponde')) 'Longitud según marca validada en servidor'
    Verificar ((SqlValor "SELECT COUNT(*) FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId=$id") -eq 0) 'Pago inválido no persiste'
    $vencido = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='VISA'; Numero='4111111111111111'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO'; Vencimiento='01/20' }
    Verificar ($vencido.Content.Contains('vencida')) 'Vencimiento pasado rechazado en servidor'
    $mesInvalido = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='VISA'; Numero='4111111111111111'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO'; Vencimiento='13/30' }
    Verificar ($mesInvalido.Content.Contains('mes entre 01 y 12')) 'Mes inválido rechazado en servidor'
    $codigoInvalido = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='AMEX'; Numero='371111111111111'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO'; CodigoSeguridad='123' }
    Verificar ($codigoInvalido.Content.Contains('debe tener 4')) 'AMEX exige cuatro dígitos de seguridad'
    $rechazado = Enviar '/Compra/Pagar' $pagoInvalido @{ ReservaId="$id"; Marca='VISA'; Numero='4111111111110000'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO' }
    Verificar ($rechazado.Content.Contains('RECHAZADO') -and $rechazado.Content.Contains('PENDIENTE')) 'Tarjeta 0000 rechazada y reserva pendiente'
    $pagar = Obtener "/Compra/Pagar/$id"
    $aprobado = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='VISA'; Numero='4111111111111111'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO' }
    Verificar ($aprobado.Content.Contains('CONFIRMADA') -and $aprobado.Content.Contains('PAGADA') -and $aprobado.Content.Contains('APROBADO')) 'Estados separados de reserva, compra y pago'
    Verificar ((SqlValor "SELECT COUNT(*) FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$id AND ((co.TipoComprobanteCodigo='FACTURA' AND co.NumeroComprobante LIKE 'ACAD-%') OR (co.TipoComprobanteCodigo='CONFIRMACION' AND co.NumeroComprobante LIKE 'CNF-%'))") -eq 2) 'Confirmación CNF y factura ACAD con numeración separada'
    Verificar ((SqlValor "SELECT COUNT(*) FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId=$id AND p.CodigoOperacion LIKE 'SIM-RV________-0[12]'") -eq 2) 'Operación con intento de dos dígitos'
    $comprobantes = Obtener "/Compra/Comprobante/$id"
    Verificar ($comprobantes.Content.Contains('CNF-') -and $comprobantes.Content.Contains('ACAD-')) 'Consulta real de comprobantes'
    $duplicado = Enviar '/Compra/Pagar' $pagar @{ ReservaId="$id"; Marca='VISA'; Numero='4111111111111111'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO' }
    Verificar ((SqlValor "SELECT COUNT(*) FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId=$id AND p.EstadoPagoCodigo='APROBADO'") -eq 1) 'Reenvío sin pago aprobado duplicado'
    $asientos = Enviar '/Compra/Iniciar' $resultados @{ clave = "$vuelo"; cantidad = '1' }
    $libre = [regex]::Match($asientos.Content, 'data-numero="([0-9]+)" aria-pressed=').Groups[1].Value
    $pasajeros = Enviar '/Compra/Asientos' $asientos @{ asientos = $libre }
    $resumen = Enviar '/Compra/Pasajeros' $pasajeros @{ 'Personas[0].TipoDocumento'='PASAPORTE'; 'Personas[0].Documento'=($documento+'E'); 'Personas[0].Nombres'='Prueba'; 'Personas[0].Apellidos'='Temporal' }
    $clave = [regex]::Match($resumen.Content, 'name="clave" value="([^"]+)"').Groups[1].Value
    $pendiente = Enviar '/Compra/Confirmar' $resumen @{ clave = $clave; AceptaCondiciones='true'; Destino='reserva' }
    Verificar ($pendiente.Content.Contains('PENDIENTE') -and $pendiente.Content.Contains('SIN PAGO')) 'Confirmar reserva sin iniciar pago'
    $idExpirar = [int](SqlValor "SELECT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento=@doc+'E'")
    Verificar ($idExpirar -gt 0) 'Segunda reserva temporal para probar expiración'
    $detalle = Obtener "/Compra/VerReserva/$idExpirar"
    Verificar ($detalle.Content.Contains('Pagar reserva')) 'Ver reserva pendiente con acceso al pago'
    $pagar = Obtener "/Compra/Pagar/$idExpirar"
    [void](Enviar '/Compra/Pagar' $pagar @{ ReservaId="$idExpirar"; Marca='VISA'; Numero='4111111111110000'; Titular='Prueba Temporal'; Metodo='TARJETA_CREDITO' })
    [void](SqlValor "UPDATE Reserva SET FechaCreacion=DATEADD(hour,-25,GETDATE()),FechaExpiracion=DATEADD(hour,-1,GETDATE()) WHERE ReservaId=$idExpirar")
    [void](Obtener '/Vuelos/Buscar')
    Verificar ((SqlValor "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$idExpirar") -eq 'EXPIRADA') 'Expiración al consultar'
    Verificar ((SqlValor "SELECT EstadoAsignacionCodigo FROM ReservaPasajeroVuelo WHERE ReservaId=$idExpirar") -eq 'LIBERADA') 'Asientos liberados al expirar'
    Verificar ((SqlValor "SELECT EstadoCompraCodigo FROM Compra WHERE ReservaId=$idExpirar") -eq 'ANULADA') 'Compra pendiente anulada al expirar'
    $domingo = SqlValor "SELECT MIN(FechaOperacion) FROM Vuelo WHERE FechaOperacion>GETDATE() AND DATEDIFF(day,'19000101',FechaOperacion)%7=6"
    if ($domingo -isnot [DBNull]) {
        $diaDomingo = ([datetime]$domingo).ToString('yyyy-MM-dd')
        $gps = SqlValor "SELECT AeropuertoId FROM Aeropuerto WHERE CodigoIATA='GPS'"
        $uio = SqlValor "SELECT AeropuertoId FROM Aeropuerto WHERE CodigoIATA='UIO'"
        $ida = Obtener "/Vuelos/Resultados?OrigenId=$uio&DestinoId=$gps&Fecha=$diaDomingo&Pasajeros=1"
        $par = [regex]::Match($ida.Content,'name="clave" value="([0-9]+,[0-9]+)"').Groups[1].Value
        Verificar ([bool]$par) 'Domingo: itinerario de ida con dos tramos'
        $regreso = Obtener "/Vuelos/Resultados?OrigenId=$gps&DestinoId=$uio&Fecha=$diaDomingo&Pasajeros=1"
        Verificar ([regex]::IsMatch($regreso.Content,'name="clave" value="[0-9]+,[0-9]+"')) 'Domingo: itinerario de regreso con dos tramos'
        $suelto = Enviar '/Compra/Iniciar' $ida @{ clave = $par.Split(',')[0]; cantidad = '1' }
        Verificar ($suelto.Content.Contains('ya no est')) 'Domingo: tramo suelto rechazado en servidor'
        $mapas = Enviar '/Compra/Iniciar' $ida @{ clave = $par; cantidad = '1' }
        Verificar ([regex]::Matches($mapas.Content,'class="panel-tramo"').Count -eq 2) 'Selección por pestañas en ambos tramos'
    }
    Write-Output 'Flujo completo verificado. Se eliminarán únicamente los registros temporales de esta prueba.'
}
finally {
    $limpiar = $conexion.CreateCommand()
    $limpiar.CommandText = @'
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @reservas TABLE (Id int PRIMARY KEY);
INSERT INTO @reservas SELECT DISTINCT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento IN (@doc,@doc+'E');
DELETE co FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId IN (SELECT Id FROM @reservas);
DELETE p FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId IN (SELECT Id FROM @reservas);
DELETE FROM Compra WHERE ReservaId IN (SELECT Id FROM @reservas);
DELETE FROM ReservaPasajeroVuelo WHERE ReservaId IN (SELECT Id FROM @reservas);
DELETE FROM Reserva WHERE ReservaId IN (SELECT Id FROM @reservas);
DELETE FROM Pasajero WHERE NumeroDocumento IN (@doc,@doc+'E');
COMMIT;
'@
    [void]$limpiar.Parameters.AddWithValue('@doc', $documento)
    try { [void]$limpiar.ExecuteNonQuery() } finally { $limpiar.Dispose(); $conexion.Dispose() }
}
