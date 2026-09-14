@echo off
setlocal
set "POSHC_PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
if exist "%SystemRoot%\Sysnative\WindowsPowerShell\v1.0\powershell.exe" set "POSHC_PS=%SystemRoot%\Sysnative\WindowsPowerShell\v1.0\powershell.exe"
"%POSHC_PS%" -NoProfile -ExecutionPolicy Bypass -File "%~dp0Extract-Deploy.ps1" %*
pause
endlocal
