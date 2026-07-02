# Scriptum Prioritized Development Todo

_Last updated: 2026-07-02_

Scriptum is now in the **buildable physical-notebook capture MVP** stage. CI restore and CI build are green, and the repository has been flattened so the WinUI project lives at the repository root.

The product direction has been corrected: Scriptum is not primarily for handwriting notes directly inside the app. The core workflow is to import or capture real physical notebook pages, preserve the original page image, transcribe handwriting into editable/searchable text, and make those notes useful for later development work.

See `docs/PRODUCT_DIRECTION.md` for the source-of-truth product direction.

## Priority key

- **P0**: Blocks the app from being usable or trusted.
- **P1**: Required for the physical-notebook capture MVP.
- **P2**: Important for daily development workflow usefulness.
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

### P0.3 Confirm local Windows launch — next

- [ ] Clone or pull `main` locally.
- [ ] Restore packages locally.
- [ ] Build from Visual Studio or Windows terminal.
- [ ] Launch the app.
- [ ] Confirm `MainWindow` opens without crashing.
- [ ] Confirm `MainView` displays the current development surface.

### P0.4 Clean project structure

- [x] Move project files from nested `Scriptum/` folder to repository root.
- [x] Update CI to build `Scriptum.csproj` from repository root.
- [ ] Confirm `Scriptum.sln` works locally.
- [ ] Confirm `.gitignore` covers build artifacts, packages, logs, user files, and secrets.
- [ ] Add local development setup instructions to the README.

---

## Phase 1: Physical notebook page import MVP

Goal: create the smallest useful Scriptum experience: open app, import/capture a real notebook page image, display it, save metadata securely, close, reopen, and reload it.

### P1.1 Replace drawing-first UI with import-first shell

- [ ] Add primary Import Page action.
- [ ] Add capture placeholder action for future camera/scanner flow.
- [ ] Display selected/imported page image in the main view.
- [ ] Move the drawing canvas behind a development/annotation-only path.
- [ ] Make the imported image the center of the UI.

### P1.2 Add imported page domain model

- [ ] Add `ImportedPage` model with ID, title, created/updated timestamps, source type, and notes.
- [ ] Add `PageImage` metadata with original local image path, file name, dimensions, content type, checksum, and thumbnail path.
- [ ] Add `Transcription` model with status, raw text, corrected text, provider metadata, and timestamps.
- [ ] Treat vector ink/strokes as optional annotation data, not primary notebook data.
- [ ] Add model versioning for future migrations.

### P1.3 Import image into app-managed local storage

- [ ] Support importing PNG/JPEG files from disk.
- [ ] Copy imported images into an app-managed local storage folder.
- [ ] Preserve the original imported image without destructive preprocessing.
- [ ] Generate a stable local file path for each imported page.
- [ ] Compute a checksum or content hash to detect duplicates or corruption.

### P1.4 Persist imported page metadata with SQLCipher

- [ ] Confirm encrypted database opens.
- [ ] Add `imported_pages` table.
- [ ] Add `page_images` table or image metadata fields.
- [ ] Save page metadata and local image path.
- [ ] Close and reopen the app.
- [ ] Load the latest imported page.
- [ ] Display the saved original image again.
- [ ] Add clear database/image-file failure messages.

### P1.5 Add image viewer basics

- [ ] Show the full imported page clearly.
- [ ] Add zoom in/out.
- [ ] Add pan.
- [ ] Add fit-to-window.
- [ ] Add actual-size view.
- [ ] Preserve readability for notebook photos and scans.

---

## Phase 2: OCR/transcription MVP

Goal: turn imported physical notebook pages into editable/searchable text while preserving the original image.

### P2.1 Add preprocessing pipeline

- [ ] Add image preprocessing service interface.
- [ ] Load original image from local path.
- [ ] Create a derived preprocessed image artifact without overwriting the original.
- [ ] Add crop/deskew/contrast cleanup hooks.
- [ ] Add debug output only for development mode.

### P2.2 Add transcription provider boundary

- [ ] Add `ITranscriptionProvider` interface.
- [ ] Add request/response models for page-image transcription.
- [ ] Add mock provider for local testing.
- [ ] Add Qwen-VL or other vision provider adapter later.
- [ ] Keep provider implementation swappable.

### P2.3 Add transcription UI

- [ ] Add Transcribe Page action.
- [ ] Show transcription status: not started, queued, processing, complete, failed.
- [ ] Show raw transcription output.
- [ ] Add editable corrected transcription field.
- [ ] Save corrected transcription.
- [ ] Preserve transcription across app restart.

### P2.4 Persist transcription records

- [ ] Add `transcriptions` table.
- [ ] Link transcription records to imported pages.
- [ ] Store raw text and corrected text separately.
- [ ] Store provider metadata and processing timestamps.
- [ ] Store failure messages safely without leaking secrets.

---

## Phase 3: Notebook organization and daily usability

- [ ] Add `Notebook` model for grouping imported physical pages.
- [ ] Add page list/sidebar.
- [ ] Add page title/rename behavior.
- [ ] Add delete behavior with confirmation.
- [ ] Add tags and project/repo association.
- [ ] Add latest page and recent pages navigation.
- [ ] Add keyboard shortcuts for import, save, search, and transcription.

---

## Phase 4: Search and development workflow usefulness

- [ ] Add full-text search over corrected transcriptions.
- [ ] Search pages by title, tags, project, and transcription text.
- [ ] Jump from search result to original page image and transcription.
- [ ] Add developer note templates for corrected transcription output.
- [ ] Add Markdown export.
- [ ] Add structured handoff exports for GitHub issues, PR checklists, docs, Codex, ChatGPT, and Qwen Coder.

---

## Phase 5: Annotation and canvas as secondary feature

The drawing canvas is not the MVP driver. It can become useful later as an annotation layer over imported notebook images.

- [ ] Add optional annotation overlay over imported images.
- [ ] Save annotations separately from the original image.
- [ ] Add eraser/selection tools for annotations.
- [ ] Add annotation visibility toggle.
- [ ] Keep original image immutable.

---

## Phase 6: Data durability and security

- [ ] Normalize persistence into notebooks, imported pages, page images, transcriptions, tags, and search index tables.
- [ ] Add schema versioning and migrations.
- [ ] Add backup/export/import safety.
- [ ] Replace development key fallback with production key-management path.
- [ ] Add storage tests for image metadata and transcription persistence.
- [ ] Add corruption handling for missing image files and malformed transcription payloads.

---

## Phase 7: Import/export and release readiness

- [ ] Import images from file picker.
- [ ] Add camera/scanner capture flow if feasible.
- [ ] Export original page image and corrected transcription.
- [ ] Export Markdown and PDF bundles.
- [ ] Replace placeholder app assets.
- [ ] Configure MSIX packaging and signing.
- [ ] Add release workflow, artifacts, changelog, and version tags.
- [ ] Add diagnostics and local crash/error logs.

---

## Immediate execution order

1. [x] Fix CI restore failure.
2. [x] Fix compile/build errors.
3. [x] Flatten repository layout to root project.
4. [ ] Confirm local Windows launch.
5. [ ] Replace drawing-first shell with import-first page image shell.
6. [ ] Add imported page and page image models.
7. [ ] Import image into app-managed local storage.
8. [ ] Save imported page metadata and local image path in SQLCipher.
9. [ ] Reopen and load latest imported page.
10. [ ] Add image viewer zoom/pan/fit.
11. [ ] Add preprocessing service boundary.
12. [ ] Add transcription provider boundary.
13. [ ] Add transcription UI and correction panel.
14. [ ] Persist raw/corrected transcription.
15. [ ] Add search over corrected transcriptions.
16. [ ] Add Markdown/export handoff.

## Working rule

Every major feature should support the physical-notebook capture workflow. Do not let the drawing canvas drive architecture unless the task is explicitly about future annotations.
