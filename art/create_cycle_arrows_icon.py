import math
import struct
from pathlib import Path

from PIL import Image, ImageDraw


OUTPUT_DIR = Path(__file__).resolve().parent
RESOURCE_PATH = OUTPUT_DIR.parent / "AllYouCanBuy" / "Assets" / "CycleBlueprintArrowsIcon.rgba"
SIZE = 128
SUPERSAMPLING = 4


def arrow_polygon(start_degrees, end_degrees, radius=1.0, width=0.22, head_width=0.42, head_length=0.52):
    start = math.radians(start_degrees)
    end = math.radians(end_degrees)
    steps = 32

    outer = []
    inner = []
    for index in range(steps + 1):
        angle = start + (end - start) * index / steps
        outer.append(((radius + width) * math.cos(angle), (radius + width) * math.sin(angle)))
        inner.append(((radius - width) * math.cos(angle), (radius - width) * math.sin(angle)))

    radial = (math.cos(end), math.sin(end))
    tangent = (-math.sin(end), math.cos(end))
    centre = (radial[0] * radius, radial[1] * radius)
    head_outer = (centre[0] + radial[0] * head_width, centre[1] + radial[1] * head_width)
    tip = (centre[0] + tangent[0] * head_length, centre[1] + tangent[1] * head_length)
    head_inner = (centre[0] - radial[0] * head_width, centre[1] - radial[1] * head_width)

    return outer + [head_outer, tip, head_inner] + list(reversed(inner))


def to_pixels(points):
    extent = 1.52
    scale = SIZE * SUPERSAMPLING / (extent * 2)
    centre = SIZE * SUPERSAMPLING / 2
    return [
        (centre + (-x) * scale, centre - y * scale)
        for x, y in points
    ]


large_size = SIZE * SUPERSAMPLING
image = Image.new("RGBA", (large_size, large_size), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)
colour = (247, 252, 255, 255)
draw.polygon(to_pixels(arrow_polygon(-20, 100)), fill=colour)
draw.polygon(to_pixels(arrow_polygon(160, 280)), fill=colour)

image = image.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

# Unity raw textures are bottom-up; preserve the PNG's displayed orientation.
raw_image = image.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
with RESOURCE_PATH.open("wb") as output:
    output.write(b"AYCI")
    output.write(struct.pack("<II", SIZE, SIZE))
    output.write(raw_image.tobytes())

print(RESOURCE_PATH)
