# Scriptum Development Status

_Last updated: 2026-07-02_

## Current stage

Scriptum is in the **capture-first notebook archive MVP** stage. The app builds locally, launches on Windows, imports notebook page images, displays the original handwritten page as the primary artifact, lists imported pages in the sidebar, and lets users save page titles plus corrected digital text.

The product direction is no longer an in-app handwriting canvas. Scriptum is for importing or capturing pages from physical notebooks, preserving the original image, and turning the handwriting into usable digital text.

## Progress estimate

| Layer | Status | Notes |
| --- | --- | --- |
| Product concept | Strong | Private physical-notebook archive with digital transcription counterpart. |
| App shell | In progress | Warm/dark notebook archive shell with left library, center page artifact, and right transcription workspace. |
| Page import | Early MVP | Image import copies supported files into local app storage and displays the imported page. |
| Local persistence | Early MVP | SQLCipher-backed page payload storage saves imported page metadata, lists imported pages, and loads selected pages. |
| Build/local launch | Working locally | Restore and Release x64 build pass locally; the app launches from Windows. |
| Transcription workspace | Early MVP | Corrected text can be edited and saved; raw transcription and AI/OCR provider are not wired yet. |
| Notebook management | Early MVP | Sidebar page list works for imported pages; notebook/project groups are placeholders and rename/delete/tags are still missing. |
| Export/import | Not started | Markdown/PDF/image export and backup/restore flows are still missing. |
| Packaging/release | Not started | App icons, MSIX signing, installer/release pipeline, and versioning still need work. |

## Usable MVP target

A usable MVP should allow someone to:

1. Launch the Windows app reliably.
2. Import a photo or scan from a physical notebook.
3. Preserve and view the original page image.
4. Save and reload imported page metadata through encrypted local storage.
5. Browse all saved pages from the sidebar.
6. Run a first transcription pass on an imported page image.
7. Edit/correct the transcription.
8. Copy or format the transcription for LLMs, GitHub, docs, Codex, or project planning.
9. Search saved corrected transcriptions.

## Current blockers

1. **Normalized persistence**
   Current storage saves a whole page payload. Later versions should split notebooks, imported pages, page images, transcription records, tags, and search indexes into separate persisted entities.

2. **Transcription provider**
   The transcription workspace is present, but OCR/preprocessing and AI provider boundaries still need implementation.

3. **Production security**
   `SCRIPTUM_DATABASE_KEY` is better than an inline-only key, but a production app needs a secure key-management layer.

## Next engineering milestones

### Milestone 1: Page library

- List all imported pages from SQLCipher-backed storage. — complete
- Select prior pages from the sidebar. — complete
- Add page title editing. — complete
- Add corrected text editing. — complete
- Add delete behavior and richer metadata display.

### Milestone 2: Imported page persistence

- Normalize imported page, page image, and transcription records.
- Add schema versioning/migrations.
- Add corrupt-payload and missing-image handling.

### Milestone 3: Transcription MVP

- Add preprocessing service boundary for imported page images.
- Add `ITranscriptionProvider`.
- Add mock transcription provider for UI testing.
- Add Qwen or other vision-model provider.
- Store raw and corrected transcription text.

### Milestone 4: Useful personal notebook

- Search transcriptions.
- Add tags and project metadata.
- Export Markdown/PDF/images.
- Add backup/import/export.
