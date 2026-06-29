# Scriptum Prioritized Development Todo

_Last updated: 2026-06-29_

This document is the working playbook for turning Scriptum from an early WinUI/ink prototype into a dependable local-first Windows notebook app that can become part of a real development workflow.

## Current development posture

Scriptum is currently in the **foundation to first runnable MVP** stage.

The highest priority is not feature expansion yet. The highest priority is to make the app build, launch, save ink reliably, and prove the basic notebook loop before layering transcription and workflow features on top.

## Priority key

- **P0**: Blocks the app from being usable or trusted.
- **P1**: Required for a usable MVP.
- **P2**: Important for daily workflow usefulness.
- **P3**: Polish, scale, release, and advanced workflow expansion.

---

# Phase 0: Build stability and repo hygiene

Goal: make the project restore, build, and validate reliably in GitHub Actions and locally.

## P0.1 Fix CI restore failure

- [ ] Capture the exact restore error from GitHub Actions logs.
- [ ] Verify package compatibility for:
  - [ ] `Microsoft.WindowsAppSDK`
  - [ ] `Microsoft.Graphics.Win2D`
  - [ ] `Microsoft.Windows.SDK.BuildTools`
  - [ ] `Microsoft.Data.Sqlite.Core`
  - [ ] `SQLitePCLRaw.bundle_sqlcipher`
- [ ] Decide whether Win2D is needed immediately or can be temporarily removed until rendering/export work starts.
- [ ] Run restore against `Scriptum/Scriptum.csproj` instead of the solution until the project is stable.
- [ ] Keep CI logs readable enough to diagnose future errors.

**Done when:** GitHub Actions completes `dotnet restore` successfully.

## P0.2 Fix compile and XAML errors

- [ ] Run CI build after restore succeeds.
- [ ] Fix generated XAML/code-behind issues.
- [ ] Fix namespace/import issues.
- [ ] Fix package API mismatches.
- [ ] Fix nullable warnings that indicate real lifecycle bugs.

**Done when:** GitHub Actions completes `dotnet build` successfully.

## P0.3 Confirm local Windows launch

- [ ] Clone branch locally on Windows or WSL + Windows toolchain.
- [ ] Restore packages locally.
- [ ] Build from Visual Studio.
- [ ] Launch the app.
- [ ] Confirm `MainWindow` opens without crash.
- [ ] Confirm `MainView` hosts the notebook canvas.

**Done when:** the app opens on a Windows machine and shows the notebook canvas.

## P0.4 Clean project structure

- [ ] Confirm `Scriptum.sln` works once project build is stable.
- [ ] Remove obsolete references if the solution is not needed.
- [ ] Ensure `.gitignore` covers build artifacts, packages, user files, logs, and secrets.
- [ ] Confirm folder layout:
  - [ ] `Models`
  - [ ] `ViewModels`
  - [ ] `Views`
  - [ ] `Services`
  - [ ] `Data`
  - [ ] `Assets`
  - [ ] `docs`
- [ ] Add a short contributor/dev setup section to the README.

**Done when:** a fresh clone has obvious setup instructions and a stable structure.

---

# Phase 1: First working notebook loop

Goal: create the smallest useful Scriptum experience: open app, draw, save, reload.

## P1.1 Stabilize drawing canvas

- [ ] Verify pointer drawing works with mouse.
- [ ] Verify pointer drawing works with pen/stylus if available.
- [ ] Verify touch input behavior.
- [ ] Smooth stroke rendering.
- [ ] Prevent accidental multi-pointer corruption.
- [ ] Add bounds handling so strokes do not create broken geometry.
- [ ] Add cursor/pen mode affordance.

**Done when:** drawing feels predictable and does not lose strokes during normal use.

## P1.2 Stabilize vector ink model

- [ ] Confirm `InkPoint` contains enough data for later transcription/rendering.
- [ ] Confirm `InkStroke` stores:
  - [ ] color
  - [ ] thickness
  - [ ] timestamps
  - [ ] pressure
  - [ ] ordered points
- [ ] Add optional stroke metadata for future tools:
  - [ ] eraser support
  - [ ] selection support
  - [ ] page transform support
- [ ] Add model versioning for future migrations.

**Done when:** saved ink can evolve without breaking old notebook files.

## P1.3 Make SQLCipher page save/load reliable

- [ ] Confirm SQLCipher database opens successfully.
- [ ] Confirm `notebook_pages` table is created.
- [ ] Save page from UI.
- [ ] Close app.
- [ ] Reopen app.
- [ ] Load latest page.
- [ ] Confirm all strokes redraw correctly.
- [ ] Add clear error messages for database failures.
- [ ] Add defensive handling for corrupt payloads.

**Done when:** a user can draw, save, close, reopen, and reload the same page.

## P1.4 Add a basic page lifecycle

- [ ] Add “New Page” command.
- [ ] Add page title field.
- [ ] Add rename page behavior.
- [ ] Add delete page behavior with confirmation.
- [ ] Add updated timestamp display.
- [ ] Add dirty-state tracking for unsaved changes.
- [ ] Add save confirmation/status indicator.

**Done when:** the user can manage more than one page without losing work.

## P1.5 Add basic notebook model

- [ ] Add `Notebook` model.
- [ ] Add notebook ID/title/created/updated fields.
- [ ] Connect pages to notebooks.
- [ ] Add default notebook on first launch.
- [ ] Add current notebook state to the ViewModel.

**Done when:** pages belong to a real notebook instead of floating independently.

---

# Phase 2: MVP navigation and daily usability

Goal: make Scriptum usable for real note-taking sessions.

## P1.6 Add sidebar navigation

- [ ] Add left sidebar layout.
- [ ] Show current notebook.
- [ ] Show page list.
- [ ] Select page from list.
- [ ] Add new page button.
- [ ] Add empty-state UI.
- [ ] Add page modified indicator.

**Done when:** the user can move between pages without manually loading latest.

## P1.7 Add autosave

- [ ] Add debounce timer after stroke creation.
- [ ] Autosave after undo/clear.
- [ ] Avoid saving too frequently during active drawing.
- [ ] Show autosave status.
- [ ] Ensure app close triggers final save if needed.

**Done when:** normal use does not require constantly pressing Save.

## P1.8 Add undo/redo stack

- [ ] Replace simple undo with command history.
- [ ] Add redo.
- [ ] Track stroke add/remove/clear commands.
- [ ] Disable buttons when unavailable.
- [ ] Ensure undo/redo updates dirty state.

**Done when:** mistakes can be reversed and restored predictably.

## P1.9 Add eraser MVP

- [ ] Add eraser mode.
- [ ] Support whole-stroke erase first.
- [ ] Add clear visual state for selected tool.
- [ ] Save erased state.
- [ ] Add tests around erase persistence.

**Done when:** user can remove unwanted writing without clearing the whole page.

## P1.10 Add basic keyboard shortcuts

- [ ] `Ctrl+S` save.
- [ ] `Ctrl+Z` undo.
- [ ] `Ctrl+Y` redo.
- [ ] `Ctrl+N` new page.
- [ ] `Delete` delete selected stroke/page where appropriate.

**Done when:** common notebook actions feel fast enough for daily workflow.

---

# Phase 3: Data architecture and durability

Goal: make Scriptum trustworthy for real work notes.

## P1.11 Normalize persistence model

Current storage saves a full page payload. That is acceptable for early MVP, but long-term storage should be more structured.

- [ ] Add database schema version table.
- [ ] Add notebooks table.
- [ ] Add pages table.
- [ ] Add strokes table or page snapshots table.
- [ ] Add transcription table.
- [ ] Add tags table.
- [ ] Add search index table.
- [ ] Add migrations.

**Done when:** database structure can support growth without rewriting the app.

## P1.12 Add backup and restore safety

- [ ] Add database backup command.
- [ ] Add export raw notebook archive.
- [ ] Add import archive flow.
- [ ] Add corruption detection.
- [ ] Add backup before destructive migrations.

**Done when:** user notes are not trapped or fragile.

## P1.13 Production key management

- [ ] Replace dev fallback key with explicit development-only behavior.
- [ ] Add secure key provider abstraction.
- [ ] Investigate Windows Credential Manager / DPAPI / TPM-backed key flow.
- [ ] Avoid logging secrets.
- [ ] Add docs for local dev key setup.

**Done when:** encrypted storage has a credible production path.

## P1.14 Add storage tests

- [ ] Add unit tests for serialization.
- [ ] Add integration tests for page save/load.
- [ ] Add migration tests.
- [ ] Add corrupted payload handling tests.
- [ ] Add CI test job once build is green.

**Done when:** storage changes can be made without fear of breaking saved notes.

---

# Phase 4: Transcription MVP

Goal: turn handwritten notes into searchable, editable text.

## P1.15 Render page or selection to image

- [ ] Add rendering service boundary.
- [ ] Convert vector ink to bitmap/image buffer.
- [ ] Support full-page render.
- [ ] Support selected-region render.
- [ ] Preserve high enough resolution for OCR/vision.

**Done when:** handwriting can be converted into an image suitable for AI transcription.

## P1.16 Add preprocessing pipeline

- [ ] Add preprocessing service interface.
- [ ] Add OpenCVSharp integration or defer behind adapter.
- [ ] Implement contrast cleanup.
- [ ] Implement crop/bounds detection.
- [ ] Implement optional deskew if needed.
- [ ] Save debug images during development only.

**Done when:** handwriting images are clean enough for reliable transcription attempts.

## P1.17 Add AI transcription provider abstraction

- [ ] Add `ITranscriptionProvider` interface.
- [ ] Add request/response models.
- [ ] Add provider settings model.
- [ ] Add local/mock provider for testing.
- [ ] Add Qwen-VL provider boundary.
- [ ] Keep provider implementation swappable.

**Done when:** Scriptum can call a transcription provider without hardcoding one vendor into the UI.

## P1.18 Add transcription UI

- [ ] Add “Transcribe Page” button.
- [ ] Add “Transcribe Selection” button.
- [ ] Add progress state.
- [ ] Add transcription result panel.
- [ ] Add editable correction field.
- [ ] Save corrected transcription.

**Done when:** user can turn handwritten notes into editable text and fix mistakes.

## P1.19 Add transcription persistence

- [ ] Store raw transcription output.
- [ ] Store corrected transcription.
- [ ] Store provider metadata.
- [ ] Store confidence/quality metadata if available.
- [ ] Link transcription to page and region.

**Done when:** text survives app restart and can be searched later.

---

# Phase 5: Search, tags, and development workflow usefulness

Goal: make Scriptum genuinely useful inside a development workflow.

## P2.1 Add full-text search

- [ ] Index corrected transcriptions.
- [ ] Search pages by text.
- [ ] Highlight matching results.
- [ ] Jump from result to page.
- [ ] Add basic ranking by recency/title/content.

**Done when:** handwritten notes become searchable development memory.

## P2.2 Add tags and project context

- [ ] Add tags to pages.
- [ ] Add project/repo association field.
- [ ] Add quick tag UI.
- [ ] Add filter by tag.
- [ ] Add filter by project.

**Done when:** notes can be organized by project, repo, or task type.

## P2.3 Add developer note templates

- [ ] Daily dev log template.
- [ ] Bug investigation template.
- [ ] Feature planning template.
- [ ] Architecture sketch template.
- [ ] Meeting notes template.
- [ ] Research notes template.

**Done when:** Scriptum can support repeatable dev workflows, not just blank pages.

## P2.4 Add Markdown export

- [ ] Export corrected transcription as Markdown.
- [ ] Include page title/date/tags.
- [ ] Include embedded image snapshot optionally.
- [ ] Add copy-to-clipboard Markdown command.

**Done when:** notes can flow into README files, GitHub issues, PRs, docs, or ChatGPT prompts.

## P2.5 Add GitHub/workflow handoff ideas

- [ ] Export note as GitHub issue draft.
- [ ] Export note as PR checklist draft.
- [ ] Export architecture notes as Markdown doc.
- [ ] Add structured prompt export for Codex/ChatGPT/Qwen Coder.

**Done when:** Scriptum becomes a bridge between handwritten thinking and implementation work.

---

# Phase 6: UX quality and notebook feel

Goal: make Scriptum feel like a premium Windows notebook app.

## P2.6 Improve canvas feel

- [ ] Add zoom.
- [ ] Add pan.
- [ ] Add page background options.
- [ ] Add lined/grid/dot paper.
- [ ] Add dark mode canvas theme.
- [ ] Add pen thickness selector.
- [ ] Add color selector.

**Done when:** the canvas feels comfortable for extended writing sessions.

## P2.7 Add selection and transform tools

- [ ] Lasso select strokes.
- [ ] Move selected strokes.
- [ ] Delete selected strokes.
- [ ] Copy/paste selected strokes.
- [ ] Resize selected strokes if practical.

**Done when:** handwritten notes are editable, not just drawable.

## P2.8 Add page thumbnails

- [ ] Render page thumbnail.
- [ ] Cache thumbnail.
- [ ] Show thumbnail in sidebar.
- [ ] Refresh thumbnail after save/autosave.

**Done when:** users can visually navigate notebooks quickly.

## P2.9 Add command palette

- [ ] Open command palette shortcut.
- [ ] Search commands.
- [ ] Run commands from keyboard.
- [ ] Include page and export actions.

**Done when:** advanced users can operate quickly without hunting through UI.

---

# Phase 7: Import/export and interoperability

Goal: make notes portable and useful outside Scriptum.

## P2.10 Export page image

- [ ] Export PNG.
- [ ] Export JPEG.
- [ ] Choose background style.
- [ ] Choose scale/resolution.

**Done when:** handwritten pages can be shared visually.

## P2.11 Export PDF

- [ ] Export single page PDF.
- [ ] Export notebook PDF.
- [ ] Include transcription optionally.
- [ ] Preserve page order.

**Done when:** Scriptum notebooks can be shared or archived professionally.

## P2.12 Import existing notes

- [ ] Import image as page background.
- [ ] Import PDF page as background.
- [ ] Import Markdown as text/transcription note.

**Done when:** Scriptum can absorb existing workflow material.

---

# Phase 8: Packaging and release readiness

Goal: turn the app into something installable and maintainable.

## P3.1 Fix app assets

- [ ] Replace placeholder/empty PNG assets.
- [ ] Add real app icon.
- [ ] Add splash screen asset.
- [ ] Validate asset sizes required by MSIX.

**Done when:** packaging does not fail due to missing or invalid assets.

## P3.2 MSIX packaging

- [ ] Configure package identity.
- [ ] Configure publisher information.
- [ ] Configure signing cert for dev builds.
- [ ] Generate local install package.
- [ ] Document sideload install steps.

**Done when:** Scriptum can be installed locally as a Windows app.

## P3.3 Release pipeline

- [ ] Add GitHub Actions release workflow.
- [ ] Upload build artifacts.
- [ ] Generate release notes.
- [ ] Tag versions.
- [ ] Add changelog.

**Done when:** releases can be created consistently.

## P3.4 Diagnostics and crash reporting

- [ ] Add structured logging.
- [ ] Add local diagnostic logs.
- [ ] Add safe error display.
- [ ] Add export diagnostics command.

**Done when:** app failures can be debugged without guessing.

---

# Phase 9: Advanced workflow vision

Goal: make Scriptum a serious thinking layer for software development.

## P3.5 AI-assisted note cleanup

- [ ] Summarize handwritten page.
- [ ] Extract tasks from notes.
- [ ] Extract decisions from notes.
- [ ] Convert rough notes into implementation checklist.
- [ ] Convert architecture sketch notes into structured Markdown.

**Done when:** Scriptum helps transform raw thinking into usable development output.

## P3.6 Project memory mode

- [ ] Group notes by project.
- [ ] Link notes to GitHub repositories.
- [ ] Link notes to issues/PRs.
- [ ] Generate project timeline from notes.
- [ ] Generate weekly project summary.

**Done when:** Scriptum becomes a project memory system, not just a notebook.

## P3.7 Local-first sync strategy

- [ ] Decide whether sync is in scope.
- [ ] Support manual backup folder.
- [ ] Support cloud-drive-friendly archive format.
- [ ] Avoid corrupting encrypted databases during sync.
- [ ] Document recommended backup workflow.

**Done when:** notes can safely move across machines without compromising local-first principles.

---

# Immediate execution order

Start here. Do not skip ahead until the current item is done.

1. [ ] Fix CI restore failure.
2. [ ] Fix compile/build errors.
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
