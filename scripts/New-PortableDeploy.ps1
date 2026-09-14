[CmdletBinding()]
param([string]$Version = (Get-Date -Format 'yyyyMMdd-HHmmss'))

$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$') { throw 'Use letters, numbers, dots, dashes or underscores for the version.' }
$repository = Split-Path $PSScriptRoot -Parent
$packageRoot = Join-Path $repository 'artifacts-deploy'
$outputRoot = Join-Path $repository "portable-deploy\$Version"
$archiveRoot = Join-Path $repository 'artifacts\portable'
$archiveName = "ClinicSol-$Version-offline.zip"
$archivePath = Join-Path $archiveRoot $archiveName
if ((Test-Path -LiteralPath $outputRoot) -or (Test-Path -LiteralPath $archivePath)) {
    throw 'This version already exists. Choose a new -Version; existing archives are never overwritten.'
}
& (Join-Path $packageRoot 'Deploy.ps1') -ValidateOnly
if (!$?) { throw 'Source deployment package validation failed.' }

$packageManifest = Get-Content -LiteralPath (Join-Path $packageRoot 'package-manifest.json') -Raw | ConvertFrom-Json
$paths = @($packageManifest.Files.Path) + 'package-manifest.json'
$seen = @{}
foreach ($relative in $paths) {
    $fullPath = [IO.Path]::GetFullPath((Join-Path $packageRoot $relative))
    if (!$fullPath.StartsWith($packageRoot + '\', [StringComparison]::OrdinalIgnoreCase) -or
        $relative -match '(?i)(^|[\\/])(\.vs|\.git|\.local|logs|backups)([\\/]|$)|appsettings\.Production\.json$|\.(bak|mdf|ldf)$') {
        throw "Not suitable for a reusable deployment archive: $relative"
    }
    if ($seen.ContainsKey($relative)) { throw "Duplicate package path: $relative" }
    $seen[$relative] = $true
}
$settings = Get-Content -LiteralPath (Join-Path $packageRoot 'app\appsettings.json') -Raw | ConvertFrom-Json
if ($settings.Setup.Token -or $settings.ConnectionStrings.DefaultConnection) {
    throw 'The reusable appsettings.json must not contain an instance connection string or setup token.'
}

New-Item -ItemType Directory -Path $archiveRoot -Force | Out-Null
New-Item -ItemType Directory -Path $outputRoot | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
Write-Host 'Compressing the validated offline deployment package...'
$zip = [IO.Compression.ZipFile]::Open($archivePath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($relative in $paths) {
        $entryName = 'artifacts-deploy/' + $relative.Replace('\', '/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $packageRoot $relative),
            $entryName, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $zip.Dispose() }

$partSize = 48MB
$buffer = New-Object byte[] (1MB)
$parts = @()
$source = [IO.File]::OpenRead($archivePath)
try {
    $number = 0
    while ($source.Position -lt $source.Length) {
        $number++
        $name = '{0}.part{1:D3}' -f $archiveName, $number
        $partPath = Join-Path $outputRoot $name
        $partStream = [IO.File]::Open($partPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
        try {
            $remaining = $partSize
            while ($remaining -gt 0) {
                $read = $source.Read($buffer, 0, [int][Math]::Min($buffer.Length, $remaining))
                if ($read -eq 0) { break }
                $partStream.Write($buffer, 0, $read)
                $remaining -= $read
            }
        } finally { $partStream.Dispose() }
        $parts += [pscustomobject]@{ Name = $name; Length = (Get-Item -LiteralPath $partPath).Length; SHA256 = (Get-FileHash -LiteralPath $partPath -Algorithm SHA256).Hash }
    }
} finally { $source.Dispose() }
$manifest = [ordered]@{
    Version = $Version
    CreatedUtc = [DateTime]::UtcNow.ToString('o')
    Archive = $archiveName
    Length = (Get-Item -LiteralPath $archivePath).Length
    SHA256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    PackageManifestSHA256 = (Get-FileHash -LiteralPath (Join-Path $packageRoot 'package-manifest.json') -Algorithm SHA256).Hash
    Parts = $parts
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $outputRoot 'archive-manifest.json') -Encoding UTF8
foreach ($name in @('Extract-Deploy.cmd', 'Extract-Deploy.ps1')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot "portable\$name") -Destination (Join-Path $outputRoot $name)
}
Set-Content -LiteralPath (Join-Path $outputRoot '.gitignore') -Value '/expanded/' -Encoding ASCII
Set-Content -LiteralPath (Join-Path $outputRoot '.gitattributes') -Value '*.zip.part* binary' -Encoding ASCII
$readme = @"
# ClinicSol $Version offline deployment

This folder contains the COMPLETE compressed deployment package in $($parts.Count) parts, at most 48 MiB each. Commit this entire folder to GitHub using Git. Git LFS and third-party archive tools are not required. Do not commit the reconstructed ZIP or expanded folder.

1. Clone/download this folder, keeping all parts, scripts and archive-manifest.json together. If downloading GitHub's repository ZIP, extract it first.
2. Double-click Extract-Deploy.cmd. It verifies every part, rebuilds the ZIP, extracts it into expanded/artifacts-deploy, and validates the deployment files. Allow roughly 1.5 GB of free disk space for reconstruction and extraction, plus installation/database space.
3. Inside expanded/artifacts-deploy, read README.md and double-click Deploy.cmd. Approve administrator elevation, enter the HTTP port, and select the default local SQL Express option if SQL Server is absent.
4. Open the printed URL and create the first administrator with the generated setup token.

Included: published backend, Angular in wwwroot, SQL Server Express offline installer, .NET 8 IIS Hosting Bundle, deployment scripts, maintenance scripts, English/Arabic guides, and SHA256 manifests. No application build, Node, .NET SDK or dependency download is needed on the target PC. Windows x64 with IIS/WAS/Management Console is required; enabling Windows features may require Windows media or network access.

Every Deploy.cmd run creates a NEW site and database. It does not upgrade an existing instance or preserve its data in a new instance. Reuse the extracted folder for additional fresh deployments; do not rerun the extractor over an existing expanded folder.

This is a portable installer package, not an application that runs without IIS/SQL installation. Intranet HTTP access follows the deployment README. Checksums detect accidental corruption, not tampering with both data and manifests.

To verify parts only: powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Extract-Deploy.ps1 -ValidateOnly

Full ZIP SHA256: $($manifest.SHA256)

For a future version, refresh artifacts-deploy, then run scripts/New-PortableDeploy.ps1 from the repository with a new -Version. Keep the parts from different versions in separate folders. Frequent binary versions enlarge Git history; GitHub Releases are suitable for retaining full ZIPs outside normal Git history.
"@
Set-Content -LiteralPath (Join-Path $outputRoot 'README.md') -Value $readme -Encoding UTF8
Write-Host "Full ZIP: $archivePath"
Write-Host "Git-ready folder: $outputRoot"
Write-Host "ZIP size: $([Math]::Round($manifest.Length / 1MB, 2)) MiB; parts: $($parts.Count)"
