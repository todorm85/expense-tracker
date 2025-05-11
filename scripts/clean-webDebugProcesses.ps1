# Find and kill the process using port 55161
$processId = (Get-NetTCPConnection -LocalPort 55161).OwningProcess
if ($processId) {
    $processId | ForEach-Object {
        Write-Host "Stopping process with ID: $_"
        Stop-Process -Id $_ -Force
    }
}
else { Write-Host "No process is using port 55161." }