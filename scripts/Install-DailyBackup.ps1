param(
    [Parameter(Mandatory)][string]$DeploymentPath,
    [Parameter(Mandatory)][PSCredential]$Credential,
    [string]$Time='02:00',
    [string]$TaskName='POS HC daily verified backup'
)
$ErrorActionPreference='Stop'
$deployment=(Resolve-Path -LiteralPath $DeploymentPath).Path
$assembly=Join-Path $deployment 'PosHCExternal.web.dll'
if(!(Test-Path -LiteralPath $assembly)){throw 'Publish the backend before installing the backup task.'}
if($assembly.Contains('"')){throw 'Deployment path cannot contain quotes.'}
$dotnet=(Get-Command dotnet).Source
$action=New-ScheduledTaskAction -Execute $dotnet -Argument ('"'+$assembly+'" --backup') -WorkingDirectory $deployment
$trigger=New-ScheduledTaskTrigger -Daily -At $Time
$settings=New-ScheduledTaskSettingsSet -StartWhenAvailable -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 5) -ExecutionTimeLimit (New-TimeSpan -Hours 2)
Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -User $Credential.UserName -Password $Credential.GetNetworkCredential().Password -Force | Select-Object TaskName,State
