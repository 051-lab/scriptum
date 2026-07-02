# Scriptum Prioritized Development Todo

_Last updated: 2026-07-02_

Scriptum is now in the **capture-first notebook archive MVP** stage. CI restore and CI build are green. The product direction is to capture or import handwritten notes from physical notebooks, preserve the original page image, and convert those notes into searchable digital text. The drawing-first prototype has been replaced by an import-first notebook page shell.

## Priority key

- **P0**: Blocks the app from being usable or trusted.
- **P1**: Required for a usable MVP.
- **P2**: Important for daily workflow usefulness.
- **P3**: Polish, scale, release, and advanced workflow expansion.

---

## Phase 0: Build stability and repo hygiene

### P0.1 Fix CI restore failure — complete

- [x] Capture restore diagnostics from GitHub Actions.
- [x] Fix SQLCipher package reference by using `SQLitePCLRaw.bundle_e_sqlcipher`.
- [x] Update `Microsoft.Windows.SDK.BuildTools` to satisfy Windows App SDK requirements.
- [x] Remove unused packages from restore surface.
- [x] Confirm `dotnet restore` passes in CI.

### P0.2 Fix compile/build errors — complete

- [x] Capture build diagnostics from GitHub Actions.
- [x] Remove duplicate explicit `PRIResource` include for `Strings/en-US/Resources.resw`.
- [x] Confirm `dotnet build` passes in CI.

### P0.3 Confirm local Windows launch — complete

- [x] Clone or pull `main` locally.
- [x] Restore packages locally.
- [x] Build from Visual Studio or Windows terminal.
- [x] Launch the app.
- [x] Confirm `MainWindow` opens without crashing.
- [x] Confirm `MainView` displays the import-first notebook archive shell.
- [x] Confirm current UI opens for further MVP work.

### P0.4 Clean project structure

- [ ] Confirm `Scriptum.sln` works locally.
- [ ] Confirm `.gitignore` covers build artifacts, packages, logs, user files, and secrets.
- [ ] Confirm folders are organized around `Models`, `ViewModels`, `Views`, `Services`, `Data`, `Assets`, and `docs`.
- [ ] Add local development setup instructions to the README.
- [ ] Reduce duplicate GitHub Actions notifications by running CI on PRs and `main` only.

---

## Phase 1: First physical-notebook capture loop

Goal: create the smallest useful Scriptum experience: import or capture a page from a physical notebook, save it locally, close and reopen the app, load the latest page, and prepare the page image for transcription.

### P1.1 Add page capture/import

- [x] Add Import Image command.
- [x] Support common notebook photo/image formats.
- [x] Copy imported page images into local app storage.
- [x] Display the imported page image in the main view.
- [x] Show basic image metadata: filename, imported timestamp, dimensions.
- [ ] Add clear failure messages for unsupported or unreadable files.

### P1.2 Stabilize physical page model

- [x] Add physical notebook page model fields: ID, title, source image path, created, updated, imported timestamp.
- [ ] Add image checksum or content identity for duplicate detection.
- [ ] Add model versioning.
- [ ] Prepare metadata for future notebook grouping, transcription, tags, and export.

### P1.3 Prove SQLCipher page save/load

- [ ] Confirm encrypted database opens.
- [ ] Confirm physical page table is created.
- [ ] Save imported page metadata from the UI.
- [ ] Close and reopen the app.
- [ ] Load the latest page.
- [ ] Confirm the page image and metadata restore correctly.
- [ ] Add clear database failure messages.
- [ ] Add corrupt-payload handling.

### P1.4 Add basic page lifecycle

- [ ] Add New Page command.
- [ ] Add page title field.
- [ ] Add rename behavior.
- [ ] Add delete behavior with confirmation.
- [ ] Add updated timestamp display.
- [ ] Add dirty-state tracking.
- [ ] Add save status/confirmation.

### P1.5 Add notebook model

- [ ] Add `Notebook` model.
- [ ] Add notebook ID, title, created, and updated fields.
- [ ] Connect pages to notebooks.
- [ ] Create default notebook on first launch.
- [ ] Expose current notebook state in the ViewModel.

---

## Phase 2: MVP navigation and daily usability

- [ ] Add sidebar page navigation.
- [ ] Add import queue or recent imports list.
- [ ] Add basic image rotate/crop workflow for photographed notebook pages.
- [ ] Add page title editing.
- [ ] Add delete behavior with confirmation.
- [ ] Add keyboard shortcuts: Import, Save, New Page, Delete.

---

## Phase 3: Data durability

- [ ] Normalize persistence into notebooks, pages, strokes/snapshots, transcriptions, tags, and search index tables.
- [ ] Add schema versioning and migrations.
- [ ] Add backup/export/import safety.
- [ ] Replace development key fallback with production key-management path.
- [ ] Add storage tests.

---

## Phase 4: Transcription MVP

- [ ] Prepare imported page image for OCR/transcription.
- [ ] Add preprocessing service boundary.
- [ ] Add OpenCVSharp adapter or equivalent preprocessing adapter.
- [ ] Add `ITranscriptionProvider` abstraction.
- [ ] Add mock/local transcription provider for testing.
- [ ] Add Qwen-VL provider boundary.
- [ ] Add transcription UI and correction panel.
- [ ] Persist raw and corrected transcription text.

---

## Phase 5: Development workflow usefulness

- [ ] Add full-text search over corrected transcriptions.
- [ ] Add tags and project/repo association.
- [ ] Add developer note templates.
- [ ] Add Markdown export.
- [ ] Add structured handoff exports for GitHub issues, PR checklists, docs, Codex, ChatGPT, and Qwen Coder.

---

## Phase 6: Premium notebook feel

- [ ] Add zoom and pan.
- [ ] Add page image viewer polish.
- [ ] Add before/after preprocessing preview.
- [ ] Add optional annotation mode for marking imported pages.
- [ ] Add page thumbnails.
- [ ] Add command palette.

---

## Phase 7: Import/export and release readiness

- [ ] Export page images.
- [ ] Export PDF pages and notebooks.
- [ ] Import PDF/Markdown notes.
- [ ] Replace placeholder app assets.
- [ ] Configure MSIX packaging and signing.
- [ ] Add release workflow, artifacts, changelog, and version tags.
- [ ] Add diagnostics and local crash/error logs.

---

## Immediate execution order

1. [x] Fix CI restore failure.
2. [x] Fix compile/build errors.
3. [x] Confirm local Windows launch.
4. [x] Replace the drawing-first surface with an import/capture-first page view.
5. [x] Save imported page metadata and image path through SQLCipher-backed storage.
6. [x] Load the latest imported page.
7. [ ] Add multiple pages.
8. [ ] Add sidebar page navigation.
9. [ ] Add notebook model.
10. [ ] Add image preprocessing pipeline.
11. [ ] Add transcription provider boundary.
12. [ ] Add transcription UI.
13. [ ] Add search.
14. [ ] Add Markdown export.
15. [ ] Add MSIX packaging.

## Working rule

Every major feature should have a clear done condition before moving to the next phase. Scriptum should grow in small, testable increments so it becomes dependable enough to support real development work.
