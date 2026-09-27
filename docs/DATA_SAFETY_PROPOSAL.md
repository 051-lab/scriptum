# Scriptum data-safety proposal

**Status:** Proposal only. Prepared from a read-only source review on 2026-09-25. This document authorizes no implementation, test/build command, app launch, install, network/provider call, or access to existing user data.

## Purpose

Make the notebook archive safe to trust with irreplaceable pages before using real personal material. Preserve the original image and notebook record together, protect confidential content, and ensure deletion is limited to files Scriptum owns.

## Source-backed baseline

- `Data/DatabaseContext.cs` applies a SQLCipher key, but falls back to the literal `scriptum-dev-key` when neither an explicit key nor `SCRIPTUM_DATABASE_KEY` is supplied. The default SQLite page and notebook storage services construct this default context for `%LOCALAPPDATA%\Scriptum\scriptum.db`.
- Imported originals and prepared images are ordinary files in `%LOCALAPPDATA%\Scriptum\ImportedPages` and `%LOCALAPPDATA%\Scriptum\PreparedPages`; they are outside the encrypted database. The README documents an optional configured vision provider that sends the prepared page image to its endpoint. No provider was contacted.
- Import copies the image before saving its page record. The UI then attempts the save; a database-save failure can leave the copied image without a corresponding page record.
- Page deletion removes the database row before image cleanup. The cleanup check compares full-path strings with `StartsWith(root)`, which does not enforce a directory boundary; cleanup exceptions are ignored. A similarly prefixed sibling path can pass that check if such a path is present in stored page metadata.
- The repository has one application `.csproj`. Its CI workflow restores and builds; no test project or `tests` directory was found in the inspected checkout.

These are static source findings, not reproduced failures. No local application data was inspected, and no files were deleted.

## Proposed work, in order

### 1. Implement the selected protection baseline and settle key lifecycle

**User-selected baseline:** protect both the database and original/prepared page images with an OS-protected key, and provide an explicit secure recovery/export path for use after Windows reinstallation or on another PC. The exact key-protection mechanism and recovery format still need a design decision before implementation. The recovery secret must not be stored in this repository, exposed in chat, or silently included in ordinary exports. The app must not silently create or open a database using the public development key when the required protected key is unavailable. Existing development-key databases must remain recoverable through an explicit, verified migration path; changing the key must not strand existing pages. At-rest protection does not protect an image after decryption for display or an explicitly configured provider request; any provider transmission remains a separate, visible user decision. Until the protection and migration path are implemented and validated, do not use real sensitive notebook pages.

### 2. Make import/save failure recoverable

Give image import a defined commit lifecycle so a copied image and its database record cannot silently diverge. On a failed save, either safely remove only the newly staged app-owned copy or retain an explicit recoverable pending state. Account for interruption between copy and database commit; do not broadly sweep or delete unknown files. The user-facing status must distinguish a successful import-and-save from a failed or pending save.

### 3. Enforce deletion ownership and consistency

Centralize validation for imported/prepared paths. Require a true path boundary beneath the expected app-owned root, reject paths that traverse reparse points/junctions or otherwise resolve outside that root, and never delete a path based only on a string prefix. Define a recoverable delete sequence across the database, search index, and image files: if one part fails, retain enough state to report and retry safely rather than silently losing the record or leaving an unexplained orphan. Keep the existing confirmation prompt.

### 4. Establish provider-free regression checks before implementation

There is no current test project in the inspected checkout, so first propose a small isolated test harness and how CI should run it. Use synthetic files and temporary directories only; do not access `%LOCALAPPDATA%`, user notebooks, credentials, devices, or external providers. Follow test-first development: each regression should be observed failing before its fix is implemented.

Minimum behavioral cases:

- Missing, wrong, and valid keys; opening a synthetic legacy-key database and migrating it without losing records; recovering a synthetic encrypted archive on a separate test profile without exposing the recovery secret.
- Import succeeds; database save fails after image staging; interruption leaves a discoverable/recoverable state; original source image remains unchanged.
- Valid owned image path; similarly prefixed sibling path; path outside the root; missing file; and reparse-point/junction escape. No outside file may be deleted.
- Database, index, or file deletion failure leaves a visible, retryable state and does not falsely report complete deletion.
- No test performs provider/network calls or reads real user data.

## Acceptance gate

Do not call Scriptum safe for irreplaceable personal notebooks until database and image protection use the selected OS-protected-key baseline; the explicit cross-PC/reinstall recovery path and existing-database compatibility are demonstrated on synthetic data; import and delete failure cases pass isolated regressions; and user-facing behavior makes failures recoverable. Build/test permissions and implementation authority must be granted separately before executing those steps.

## Explicit non-goals

No broad refactor, schema redesign, feature expansion, live OCR/vision validation, external backup, AppData inspection, user-data migration, dependency installation, application launch, build, test, commit, push, or release is included in this proposal.
