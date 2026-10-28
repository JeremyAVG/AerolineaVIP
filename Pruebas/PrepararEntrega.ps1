param([string]$Destino = (Join-Path $env:TEMP ('AerolineaVIP_Entrega-' + [guid]::NewGuid().ToString('N') + '.zip')))
$ErrorActionPreference = 'Stop'
$raiz = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent)).TrimEnd('\')
$destinoFinal = [IO.Path]::GetFullPath($Destino)
if (Test-Path -LiteralPath $destinoFinal) { throw 'El archivo de destino ya existe; use una ruta nueva para no sobrescribirlo.' }
$excluidas = @('.git', '.vs', 'bin', 'obj', 'packages')
function ArchivosFuente([string]$directorio) {
    foreach ($elemento in Get-ChildItem -LiteralPath $directorio -Force) {
        if (($elemento.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw "Revise el enlace antes de empaquetar: $($elemento.FullName)" }
        if ($elemento.PSIsContainer) {
            if ($excluidas -notcontains $elemento.Name) { ArchivosFuente $elemento.FullName }
        }
        elseif ($elemento.FullName -ne $destinoFinal) { $elemento }
    }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$archivos = @(ArchivosFuente $raiz | Sort-Object FullName)
$zip = [IO.Compression.ZipFile]::Open($destinoFinal, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($archivo in $archivos) {
        $nombre = $archivo.FullName.Substring($raiz.Length + 1).Replace('\', '/')
        [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $archivo.FullName, $nombre, [IO.Compression.CompressionLevel]::Optimal)
    }
}
finally { $zip.Dispose() }
$lectura = [IO.Compression.ZipFile]::OpenRead($destinoFinal)
$sha = [Security.Cryptography.SHA256]::Create()
try {
    if ($lectura.Entries.Count -ne $archivos.Count) { throw 'El número de archivos del ZIP no coincide con las fuentes.' }
    foreach ($entrada in $lectura.Entries) {
        $segmentos = $entrada.FullName.Split('/')
        foreach ($segmento in $segmentos) { if ($excluidas -contains $segmento) { throw "Carpeta excluida dentro del ZIP: $($entrada.FullName)" } }
        $fuente = Join-Path $raiz $entrada.FullName.Replace('/', '\')
        $flujoFuente = [IO.File]::OpenRead($fuente)
        $flujoZip = $entrada.Open()
        try {
            $hashFuente = [Convert]::ToBase64String($sha.ComputeHash($flujoFuente))
            $hashZip = [Convert]::ToBase64String($sha.ComputeHash($flujoZip))
            if ($hashFuente -ne $hashZip) { throw "El archivo del ZIP no coincide: $($entrada.FullName)" }
        }
        finally { $flujoFuente.Dispose(); $flujoZip.Dispose() }
    }
    foreach ($requerido in @('AerolineaVIP.slnx', 'AerolineaVIP/AerolineaVIP.csproj', 'BaseDatos/Script_BD_AerolineaVIP.sql', 'AerolineaVIP/Scripts/Ampliar_AspNetUsers.sql')) {
        if ($null -eq $lectura.GetEntry($requerido)) { throw "Falta $requerido en el ZIP" }
    }
    Write-Output "ZIP verificado: $destinoFinal"
    Write-Output "Archivos: $($lectura.Entries.Count); todos descomprimidos y comparados por SHA-256."
    Write-Output 'Contenido de primer nivel:'
    $lectura.Entries | ForEach-Object { $partes = $_.FullName.Split('/'); if ($partes.Length -gt 1) { $partes[0] + '/' } else { $partes[0] } } | Sort-Object -Unique | Write-Output
}
finally { $sha.Dispose(); $lectura.Dispose() }
