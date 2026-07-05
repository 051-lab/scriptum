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

        pixel_values = processor(images=image, return_tensors="pt").pixel_values.to(device)
        with torch.no_grad():
            generated_ids = model.generate(pixel_values)

        text = processor.batch_decode(generated_ids, skip_special_tokens=True)[0]
        print(text.strip())
        return 0
    except Exception as exc:  # noqa: BLE001 - this is a process boundary.
        print(f"TrOCR transcription failed: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
