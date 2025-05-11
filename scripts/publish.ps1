Param($workspaceFolder)

dotnet publish "$workspaceFolder/ExpenseTracker.Web/ExpenseTracker.Web.csproj" /p:PublishProfile=FolderProfile;

Copy-Item -Path "$workspaceFolder/ExpenseTracker.Web/bin/Release/net8.0/win-x64/publish/*" -Destination "C:\web\expenses\" -Recurse -Force