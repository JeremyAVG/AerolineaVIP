param([string]$BaseUrl = 'http://localhost:59817')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
[xml]$config = Get-Content (Join-Path (Split-Path $PSScriptRoot -Parent) 'AerolineaVIP/Web.config')
$entidad = ($config.configuration.connectionStrings.add | Where-Object name -eq 'AerolineaVIPEntities').connectionString
$conexion = [Data.SqlClient.SqlConnection]::new([regex]::Match($entidad, 'provider connection string="([^"]+)"').Groups[1].Value)
$conexion.Open()
$prefijo = 'F4' + [guid]::NewGuid().ToString('N').Substring(0, 12)
$frecuenciaId = 0
function Sql([string]$consulta) {
    $comando = $conexion.CreateCommand(); $comando.CommandText = $consulta
    [void]$comando.Parameters.AddWithValue('@doc', $prefijo)
    try { return $comando.ExecuteScalar() } finally { $comando.Dispose() }
}
function Comprobar([bool]$condicion, [string]$texto) { if (-not $condicion) { throw "FALLO: $texto" }; Write-Output "OK: $texto" }
function Obtener([string]$ruta) { return Invoke-WebRequest ($BaseUrl+$ruta) -WebSession $script:sesion -UseBasicParsing }
function Enviar([string]$ruta, $pagina, [hashtable]$datos) {
    $datos.__RequestVerificationToken = [regex]::Match($pagina.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    if (-not $datos.__RequestVerificationToken) { throw 'Falta token antifalsificación.' }
    return Invoke-WebRequest ($BaseUrl+$ruta) -Method Post -WebSession $script:sesion -Body $datos -UseBasicParsing
}
function Entrar([string]$usuario, [string]$clave) {
    $pagina = Invoke-WebRequest ($BaseUrl+'/Account/Login') -SessionVariable nuevaSesion -UseBasicParsing
    $script:sesion = $nuevaSesion
    [void](Enviar '/Account/Login' $pagina @{UserName=$usuario;Password=$clave;RememberMe='false'})
}
function Denegado([string]$ruta) {
    try { $respuesta = Obtener $ruta; return $respuesta.Content.Contains('Acceso no autorizado') -or $respuesta.Content.Contains('No tiene permisos') } catch { return [int]$_.Exception.Response.StatusCode -in @(403,404) }
}
try {
    $frecuenciaId = [int](Sql @'
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @ahora datetime2=GETDATE(), @salida datetime2=DATEADD(hour,3,GETDATE());
DECLARE @fecha date=CONVERT(date,@salida), @hora time(0)=CONVERT(time(0),@salida), @llegada time(0)=CONVERT(time(0),DATEADD(minute,30,@salida));
IF @llegada<=@hora THROW 50100,'La prueba necesita un intervalo que no cruce medianoche.',1;
DECLARE @dia tinyint=(DATEDIFF(day,'19000101',@fecha)%7)+1;
DECLARE @ruta int=(SELECT TOP(1) RutaId FROM Ruta WHERE Activa=1 ORDER BY RutaId);
DECLARE @avion int=(SELECT TOP(1) a.AeronaveId FROM Aeronave a WHERE a.EstadoAeronaveCodigo='DISPONIBLE'
 AND NOT EXISTS(SELECT 1 FROM vw_VueloDetalle v WHERE v.AeronaveId=a.AeronaveId AND v.EstadoVueloCodigo<>'CANCELADO'
 AND v.FechaOperacion IN(@fecha,DATEADD(day,7,@fecha),DATEADD(day,14,@fecha)) AND v.HoraSalida<@llegada AND v.HoraLlegada>@hora) ORDER BY a.AeronaveId);
IF @avion IS NULL THROW 50101,'La prueba necesita una aeronave libre por 30 minutos.',1;
INSERT Frecuencia(RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa) VALUES(@ruta,@dia,@hora,@llegada,1);
DECLARE @f int=SCOPE_IDENTITY();
INSERT Vuelo(FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo) VALUES(@f,@fecha,@avion,'PROGRAMADO');
DECLARE @cerca int=SCOPE_IDENTITY();
INSERT Vuelo(FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo) VALUES(@f,DATEADD(day,7,@fecha),@avion,'PROGRAMADO');
DECLARE @lejos int=SCOPE_IDENTITY();
INSERT Vuelo(FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo) VALUES(@f,DATEADD(day,14,@fecha),@avion,'PROGRAMADO');
DECLARE @cancelado int=SCOPE_IDENTITY();
DECLARE @cliente nvarchar(128)=(SELECT Id FROM AerolineaVIP_Identity.dbo.AspNetUsers WHERE UserName='cliente01');
DECLARE @otro nvarchar(128)=(SELECT Id FROM AerolineaVIP_Identity.dbo.AspNetUsers WHERE UserName='admin01');
DECLARE @i int=0;
WHILE @i<6 BEGIN
 DECLARE @documento varchar(20)=@doc+CONVERT(varchar(1),@i);
 INSERT Pasajero(TipoDocumentoCodigo,NumeroDocumento,Nombres,Apellidos,FechaRegistro) VALUES('PASAPORTE',@documento,N'Prueba',N'Fase Cuatro',@ahora);
 DECLARE @p int=SCOPE_IDENTITY();
 INSERT Reserva(UsuarioId,FechaCreacion,FechaExpiracion,EstadoReservaCodigo) VALUES(CASE WHEN @i=4 THEN @otro ELSE @cliente END,@ahora,DATEADD(hour,1,@ahora),CASE WHEN @i IN(0,5) THEN 'PENDIENTE' ELSE 'CONFIRMADA' END);
 DECLARE @r int=SCOPE_IDENTITY();
 INSERT ReservaPasajeroVuelo(ReservaId,PasajeroId,VueloId,NumeroAsiento,ImporteTramo,EstadoAsignacionCodigo,FechaAsignacion)
 VALUES(@r,@p,CASE WHEN @i=2 THEN @cerca WHEN @i=3 THEN @cancelado ELSE @lejos END,CONVERT(tinyint,@i+1),100,CASE WHEN @i IN(0,5) THEN 'PENDIENTE' ELSE 'CONFIRMADA' END,@ahora);
 IF @i NOT IN(0,5) BEGIN
  INSERT Compra(ReservaId,FechaCompra,Importe,MonedaCodigo,EstadoCompraCodigo,ClaveIdempotencia) VALUES(@r,@ahora,100,'USD','PAGADA',NEWID());
  DECLARE @c int=SCOPE_IDENTITY();
  INSERT Pago(CompraId,FechaPago,Importe,EstadoPagoCodigo,MetodoPagoCodigo,MarcaTarjetaCodigo,UltimosCuatroDigitos,NombreTitular,CodigoOperacion,EsSimulado,MensajeResultado)
  VALUES(@c,@ahora,100,'APROBADO','TARJETA_CREDITO','VISA','1111','Prueba',@doc+CONVERT(varchar(1),@i),1,'Prueba temporal');
  INSERT Comprobante(CompraId,TipoComprobanteCodigo,NumeroComprobante,FechaEmision,EstadoComprobanteCodigo) VALUES(@c,'FACTURA','TEST-'+CONVERT(varchar(10),@c),@ahora,'EMITIDO');
 END;
 SET @i+=1;
END;
UPDATE Vuelo SET EstadoVueloCodigo='CANCELADO' WHERE VueloId=@cancelado;
COMMIT;
SELECT @f;
'@)
    Comprobar ($frecuenciaId -gt 0) 'Datos temporales aislados creados'
    $ids = @(); $codigos = @()
    foreach ($numero in 0..5) {
        $ids += [int](Sql "SELECT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento=@doc+'$numero'")
        $codigos += [string](Sql "SELECT r.CodigoReserva FROM Reserva r WHERE r.ReservaId=$($ids[$numero])")
    }
    Entrar 'cliente01' 'Cliente#2026'
    $lista = Obtener '/Reservas/MisReservas?Estado=CONFIRMADA&Pagina=1&UsuarioId=ignorado'
    Comprobar ($lista.Content.Contains($codigos[1]) -and -not $lista.Content.Contains($codigos[4])) 'Mis reservas ignora UsuarioId de URL y filtra por autenticación'
    Comprobar (Denegado '/Reservas/Asistencia') 'Cliente no puede acceder a Asistencia'
    Comprobar (-not $lista.Content.Contains('>Asistencia</a>')) 'Menú Asistencia oculto para Cliente'
    $fechaViaje = ([datetime](Sql "SELECT MIN(v.FechaOperacion) FROM ReservaPasajeroVuelo a JOIN Vuelo v ON v.VueloId=a.VueloId WHERE a.ReservaId=$($ids[1])")).ToString('yyyy-MM-dd')
    $porViaje = Obtener "/Reservas/MisReservas?Desde=$fechaViaje&Hasta=$fechaViaje&Estado=CONFIRMADA"
    Comprobar ($porViaje.Content.Contains($codigos[1]) -and -not $porViaje.Content.Contains($codigos[2])) 'Filtros por fecha de viaje del primer tramo, no por creación'
    $confirmada = Obtener "/Compra/Final/$($ids[2])"
    Comprobar (-not $confirmada.Content.Contains('Vence:') -and $confirmada.Content.Contains('Pasaporte ')) 'Confirmación con documento legible y sin vencimiento de reserva pagada'
    Comprobar (Denegado "/Reservas/Gestion/$($ids[4])") 'Detalle ajeno denegado'
    $gestion = Obtener "/Reservas/Gestion/$($ids[0])"
    Comprobar ($gestion.Content.Contains('Vence:')) 'Vencimiento visible mientras la reserva está pendiente'
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[0];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$($ids[0])") -eq 'CANCELADA') 'Pendiente cancelada sin costo'
    Comprobar ((Sql "SELECT COUNT(*) FROM Compra WHERE ReservaId=$($ids[0])") -eq 0) 'Cancelación pendiente sin compra ni reembolso'
    $gestion = Obtener "/Reservas/Gestion/$($ids[1])"
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[1];ConfirmarCancelacion='false'})
    Comprobar ((Sql "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$($ids[1])") -eq 'CONFIRMADA') 'Confirmación de cancelación obligatoria'
    [void](Sql "INSERT Comprobante(CompraId,TipoComprobanteCodigo,NumeroComprobante,FechaEmision,EstadoComprobanteCodigo) SELECT CompraId,'NOTA_CREDITO','NC-'+RIGHT('000000'+CONVERT(varchar(10),CompraId),6),GETDATE(),'EMITIDO' FROM Compra WHERE ReservaId=$($ids[1])")
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[1];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT COUNT(*) FROM Reserva r JOIN Compra c ON c.ReservaId=r.ReservaId JOIN Pago p ON p.CompraId=c.CompraId JOIN ReservaPasajeroVuelo a ON a.ReservaId=r.ReservaId WHERE r.ReservaId=$($ids[1]) AND r.EstadoReservaCodigo='CONFIRMADA' AND c.EstadoCompraCodigo='PAGADA' AND p.EstadoPagoCodigo='APROBADO' AND p.Importe=100 AND a.EstadoAsignacionCodigo='CONFIRMADA'") -eq 1) 'Fallo al emitir nota revierte toda la transacción'
    [void](Sql "DELETE co FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$($ids[1]) AND co.TipoComprobanteCodigo='NOTA_CREDITO'")
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[1];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT EstadoCompraCodigo FROM Compra WHERE ReservaId=$($ids[1])") -eq 'ANULADA') 'Compra anulada al cancelar reserva pagada'
    Comprobar ((Sql "SELECT p.Importe FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId=$($ids[1]) AND p.EstadoPagoCodigo='REVERSADO'") -eq 80) 'Reembolso simulado del 80 %'
    Comprobar ((Sql "SELECT EstadoAsignacionCodigo FROM ReservaPasajeroVuelo WHERE ReservaId=$($ids[1])") -eq 'CANCELADA') 'Asientos cancelados y liberados'
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[1];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT COUNT(*) FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$($ids[1]) AND co.TipoComprobanteCodigo='NOTA_CREDITO'") -eq 1) 'Reenvío de cancelación no duplica nota ni reembolso'
    $nc = [int](Sql "SELECT co.ComprobanteId FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$($ids[1]) AND co.TipoComprobanteCodigo='NOTA_CREDITO' AND co.NumeroComprobante LIKE 'NC-%'")
    $nota = Obtener "/Comprobantes/VistaPrevia/$nc"
    Comprobar ($nota.Content.Contains('80.00 USD')) 'Nota de crédito por el valor reembolsado'
    $pdf = Obtener "/Comprobantes/Descargar/$nc"
    Comprobar ($pdf.Headers['Content-Type'] -like 'application/pdf*' -and [Text.Encoding]::ASCII.GetString($pdf.RawContentStream.ToArray()).StartsWith('%PDF-')) 'PDF generado en servidor'
    $ajeno = [int](Sql "SELECT co.ComprobanteId FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$($ids[4])")
    Comprobar (Denegado "/Comprobantes/Descargar/$ajeno") 'PDF ajeno denegado'
    $gestion = Obtener "/Reservas/Gestion/$($ids[2])"
    Comprobar ($gestion.Content.Contains('no se permite cancelar por web')) 'Motivo visible dentro de las 24 horas'
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[2];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$($ids[2])") -eq 'CONFIRMADA') 'Cancelación dentro de 24 horas rechazada en servidor'
    $check = Obtener "/CheckIn?CodigoReserva=$($codigos[2])&Documento=$($prefijo)2"
    $asignacion = [int](Sql "SELECT AsignacionId FROM ReservaPasajeroVuelo WHERE ReservaId=$($ids[2])")
    Comprobar ($check.Content.Contains('Elegible para check-in')) 'Check-in abierto para tramo dentro de 24 horas'
    [void](Enviar '/CheckIn/Registrar' $check @{CodigoReserva=$codigos[2];Documento=($prefijo+'2');Seleccionados=$asignacion})
    [void](Enviar '/CheckIn/Registrar' $check @{CodigoReserva=$codigos[2];Documento=($prefijo+'2');Seleccionados=$asignacion})
    Comprobar ((Sql "SELECT COUNT(*) FROM CheckIn WHERE AsignacionId=$asignacion AND EstadoCheckInCodigo='COMPLETADO'") -eq 1) 'Check-in único ante reenvío'
    $neutral = Obtener "/CheckIn?CodigoReserva=$($codigos[4])&Documento=$($prefijo)4"
    Comprobar ($neutral.Content.Contains('No se encontr')) 'Búsqueda de reserva ajena con mensaje neutral'
    Entrar 'counter01' 'Counter#2026'
    Comprobar (Denegado '/Admin/Index') 'Counter no entra al área Admin'
    Comprobar (Denegado "/Reservas/Gestion/$($ids[4])") 'Counter no accede por ID a una reserva no encontrada'
    Comprobar (Denegado "/Comprobantes/Descargar/$ajeno") 'Counter no descarga por ID un comprobante de reserva no encontrada'
    $asistencia = Obtener "/Reservas/Asistencia?Buscar=$($codigos[2])"
    Comprobar ($asistencia.Content.Contains($codigos[2]) -and $asistencia.Content.Contains('>Asistencia</a>')) 'Counter busca por código y tiene menú Asistencia'
    $porDocumento = Obtener "/Reservas/Asistencia?Buscar=$($prefijo)2"
    Comprobar ($porDocumento.Content.Contains($codigos[2])) 'Counter busca por documento del pasajero'
    $gestionPagada = Obtener "/Reservas/Gestion/$($ids[2])"
    Comprobar ($gestionPagada.Content.Contains('counter solo puede cancelar reservas')) 'Gestión del counter explica prohibición de cancelar pagadas'
    $nueva = Obtener '/Reservas/NuevaCompra'
    [void](Enviar '/Reservas/Cancelar' $nueva @{ReservaId=$ids[2];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$($ids[2])") -eq 'CONFIRMADA') 'Servidor rechaza intento del counter de cancelar una pagada'
    $asistenciaPendiente = Obtener "/Reservas/Asistencia?Buscar=$($codigos[5])"
    Comprobar ($asistenciaPendiente.Content.Contains('>Pagar</a>')) 'Asistencia ofrece pago de una pendiente'
    $pagoAsistido = Obtener "/Compra/Pagar/$($ids[5])"
    Comprobar ($pagoAsistido.Content.Contains('4. Pago simulado')) 'Counter accede al pago de una reserva encontrada'
    $gestionPendiente = Obtener "/Reservas/Gestion/$($ids[5])"
    [void](Enviar '/Reservas/Cancelar' $gestionPendiente @{ReservaId=$ids[5];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT EstadoReservaCodigo FROM Reserva WHERE ReservaId=$($ids[5])") -eq 'CANCELADA') 'Counter cancela reservas pendientes vigentes'
    [void](Obtener "/Reservas/Asistencia?Buscar=$($codigos[2])")
    $check = Obtener "/CheckIn?CodigoReserva=$($codigos[2])&Documento=$($prefijo)2"
    $checkId = [int](Sql "SELECT CheckInId FROM CheckIn WHERE AsignacionId=$asignacion AND EstadoCheckInCodigo='COMPLETADO'")
    [void](Enviar '/CheckIn/Anular' $check @{CodigoReserva=$codigos[2];Documento=($prefijo+'2');checkInId=$checkId})
    Comprobar ((Sql "SELECT EstadoCheckInCodigo FROM CheckIn WHERE CheckInId=$checkId") -eq 'ANULADO') 'Counter puede anular check-in'
    $nueva = Obtener '/Reservas/NuevaCompra'
    $noCliente = Enviar '/Reservas/NuevaCompra' $nueva @{Cliente='admin01'}
    Comprobar ($noCliente.Content.Contains('cuenta existente con rol Cliente')) 'Nueva compra rechaza cuentas que no son Cliente'
    $porNombre = Enviar '/Reservas/NuevaCompra' $noCliente @{Cliente='cliente01'}
    Comprobar ($porNombre.Content.Contains('Nueva compra para el cliente cliente01')) 'Counter selecciona cliente existente por usuario'
    $correoCliente = [string](Sql "SELECT Email FROM AerolineaVIP_Identity.dbo.AspNetUsers WHERE UserName='cliente01'")
    $porCorreo = Enviar '/Reservas/NuevaCompra' $nueva @{Cliente=$correoCliente}
    Comprobar ($porCorreo.Content.Contains('Nueva compra para el cliente cliente01')) 'Counter selecciona cliente existente por correo'
    $vueloCompra = [int](Sql "SELECT TOP(1) VueloId FROM Vuelo WHERE FrecuenciaId=$frecuenciaId AND EstadoVueloCodigo='PROGRAMADO' AND FechaOperacion>CONVERT(date,GETDATE()) ORDER BY FechaOperacion")
    $origen = [int](Sql "SELECT r.AeropuertoOrigenId FROM Vuelo v JOIN Frecuencia f ON f.FrecuenciaId=v.FrecuenciaId JOIN Ruta r ON r.RutaId=f.RutaId WHERE v.VueloId=$vueloCompra")
    $destino = [int](Sql "SELECT r.AeropuertoDestinoId FROM Vuelo v JOIN Frecuencia f ON f.FrecuenciaId=v.FrecuenciaId JOIN Ruta r ON r.RutaId=f.RutaId WHERE v.VueloId=$vueloCompra")
    $resultados = Obtener "/Vuelos/Resultados?OrigenId=$origen&DestinoId=$destino&Fecha=$fechaViaje&Pasajeros=1"
    $asientos = Enviar '/Compra/Iniciar' $resultados @{clave="$vueloCompra";cantidad='1'}
    $libre = [regex]::Match($asientos.Content, 'data-numero="([0-9]+)" aria-pressed=').Groups[1].Value
    $pasajeros = Enviar '/Compra/Asientos' $asientos @{asientos=$libre}
    $resumen = Enviar '/Compra/Pasajeros' $pasajeros @{UsuarioCliente='admin01';'Personas[0].TipoDocumento'='PASAPORTE';'Personas[0].Documento'=($prefijo+'C');'Personas[0].Nombres'='Prueba';'Personas[0].Apellidos'='Counter'}
    Comprobar ($resumen.Content.Contains('Pasaporte ') -and -not $resumen.Content.Contains('PASAPORTE ')) 'Confirmar compra usa el nombre legible del documento'
    $clave = [regex]::Match($resumen.Content, 'name="clave" value="([^"]+)"').Groups[1].Value
    $finalCounter = Enviar '/Compra/Confirmar' $resumen @{clave=$clave;AceptaCondiciones='true';Destino='reserva'}
    $reservaCounter = [int](Sql "SELECT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento=@doc+'C'")
    Comprobar ($reservaCounter -gt 0 -and (Sql "SELECT COUNT(*) FROM Reserva r JOIN AerolineaVIP_Identity.dbo.AspNetUsers u ON u.Id=r.UsuarioId WHERE r.ReservaId=$reservaCounter AND u.UserName='cliente01'") -eq 1) 'Compra del counter queda con UsuarioId de cliente01 e ignora cliente adulterado en POST'
    Comprobar ($finalCounter.Content.Contains('Vence:')) 'Compra asistida pendiente muestra su vencimiento'
    $codigoCounter = [string](Sql "SELECT CodigoReserva FROM Reserva WHERE ReservaId=$reservaCounter")
    [void](Obtener "/Reservas/Asistencia?Buscar=$codigoCounter")
    [void](Enviar '/Reservas/NuevaCompra' $nueva @{Cliente='cliente01'})
    $pagarCounter = Obtener "/Compra/Pagar/$reservaCounter"
    $pagadaCounter = Enviar '/Compra/Pagar' $pagarCounter @{ReservaId=$reservaCounter;Marca='VISA';Numero='4111111111111111';Titular='Prueba Counter';Metodo='TARJETA_CREDITO';Vencimiento=(Get-Date).AddYears(1).ToString('MM/yy');CodigoSeguridad='123'}
    Comprobar ($pagadaCounter.Content.Contains('APROBADO') -and -not $pagadaCounter.Content.Contains('Vence:')) 'Counter paga desde asistencia y desaparece vencimiento al confirmar'
    $facturaCounter = [int](Sql "SELECT co.ComprobanteId FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId=$reservaCounter AND co.TipoComprobanteCodigo='FACTURA'")
    $pdfCounter = Obtener "/Comprobantes/Descargar/$facturaCounter"
    Comprobar ($pdfCounter.Headers['Content-Type'] -like 'application/pdf*') 'Counter emite comprobante de la reserva asistida'
    [void](Obtener "/Reservas/Asistencia?Buscar=$($codigos[2])")
    Comprobar (Denegado "/Comprobantes/Descargar/$facturaCounter") 'Nueva búsqueda no conserva acceso a reservas ajenas a la asistencia actual'
    $raiz = Split-Path $PSScriptRoot -Parent
    [void][Reflection.Assembly]::LoadFrom((Join-Path $raiz 'AerolineaVIP/bin/AerolineaVIP.dll'))
    $vueloPrueba = [AerolineaVIP.Models.Vuelo]::new()
    $vueloPrueba.FechaOperacion = [datetime]'2050-01-01'
    $vueloPrueba.EstadoVueloCodigo = 'PROGRAMADO'
    $vueloPrueba.Frecuencia = [AerolineaVIP.Models.Frecuencia]::new()
    $vueloPrueba.Frecuencia.HoraSalida = [timespan]'12:00:00'
    $asignacionPrueba = [AerolineaVIP.Models.ReservaPasajeroVuelo]::new()
    $asignacionPrueba.Vuelo = $vueloPrueba
    $asignacionPrueba.Reserva = [AerolineaVIP.Models.Reserva]::new()
    $asignacionPrueba.Reserva.EstadoReservaCodigo = 'CONFIRMADA'
    $asignacionPrueba.EstadoAsignacionCodigo = 'CONFIRMADA'
    Comprobar ($null -eq [AerolineaVIP.Models.CheckIn]::Elegibilidad($asignacionPrueba, $vueloPrueba.Salida.AddHours(-24))) 'Check-in abre exactamente 24 horas antes'
    Comprobar ($null -ne [AerolineaVIP.Models.CheckIn]::Elegibilidad($asignacionPrueba, $vueloPrueba.Salida.AddHours(-24).AddTicks(-1))) 'Check-in cerrado antes de las 24 horas'
    Comprobar ($null -ne [AerolineaVIP.Models.CheckIn]::Elegibilidad($asignacionPrueba, $vueloPrueba.Salida.AddHours(-1))) 'Check-in cierra exactamente una hora antes'
    Comprobar ($null -eq [AerolineaVIP.Models.CheckIn]::Elegibilidad($asignacionPrueba, $vueloPrueba.Salida.AddHours(-1).AddTicks(-1))) 'Check-in abierto justo antes del cierre'
    Entrar 'admin01' 'Admin#2026'
    Comprobar (Denegado '/Reservas/Asistencia') 'Asistencia exclusiva de EmpleadoCounter'
    $gestion = Obtener "/Reservas/Gestion/$($ids[3])"
    [void](Enviar '/Reservas/Cancelar' $gestion @{ReservaId=$ids[3];ConfirmarCancelacion='true'})
    Comprobar ((Sql "SELECT p.Importe FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId=$($ids[3]) AND p.EstadoPagoCodigo='REVERSADO'") -eq 100) 'Administrador reembolsa 100 % por vuelo cancelado'
    Write-Output 'FASE 4 verificada. Se eliminan únicamente sus datos temporales.'
}
finally {
    [void](Sql @'
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @reservas TABLE(Id int PRIMARY KEY);
INSERT @reservas SELECT DISTINCT a.ReservaId FROM ReservaPasajeroVuelo a JOIN Pasajero p ON p.PasajeroId=a.PasajeroId WHERE p.NumeroDocumento LIKE @doc+'%';
DELETE ch FROM CheckIn ch JOIN ReservaPasajeroVuelo a ON a.AsignacionId=ch.AsignacionId WHERE a.ReservaId IN(SELECT Id FROM @reservas);
DELETE co FROM Comprobante co JOIN Compra c ON c.CompraId=co.CompraId WHERE c.ReservaId IN(SELECT Id FROM @reservas);
DELETE p FROM Pago p JOIN Compra c ON c.CompraId=p.CompraId WHERE c.ReservaId IN(SELECT Id FROM @reservas);
DELETE Compra WHERE ReservaId IN(SELECT Id FROM @reservas);
DELETE ReservaPasajeroVuelo WHERE ReservaId IN(SELECT Id FROM @reservas);
DELETE Reserva WHERE ReservaId IN(SELECT Id FROM @reservas);
DELETE Pasajero WHERE NumeroDocumento LIKE @doc+'%';
COMMIT;
'@)
    if ($frecuenciaId -gt 0) { [void](Sql "DELETE Vuelo WHERE FrecuenciaId=$frecuenciaId; DELETE Frecuencia WHERE FrecuenciaId=$frecuenciaId;") }
    $conexion.Dispose()
}
