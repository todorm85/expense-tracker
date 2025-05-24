$launchSettingsPath = "$PSScriptRoot\..\ExpenseTracker.Web\Properties\launchSettings.json"
$launchSettings = Get-Content $launchSettingsPath | ConvertFrom-Json
$applicationUrl = $launchSettings.profiles.Debug.applicationUrl
$ports = ($applicationUrl -split ';') -replace '.*:(\d+)', '$1'
foreach ($port in $ports) {
    $processId = (Get-NetTCPConnection -LocalPort $port).OwningProcess | Select-Object -Unique
    if ($processId) {
        $processId | ForEach-Object {
            Write-Host "Stopping process with ID: $_"
            Stop-Process -Id $_ -Force
        }
    }
    else { Write-Host "No process is using port $port." }
}