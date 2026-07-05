# Scriptum Prioritized Development Todo

_Last updated: 2026-07-05_

Scriptum is now in the **capture-first notebook archive MVP** stage. CI restore and CI build are green, and the repository has been flattened so the WinUI project lives at the repository root.

The product direction has been corrected: Scriptum is not primarily for handwriting notes directly inside the app. The core workflow is to import or capture real physical notebook pages, preserve the original page image, transcribe handwriting into editable/searchable text, and make those notes useful for later development work.

See `docs/PRODUCT_DIRECTION.md` for the source-of-truth product direction.

## Priority Key

- **P0**: Blocks the app from being usable or trusted.
- **P1**: Required for the physical-notebook capture MVP.
- **P2**: Important for daily development workflow usefulness.
- **P3**: Polish, scale, release, and advanced workflow expansion.

---

## Phase 0: Build Stability And Repo Hygiene

### P0.1 Fix CI restore failure - complete

- [x] Capture restore diagnostics from GitHub Actions.
- [x] Fix SQLCipher package reference by using `SQLitePCLRaw.bundle_e_sqlcipher`.
- [x] Update `Microsoft.Windows.SDK.BuildTools` to satisfy Windows App SDK requirements.
- [x] Remove unused packages from restore surface.
- [x] Confirm `dotnet restore` passes in CI.

### P0.2 Fix compile/build errors - complete

- [x] Capture build diagnostics from GitHub Actions.
- [x] Remove duplicate explicit `PRIResource` include for `Strings/en-US/Resources.resw`.
- [x] Confirm `dotnet build` passes in CI.

### P0.3 Confirm local Windows launch - complete

- [x] Clone or pull `main` locally.
- [x] Restore packages locally.
- [x] Build from Visual Studio or Windows terminal.
- [x] Launch the app.
- [x] Confirm `MainWindow` opens without crashing.
- [x] Confirm `MainView` displays the import-first notebook archive shell.
- [x] Confirm current UI opens for further MVP work.

### P0.4 Clean project structure - complete

- [x] Move project files from nested `Scriptum/` folder to repository root.
- [x] Update CI to build `Scriptum.csproj` from repository root.
- [x] Confirm `Scriptum.sln` works locally.
- [x] Confirm `.gitignore` covers build artifacts, packages, logs, user files, and secrets.
- [x] Confirm folders are organized around `Models`, `ViewModels`, `Views`, `Services`, `Data`, `Assets`, and `docs`.
- [x] Add local development setup instructions to the README.
- [x] Reduce duplicate GitHub Actions notifications by running CI on PRs and `main` only.

---

## Phase 1: First Physical-Notebook Capture Loop

Goal: create the smallest useful Scriptum experience: import or capture a page from a physical notebook, save it locally, close and reopen the app, load the latest page, and prepare the page image for transcription.

### P1.1 Add page capture/import

- [x] Add Import Image command.
- [x] Support common notebook photo/image formats.
- [x] Copy imported page images into local app storage.
- [x] Display the imported page image in the main view.
- [x] Show basic image metadata: filename, imported timestamp, dimensions.
- [ ] Add clear failure messages for unsupported or unreadable files.
- [ ] Add camera/scanner capture flow if feasible.

### P1.2 Stabilize physical page model

- [x] Add physical notebook page model fields: ID, title, source image path, created, updated, imported timestamp.
- [x] Add `Notebook` model.
- [x] Connect pages to the default notebook.
- [ ] Add image checksum or content identity for duplicate detection.
- [ ] Add model versioning.
- [ ] Prepare metadata for future notebook grouping, transcription, tags, and export.

### P1.3 Prove SQLCipher page save/load

- [ ] Confirm encrypted database opens.
- [ ] Confirm physical page table is created.
- [x] Save imported page metadata from the UI.
- [x] Load the latest page.
- [x] Confirm the page image and metadata restore correctly.
- [x] List imported pages from encrypted local storage.
- [x] Select an imported page from the sidebar.
- [ ] Add clear database failure messages.
- [x] Add corrupt-payload handling.
- [x] Add missing imported-image recovery state.

### P1.4 Add basic page lifecycle

- [x] Add New Page command.
- [x] Add page title field.
- [x] Add rename behavior.
- [x] Add delete behavior with confirmation.
- [x] Add updated timestamp display.
- [x] Add dirty-state tracking.
- [x] Add unsaved-edit guard before page switching/reset actions.
- [x] Add save status/confirmation.
- [ ] Add full notebook grouping and selection.

---

## Phase 2: MVP Navigation And Daily Usability

- [x] Add sidebar page navigation.
- [x] Add basic sidebar search over titles, filenames, notebooks, and saved transcription text.
- [x] Preserve sidebar selection and refresh edited titles.
- [x] Add basic image rotation workflow for photographed notebook pages.
- [x] Add keyboard shortcuts: Import, Save, Load Latest, Delete, Transcribe.
- [x] Add New Page command and shortcut.
- [ ] Add import queue or recent imports list.
- [ ] Add basic image crop workflow for photographed notebook pages.
- [ ] Add zoom and pan.
- [ ] Add page image viewer polish.
- [ ] Add before/after preprocessing preview.
- [ ] Add page thumbnails.
- [ ] Add command palette.

---

## Phase 3: Transcription MVP

- [x] Prepare imported page image for OCR/transcription.
- [x] Add preprocessing service boundary.
- [x] Add prepared-image storage path and transform metadata placeholders.
- [x] Add disabled OpenCVSharp adapter shell or equivalent preprocessing adapter.
- [x] Add `ITranscriptionProvider` abstraction.
- [x] Add mock/local transcription provider for testing.
- [x] Add Qwen-VL provider boundary.
- [x] Add transcription UI and correction panel.
- [x] Persist corrected transcription text.
- [x] Persist raw transcription text separately from corrected text.
- [x] Add copy/clear controls for local transcription text.
- [ ] Implement and validate real OpenCVSharp preprocessing.
- [ ] Implement and validate real Qwen-VL provider.
- [ ] Add transcription provider metadata and processing timestamps.
- [ ] Add transcription failure persistence without leaking secrets.

---

## Phase 4: Search And Development Workflow Usefulness

- [x] Add basic sidebar search over titles, filenames, notebooks, and saved transcription text.
- [x] Add Markdown export for the current page corrected text.
- [x] Add basic LLM-formatted clipboard handoff.
- [ ] Add full-text search index over corrected transcriptions.
- [ ] Add tags and project/repo association.
- [ ] Add developer note templates.
- [ ] Add batch Markdown export.
- [ ] Add structured handoff exports for GitHub issues, PR checklists, docs, Codex, ChatGPT, and Qwen Coder.

---

## Phase 5: Annotation And Canvas As Secondary Feature

The drawing canvas is not the MVP driver. It can become useful later as an annotation layer over imported notebook images.

- [ ] Add optional annotation overlay over imported images.
- [ ] Save annotations separately from the original image.
- [ ] Add eraser/selection tools for annotations.
- [ ] Add annotation visibility toggle.
- [ ] Keep original image immutable.

---

## Phase 6: Data Durability And Security

- [ ] Normalize persistence into notebooks, imported pages, page images, transcriptions, tags, and search index tables.
- [ ] Add schema versioning and migrations.
- [ ] Add backup/export/import safety.
- [ ] Replace development key fallback with production key-management path.
- [ ] Add storage tests for image metadata and transcription persistence.
- [ ] Add corruption handling for missing image files and malformed transcription payloads.

---

## Phase 7: Import/export And Release Readiness

- [ ] Export page images.
- [ ] Export PDF pages and notebooks.
- [ ] Import PDF/Markdown notes.
- [ ] Replace placeholder app assets.
- [ ] Configure MSIX packaging and signing.
- [ ] Add release workflow, artifacts, changelog, and version tags.
- [ ] Add diagnostics and local crash/error logs.

---

## Immediate Execution Order

1. [x] Fix CI restore failure.
2. [x] Fix compile/build errors.
3. [x] Flatten repository layout to root project.
4. [x] Confirm local Windows launch.
5. [x] Replace drawing-first shell with import-first page image shell.
6. [x] Add imported page, notebook, and page image models.
7. [x] Import image into app-managed local storage.
8. [x] Save imported page metadata and local image path in SQLCipher.
9. [x] Reopen and load latest imported page.
10. [x] Add page library sidebar.
11. [x] Add image preprocessing pipeline boundary.
12. [x] Add transcription provider boundary.
13. [x] Add transcription UI and correction panel.
14. [x] Persist raw/corrected transcription.
15. [x] Add basic sidebar search.
16. [x] Add Markdown/export handoff.
17. [ ] Add full notebook grouping and selection.
18. [ ] Add full-text indexed search.
19. [ ] Add MSIX packaging.

## Working Rule

Every major feature should support the physical-notebook capture workflow. Do not let the drawing canvas drive architecture unless the task is explicitly about future annotations.
