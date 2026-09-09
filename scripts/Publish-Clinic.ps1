param([string]$NodeExecutable='node',[string]$NpmCli='',[switch]$SkipDependencyInstall)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$client=Join-Path $root 'Pos-HC/PosHCExternal.web/ClientApp'
$destination=Join-Path $root 'artifacts/clinic'
Push-Location $client
try {
    if(!$SkipDependencyInstall) {
      if(!$NpmCli) {
        $npmCommand=(Get-Command npm.cmd -ErrorAction Stop).Source
        $NpmCli=Join-Path (Split-Path $npmCommand -Parent) 'node_modules/npm/bin/npm-cli.js'
      }
      if(!(Test-Path -LiteralPath $NpmCli)){throw 'Supply -NpmCli with the path to npm-cli.js.'}
      & $NodeExecutable $NpmCli ci
      if($LASTEXITCODE -ne 0){throw 'Dependency installation failed.'}
    }
    & $NodeExecutable node_modules/@angular/cli/bin/ng.js build
    if($LASTEXITCODE -ne 0){throw 'Client build failed.'}
} finally {Pop-Location}
& dotnet publish (Join-Path $root 'Pos-HC/PosHCExternal.web/PosHCExternal.web.csproj') -c Release -o $destination
if($LASTEXITCODE -ne 0){throw 'Backend publish failed.'}
$wwwroot=Join-Path $destination 'wwwroot'
New-Item -ItemType Directory -Force $wwwroot | Out-Null
Copy-Item -Path (Join-Path $client 'dist/ClientApp/browser/*') -Destination $wwwroot -Recurse -Force
Write-Output "Published clinic application: $destination"
