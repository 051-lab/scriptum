# Scriptum Prioritized Development Todo

_Last updated: 2026-06-29_

Scriptum is now in the **buildable first runnable MVP** stage. CI restore and CI build are green. The next priority is proving the app launches locally on Windows and that the notebook canvas can draw, save, close, reopen, and reload ink.

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

### P0.3 Confirm local Windows launch — in progress

- [x] Clone or pull `main` locally.
- [x] Restore packages locally.
- [x] Build from Visual Studio or Windows terminal.
- [x] Launch the app.
- [x] Confirm `MainWindow` opens without crashing.
- [x] Confirm `MainView` displays the notebook canvas.
- [ ] Manually verify drawing input from the Windows desktop session.

### P0.4 Clean project structure

- [ ] Confirm `Scriptum.sln` works locally.
- [ ] Confirm `.gitignore` covers build artifacts, packages, logs, user files, and secrets.
- [ ] Confirm folders are organized around `Models`, `ViewModels`, `Views`, `Services`, `Data`, `Assets`, and `docs`.
- [ ] Add local development setup instructions to the README.
- [ ] Reduce duplicate GitHub Actions notifications by running CI on PRs and `main` only.

---

## Phase 1: First working notebook loop

Goal: create the smallest useful Scriptum experience: open app, draw, save, close, reopen, reload.

### P1.1 Stabilize drawing canvas

- [ ] Verify manual mouse drawing in a Windows desktop session; WSL-driven synthetic mouse input did not create strokes during local setup validation.
- [ ] Verify mouse drawing.
- [ ] Verify pen/stylus drawing.
- [ ] Verify touch behavior.
- [ ] Smooth stroke rendering.
- [ ] Prevent multi-pointer corruption.
- [ ] Add bounds handling.
- [ ] Add clear pen-mode visual state.

### P1.2 Stabilize vector ink model

- [ ] Confirm `InkPoint` stores enough data for rendering/transcription.
- [ ] Confirm `InkStroke` stores color, thickness, pressure, timestamps, and ordered points.
- [ ] Add model versioning.
- [ ] Prepare metadata for future eraser, selection, and page transforms.

### P1.3 Prove SQLCipher save/load

- [ ] Confirm encrypted database opens.
- [ ] Confirm `notebook_pages` table is created.
- [ ] Save a page from the UI.
- [ ] Close and reopen the app.
- [ ] Load the latest page.
- [ ] Confirm strokes redraw correctly.
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
- [ ] Add autosave with debounce.
- [ ] Add undo/redo command stack.
- [ ] Add eraser MVP.
- [ ] Add keyboard shortcuts: Save, Undo, Redo, New Page, Delete.

---

## Phase 3: Data durability

- [ ] Normalize persistence into notebooks, pages, strokes/snapshots, transcriptions, tags, and search index tables.
- [ ] Add schema versioning and migrations.
- [ ] Add backup/export/import safety.
- [ ] Replace development key fallback with production key-management path.
- [ ] Add storage tests.

---

## Phase 4: Transcription MVP

- [ ] Render page or selection to image.
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
- [ ] Add lined/grid/dot paper backgrounds.
- [ ] Add dark mode canvas option.
- [ ] Add pen thickness and color selectors.
- [ ] Add selection/lasso tools.
- [ ] Add page thumbnails.
- [ ] Add command palette.

---

## Phase 7: Import/export and release readiness

- [ ] Export page images.
- [ ] Export PDF pages and notebooks.
- [ ] Import image/PDF/Markdown notes.
- [ ] Replace placeholder app assets.
- [ ] Configure MSIX packaging and signing.
- [ ] Add release workflow, artifacts, changelog, and version tags.
- [ ] Add diagnostics and local crash/error logs.

---

## Immediate execution order

1. [x] Fix CI restore failure.
2. [x] Fix compile/build errors.
3. [ ] Confirm local Windows launch.
4. [ ] Verify canvas drawing with mouse/pen/touch.
5. [ ] Verify SQLCipher save/load from the UI.
6. [ ] Add multiple pages.
7. [ ] Add sidebar page navigation.
8. [ ] Add autosave.
9. [ ] Add notebook model.
10. [ ] Add transcription rendering pipeline.
11. [ ] Add transcription provider boundary.
12. [ ] Add transcription UI.
13. [ ] Add search.
14. [ ] Add Markdown export.
15. [ ] Add MSIX packaging.

## Working rule

Every major feature should have a clear done condition before moving to the next phase. Scriptum should grow in small, testable increments so it becomes dependable enough to support real development work.
