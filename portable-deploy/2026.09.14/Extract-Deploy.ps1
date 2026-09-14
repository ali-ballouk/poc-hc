[CmdletBinding()]
param([switch]$ValidateOnly, [string]$OutputDirectory)

$ErrorActionPreference = 'Stop'
try {
    $manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'archive-manifest.json') -Raw | ConvertFrom-Json
    if (!$manifest.Parts -or $manifest.Archive -notmatch '^ClinicSol-[A-Za-z0-9._-]+\.zip$') {
        throw 'Invalid archive manifest.'
    }
    $total = [long]0
    $index = 0
    foreach ($part in $manifest.Parts) {
        $index++
        $expectedName = '{0}.part{1:D3}' -f $manifest.Archive, $index
        if ($part.Name -ne $expectedName) { throw "Invalid archive part order: $($part.Name)" }
        $path = Join-Path $PSScriptRoot $part.Name
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing $($part.Name). Copy every part into this folder." }
        if ((Get-Item -LiteralPath $path).Length -ne $part.Length -or
            (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $part.SHA256) {
            throw "Damaged archive part: $($part.Name). Download or copy it again."
        }
        $total += $part.Length
    }
    if ($total -ne $manifest.Length) { throw 'Archive length does not match the manifest.' }
    Write-Host "Verified all $index archive parts."
    if ($ValidateOnly) { return }

    if (!$OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'expanded' }
    $destination = [IO.Path]::GetFullPath($OutputDirectory)
    if (Test-Path -LiteralPath $destination) {
        throw "Output already exists: $destination. Use its extracted package, or choose a new -OutputDirectory. Nothing was overwritten."
    }
    New-Item -ItemType Directory -Path $destination | Out-Null
    $zipPath = Join-Path $destination $manifest.Archive
    $output = [IO.File]::Open($zipPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    try {
        foreach ($part in $manifest.Parts) {
            $input = [IO.File]::OpenRead((Join-Path $PSScriptRoot $part.Name))
            try { $input.CopyTo($output) } finally { $input.Dispose() }
        }
    } finally { $output.Dispose() }
    if ((Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash -ne $manifest.SHA256) {
        throw 'Reassembled ZIP verification failed.'
    }
    Write-Host 'ZIP verified. Extracting the offline deployment package...'
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        foreach ($entry in $archive.Entries) {
            $target = [IO.Path]::GetFullPath((Join-Path $destination $entry.FullName))
            if (!$target.StartsWith($destination + '\', [StringComparison]::OrdinalIgnoreCase)) {
                throw "Unsafe archive path: $($entry.FullName)"
            }
        }
    } finally { $archive.Dispose() }
    [IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $destination)
    $deployer = Join-Path $destination 'artifacts-deploy\Deploy.ps1'
    & $deployer -ValidateOnly
    if (!$?) { throw 'Extracted deployment package validation failed.' }
    Write-Host "`nReady: $(Join-Path $destination 'artifacts-deploy\Deploy.cmd')"
    Write-Host 'Double-click Deploy.cmd to create a NEW instance and select its HTTP port.'
    Write-Host 'This extractor does not install software or modify IIS or a database.'
} catch {
    Write-Error $_
    exit 1
}
