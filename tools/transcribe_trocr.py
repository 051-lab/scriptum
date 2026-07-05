#!/usr/bin/env python
"""Run a local TrOCR handwritten transcription pass for Scriptum.

This helper intentionally keeps model dependencies outside the WinUI app.
Install dependencies in a Python environment, then point Scriptum at this
script with SCRIPTUM_TRANSCRIPTION_PROVIDER=trocr.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import Iterable


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Transcribe an image with TrOCR.")
    parser.add_argument("image", help="Path to the notebook page image.")
    parser.add_argument(
        "--model",
        default="microsoft/trocr-base-handwritten",
        help="Hugging Face TrOCR model id or local model path.",
    )
    parser.add_argument(
        "--device",
        default="cpu",
        choices=["cpu", "cuda", "mps"],
        help="Torch device. CPU is the safest default on this machine.",
    )
    parser.add_argument(
        "--mode",
        default="lines",
        choices=["page", "lines"],
        help="Transcribe the whole page or split it into horizontal text lines first.",
    )
    parser.add_argument(
        "--line-threshold",
        type=int,
        default=230,
        help="Grayscale threshold used for detecting dark handwriting pixels.",
    )
    parser.add_argument(
        "--min-line-height",
        type=int,
        default=12,
        help="Minimum detected line height in pixels.",
    )
    parser.add_argument(
        "--line-padding",
        type=int,
        default=10,
        help="Vertical padding around detected line crops in pixels.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    image_path = Path(args.image)
    if not image_path.exists():
        print(f"Image not found: {image_path}", file=sys.stderr)
        return 2

    try:
        import torch
        from PIL import Image
        from transformers import TrOCRProcessor, VisionEncoderDecoderModel
    except ImportError as exc:
        print(
            "Missing Python dependencies. Install them with: "
            "python -m pip install torch pillow transformers sentencepiece",
            file=sys.stderr,
        )
        print(str(exc), file=sys.stderr)
        return 3

    try:
        image = Image.open(image_path).convert("RGB")
        processor = TrOCRProcessor.from_pretrained(args.model)
        model = VisionEncoderDecoderModel.from_pretrained(args.model)
        device = torch.device(args.device)
        model.to(device)
        model.eval()

        if args.mode == "page":
            text = transcribe_image(torch, processor, model, device, image)
            print(text.strip())
            return 0

        line_images = list(
            detect_line_images(
                image,
                threshold=args.line_threshold,
                min_line_height=args.min_line_height,
                padding=args.line_padding,
            )
        )
        if not line_images:
            text = transcribe_image(torch, processor, model, device, image)
            print(text.strip())
            return 0

        lines: list[str] = []
        for line_image in line_images:
            text = transcribe_image(torch, processor, model, device, line_image)
            if text.strip():
                lines.append(text.strip())

        print("\n".join(lines).strip())
        return 0
    except Exception as exc:  # noqa: BLE001 - this is a process boundary.
        print(f"TrOCR transcription failed: {exc}", file=sys.stderr)
        return 1


def transcribe_image(torch, processor, model, device, image) -> str:
    pixel_values = processor(images=image, return_tensors="pt").pixel_values.to(device)
    with torch.no_grad():
        generated_ids = model.generate(pixel_values)

    return processor.batch_decode(generated_ids, skip_special_tokens=True)[0]


def detect_line_images(
    image,
    threshold: int,
    min_line_height: int,
    padding: int,
) -> Iterable:
    grayscale = image.convert("L")
    width, height = grayscale.size
    pixels = grayscale.load()

    row_has_ink: list[bool] = []
    min_ink_pixels = max(3, width // 150)
    for y in range(height):
        ink_pixels = 0
        for x in range(width):
            if pixels[x, y] < threshold:
                ink_pixels += 1
        row_has_ink.append(ink_pixels >= min_ink_pixels)

    bands: list[tuple[int, int]] = []
    start: int | None = None
    gap = 0
    max_gap = 4
    for y, has_ink in enumerate(row_has_ink):
        if has_ink:
            if start is None:
                start = y
            gap = 0
            continue

        if start is not None:
            gap += 1
            if gap > max_gap:
                end = y - gap
                if end - start + 1 >= min_line_height:
                    bands.append((start, end))
                start = None
                gap = 0

    if start is not None:
        end = height - 1
        if end - start + 1 >= min_line_height:
            bands.append((start, end))

    for start_y, end_y in merge_close_bands(bands):
        top = max(0, start_y - padding)
        bottom = min(height, end_y + padding + 1)
        crop = image.crop((0, top, width, bottom))
        yield trim_horizontal_whitespace(crop, threshold=threshold, padding=padding)


def merge_close_bands(bands: list[tuple[int, int]]) -> list[tuple[int, int]]:
    if not bands:
        return []

    merged = [bands[0]]
    max_gap = 8
    for start, end in bands[1:]:
        previous_start, previous_end = merged[-1]
        if start - previous_end <= max_gap:
            merged[-1] = (previous_start, end)
        else:
            merged.append((start, end))

    return merged


def trim_horizontal_whitespace(image, threshold: int, padding: int):
    grayscale = image.convert("L")
    width, height = grayscale.size
    pixels = grayscale.load()
    columns: list[int] = []

    for x in range(width):
        for y in range(height):
            if pixels[x, y] < threshold:
                columns.append(x)
                break

    if not columns:
        return image

    left = max(0, min(columns) - padding)
    right = min(width, max(columns) + padding + 1)
    return image.crop((left, 0, right, height))


if __name__ == "__main__":
    raise SystemExit(main())
