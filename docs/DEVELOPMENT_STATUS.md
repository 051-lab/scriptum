# Scriptum Development Status

_Last updated: 2026-06-29_

## Current stage

Scriptum is in the **early MVP foundation** stage. The repository is no longer just a concept README: it now has a WinUI app shell, a notebook canvas MVP, vector ink models, local page persistence abstractions, and a SQLCipher-backed storage service. However, it is **not yet a working end-user application** because CI is still blocked at restore/build validation.

## Progress estimate

| Layer | Status | Notes |
| --- | --- | --- |
| Product concept | Strong | Premium local-first Windows notebook with handwritten note transcription. |
| App shell | In progress | WinUI app shell and main view are present. |
| Notebook canvas | Early MVP | Pointer-driven drawing canvas exists with undo, clear, save, and load-latest controls. |
| Ink data model | Early MVP | Editable vector stroke models exist. |
| Local persistence | Early MVP | JSON fallback exists; SQLCipher-backed page storage has been added. |
| Build/CI | Blocked | GitHub Actions currently fails during restore before compile errors can surface. |
| Encryption/key management | Prototype only | Database key can come from `SCRIPTUM_DATABASE_KEY`, but a production key vault/TPM flow is still needed. |
| Transcription pipeline | Not started | OpenCV preprocessing, handwriting image generation, Qwen-VL adapter, and correction UI are still missing. |
| Notebook management | Not started | Multiple notebooks, page lists, rename/delete, tags, and search are still missing. |
| Export/import | Not started | Markdown/PDF/image export and backup/restore flows are still missing. |
| Packaging/release | Not started | App icons, MSIX signing, installer/release pipeline, and versioning are still missing. |

## Honest completion estimate

Scriptum is approximately **20–25% of the way to a usable MVP**.

It is closer to **10–15% of the way to the full premium vision** described in the README, because handwriting transcription, AI cleanup, notebook organization, search, export, and production-grade security still need to be built.

## What should be considered a usable MVP?

A usable MVP should allow someone to:

1. Launch the Windows app reliably.
2. Create or open a notebook.
3. Draw handwritten notes on a page.
4. Save and reload editable vector ink.
5. Maintain multiple pages.
6. Run a first transcription pass on a page or selected region.
7. Edit/correct the transcription.
8. Search saved transcriptions.
9. Export a page to a useful format.

## Current blockers

1. **CI restore/build validation**  
   The app cannot be considered stable until GitHub Actions reaches a clean restore/build.

2. **Dependency alignment**  
   The project has moved from `Win2D.uwp` to `Microsoft.Graphics.Win2D`, and Windows App SDK has been updated, but restore is still failing and needs a precise fix.

3. **Real notebook architecture**  
   Current page persistence saves a whole page payload. That is acceptable for an early MVP, but later versions should split notebooks, pages, strokes, transcription jobs, and search indexes into separate persisted entities.

4. **Production security**  
   `SCRIPTUM_DATABASE_KEY` is better than an inline-only key, but a production app needs a secure key-management layer.

## Next engineering milestones

### Milestone 1: Buildable app

- Get `dotnet restore` passing in CI.
- Get `dotnet build` passing in CI.
- Fix XAML/code-behind compile issues surfaced by the build.
- Confirm the app launches locally on Windows.

### Milestone 2: Notebook MVP

- Add notebook and page models.
- Add a page list/sidebar.
- Add create/rename/delete page controls.
- Persist pages through SQLCipher storage.
- Reload saved pages reliably.

### Milestone 3: Transcription MVP

- Render page/region ink to an image buffer.
- Add preprocessing service boundary for OpenCV.
- Add AI provider boundary for Qwen-VL or other vision model providers.
- Store transcription results with page metadata.
- Add correction UI.

### Milestone 4: Useful personal notebook

- Search transcriptions.
- Add tags and page metadata.
- Export Markdown/PDF/images.
- Add backup/import/export.

## Current recommendation

Do not merge this PR as a production-ready feature yet. Keep using it as the stabilization/MVP branch until restore and build are green. Once CI is green, merge this branch into `main`, then start the notebook-management milestone in a smaller follow-up PR.
