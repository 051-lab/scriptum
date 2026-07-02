# scriptum
A premium Windows app for capturing handwritten notebook pages, turning them into searchable digital notes, and preserving the original page image alongside transcription.

## Local development setup

Scriptum is a Windows desktop app built with C#, .NET 8, WinUI 3, Windows App SDK, Win2D, and SQLCipher-backed SQLite storage. The active MVP direction is importing or capturing handwritten notes from physical notebooks, not requiring users to handwrite notes inside the application. The active project and solution live at the repository root:

- `Scriptum.csproj`
- `Scriptum.sln`

Prerequisites:

- Windows 10/11 desktop environment suitable for WinUI 3 development.
- .NET 8 SDK.
- Visual Studio 2022 with Windows App SDK/WinUI workload support, or Windows PowerShell with the .NET SDK on `PATH`.

From Windows PowerShell at the repository root:

```powershell
git checkout main
git pull --ff-only
git checkout -B codex-local-stabilization

dotnet restore Scriptum.csproj -p:Platform=x64
dotnet build Scriptum.csproj --configuration Release --no-restore -p:Platform=x64
dotnet run --project Scriptum.csproj --configuration Release -p:Platform=x64
```

The local database is created on first save at:

```text
%LOCALAPPDATA%\Scriptum\scriptum.db
```

WSL can be useful for Git and text editing, but WinUI 3 launch and manual canvas testing should be run from the Windows desktop session so the app can create a real window and receive mouse, pen, or touch input.
