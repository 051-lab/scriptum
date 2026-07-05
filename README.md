# Scriptum

Scriptum is a local-first Windows desktop app for turning real physical handwritten notebook pages into preserved, searchable, useful digital development notes.

The intended workflow is capture-first:

1. Import or capture an image of a physical notebook page.
2. Preserve the original page image locally.
3. Store page metadata and local image paths in encrypted local storage.
4. Prepare the page image for OCR or vision transcription.
5. Transcribe handwriting into editable/searchable text.
6. Preserve both the original image and the transcription for later development work.

The in-app drawing canvas is not the main MVP direction. It should be treated as a possible future annotation feature over imported page images.

See `docs/PRODUCT_DIRECTION.md`, `docs/DEVELOPMENT_STATUS.md`, and `docs/TODO.md` for current planning and implementation priorities.

## Local Development Setup

Scriptum is built with C#, .NET 8, WinUI 3, Windows App SDK, Win2D, and SQLCipher-backed SQLite storage. The active project and solution live at the repository root:

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

dotnet restore Scriptum.csproj -p:Platform=x64
dotnet build Scriptum.csproj --configuration Release --no-restore -p:Platform=x64
dotnet run --project Scriptum.csproj --configuration Release -p:Platform=x64
```

The local database is created on first save at:

```text
%LOCALAPPDATA%\Scriptum\scriptum.db
```

### Optional Qwen vision transcription

By default, Scriptum uses a mock transcription provider so local development does not require network credentials. To test real handwriting transcription with an OpenAI-compatible Qwen vision endpoint, set these variables in Windows PowerShell before launching:

```powershell
$env:SCRIPTUM_TRANSCRIPTION_PROVIDER = "qwen"
$env:SCRIPTUM_QWEN_ENDPOINT = "https://your-qwen-compatible-host/v1"
$env:SCRIPTUM_QWEN_MODEL = "your-image-capable-qwen-model"
$env:QWEN_API_KEY = "your-api-key"

dotnet run --project Scriptum.csproj --configuration Release -p:Platform=x64
```

`SCRIPTUM_QWEN_ENDPOINT` can be either a base `/v1` URL or a full `/chat/completions` URL. The app sends the prepared notebook page image to the provider and saves the returned text as the page's raw transcription.

### Optional Windows OCR transcription

For a no-key OCR baseline, use the Windows OCR provider:

```powershell
$env:SCRIPTUM_TRANSCRIPTION_PROVIDER = "windows-ocr"

dotnet run --project Scriptum.csproj --configuration Release -p:Platform=x64
```

Windows OCR is local and credential-free, but handwritten notebook accuracy depends heavily on the handwriting, lighting, contrast, crop, and installed Windows OCR language support. Vision-model transcription remains the stronger path for messy handwritten pages.

WSL can be useful for Git and text editing, but WinUI 3 launch and manual UI testing should be run from the Windows desktop session so the app can create a real window and receive mouse, pen, or touch input.
