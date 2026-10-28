param([switch]$GuardarInventario)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
$web = Join-Path $raiz 'AerolineaVIP'
$vistas = @(Get-ChildItem $web -Recurse -File -Filter *.cshtml | Where-Object { $_.FullName -notmatch '\\(bin|obj|packages|\.vs|\.git)\\' } | Sort-Object FullName)
$errores = [Collections.Generic.List[string]]::new()
$inventario = [Collections.Generic.List[string]]::new()
$inventario.Add('# Inventario de revisión Razor')
$inventario.Add('')
$inventario.Add('Todas las vistas fuente se revisan, incluidas parciales, plantillas de Identity y área Admin. Los if/foreach de presentación, controles por rol y formatos de fecha/moneda no son cálculos de negocio. Importes, porcentajes, estados, elegibilidad, consultas y reglas se resuelven fuera de las vistas.')
$inventario.Add('')
$inventario.Add('| Vista | Modelo | Revisión |')
$inventario.Add('|---|---|---|')
$tipadas = 0
[xml]$proyecto = Get-Content (Join-Path $web 'AerolineaVIP.csproj')
$contenidos = @($proyecto.Project.ItemGroup.Content | ForEach-Object { $_.Include })
foreach ($vista in $vistas) {
    $relativa = $vista.FullName.Substring($raiz.Length + 1).Replace('\', '/')
    $texto = [IO.File]::ReadAllText($vista.FullName)
    $sinComentarios = [regex]::Replace($texto, '(?s)@\*.*?\*@|<!--.*?-->', '')
    $modelo = [regex]::Match($texto, '(?m)^@model\s+([^\r\n]+)')
    $nombreModelo = 'Sin modelo (layout/estática)'
    if ($modelo.Success) {
        $tipadas++
        $nombreModelo = $modelo.Groups[1].Value.Trim()
        if ($nombreModelo -match '(?:^|<|,\s*)[A-Za-z_]\w*\.') { $errores.Add("Modelo con namespace en $relativa") }
        $prefijo = $texto.Substring(0, $modelo.Index).TrimEnd()
        if ($prefijo -notmatch '(?m)^@using\s+[A-Za-z_][A-Za-z0-9_.]*$') { $errores.Add("Falta @using antes del modelo en $relativa") }
        if ($texto -match '@using\s+[^\r\n]+@model') { $errores.Add("Directivas en una línea en $relativa") }
    }
    if ($sinComentarios -match 'GetOwinContext|GetExternalAuthenticationTypes|new\s+(?:AerolineaVIPEntities|ApplicationDbContext|SelectList)|\.Database\.|\.(?:Sum|Average|Where|Select|GroupBy|OrderBy|ExecuteSqlCommand|SqlQuery)\s*\(') {
        $errores.Add("Consulta, preparación de datos o agregado en vista $relativa")
    }
    if ($sinComentarios -match '@\([^\r\n]*(?:\s[*/%]\s|\s\+\s[^\"\r\n]*\d)') { $errores.Add("Revisar aritmética en $relativa") }
    $inclusion = $vista.FullName.Substring($web.Length + 1)
    if ($contenidos -notcontains $inclusion) { $errores.Add("Vista ausente del csproj: $relativa") }
    $bytes = [IO.File]::ReadAllBytes($vista.FullName)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 239 -or $bytes[1] -ne 187 -or $bytes[2] -ne 191) { $errores.Add("Vista sin BOM UTF-8: $relativa") }
    $inventario.Add('| ' + $relativa + ' | `' + $nombreModelo + '` | Conforme |')
}
Add-Type -AssemblyName System.Data
[xml]$config = Get-Content (Join-Path $web 'Web.config')
foreach ($nombre in @('DefaultConnection', 'AerolineaVIPEntities')) {
    $cadena = ($config.configuration.connectionStrings.add | Where-Object name -eq $nombre).connectionString
    if ($nombre -eq 'AerolineaVIPEntities') { $cadena = [regex]::Match($cadena, 'provider connection string="([^"]+)"').Groups[1].Value }
    $conexion = [Data.SqlClient.SqlConnectionStringBuilder]::new($cadena)
    if ($conexion.DataSource -ne '.' -or $conexion.UserID -ne 'sa' -or $conexion.Password -ne '123' -or $conexion.IntegratedSecurity) { $errores.Add("Conexión de entrega incorrecta: $nombre") }
    Write-Output "OK: conexión $nombre verificada"
}
foreach ($script in @('BaseDatos/Script_BD_AerolineaVIP.sql', 'AerolineaVIP/Scripts/Ampliar_AspNetUsers.sql')) {
    if (-not (Test-Path (Join-Path $raiz $script))) { $errores.Add("Falta script $script") }
}
if ($errores.Count) { throw ($errores -join "`n") }
if ($GuardarInventario) {
    [IO.File]::WriteAllLines((Join-Path $raiz 'Documentacion/Inventario_Vistas.md'), $inventario, [Text.UTF8Encoding]::new($true))
}
Write-Output "OK: $($vistas.Count) vistas revisadas ($tipadas con modelo), directivas y csproj conformes; sin consultas ni agregados en vistas."
Write-Output 'OK: scripts SQL de entrega presentes'
