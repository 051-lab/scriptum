# Scriptum Product Direction

_Last updated: 2026-07-02_

## Correct product thesis

Scriptum is a local-first Windows desktop app for turning **real physical handwritten notebook pages** into preserved, searchable, useful digital development memory.

The primary user does not start by handwriting directly inside Scriptum. The primary user writes in real notebooks first, then uses Scriptum to import or capture those pages, preserve the original image, transcribe the handwriting, and organize the resulting text for later software development work.

## Primary MVP workflow

1. Open the Windows desktop app.
2. Import an image file or capture a physical notebook page.
3. Display the original page image clearly in the app.
4. Save page metadata and the local image path in encrypted local storage.
5. Reopen the app and load the latest imported page.
6. Prepare the image for OCR or vision transcription.
7. Transcribe the handwritten notes into editable/searchable text.
8. Preserve both the original image and the transcription.

## What Scriptum is not, at least for the MVP

Scriptum is not primarily a blank digital handwriting canvas. A drawing surface can remain useful as a temporary development tool or later annotation layer, but it should not drive the MVP architecture, data model, or UI priorities.

The earlier pointer-driven drawing canvas should be treated as:

- a development surface for testing UI/persistence plumbing,
- a possible future annotation feature over imported images,
- not the main notebook creation workflow.

## Core domain model direction

The main persisted entities should represent imported physical notebook pages and transcription results.

Recommended early entities:

- `Notebook`: a collection of imported pages.
- `ImportedPage`: metadata for one physical notebook page.
- `PageImage`: original image path, capture/import source, dimensions, checksum, and optional thumbnail path.
- `Transcription`: raw OCR/vision output, corrected text, provider metadata, status, timestamps, and confidence/quality notes when available.
- `Tag` or project metadata: optional organization for development workflow search.

Vector ink/stroke entities should be considered optional annotation data, not the primary source of truth.

## UI direction

The first real UI should optimize for imported page review and transcription:

- import/capture button,
- image viewer with zoom and pan,
- latest/imported pages list,
- page metadata panel,
- transcription status and actions,
- editable transcription panel,
- search results over corrected transcription text.

A canvas can exist later as an overlay for markup, selection, cropping, or annotation, but the imported original image must remain preserved.

## Storage direction

Scriptum should store user data locally and securely:

- keep original imported page images in an app-managed local storage folder,
- store metadata and transcription records in encrypted SQLCipher-backed storage,
- store only local file paths or content hashes in the database unless there is a reason to store image blobs,
- preserve original images without destructive preprocessing,
- save preprocessed images separately as derived artifacts when needed for transcription.

## Transcription direction

The transcription pipeline should be provider-agnostic:

- image preprocessing service,
- OCR/vision provider interface,
- mock provider for testing,
- Qwen-VL or other vision model adapter later,
- editable correction UI,
- persisted raw and corrected transcription text.

## Immediate implementation priority

The next development work should not deepen the drawing canvas. It should pivot toward:

1. local Windows launch verification,
2. image import into app-managed storage,
3. imported image display,
4. encrypted metadata persistence,
5. latest imported page load,
6. preprocessing/transcription service boundaries,
7. editable transcription UI,
8. search over corrected transcriptions.
