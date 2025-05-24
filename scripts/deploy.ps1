Param($workspaceFolder, $destination)

Remove-Item -Path "$destination*" -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item -Path "$workspaceFolder/ExpenseTracker.Web/bin/Release/net8.0/win-x64/publish/*" -Destination $destination -Recurse -Force