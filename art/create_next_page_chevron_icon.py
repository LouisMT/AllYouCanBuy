import struct
from pathlib import Path

from PIL import Image, ImageDraw


OUTPUT_DIR = Path(__file__).resolve().parent
RESOURCE_PATH = OUTPUT_DIR.parent / "AllYouCanBuy" / "Assets" / "NextPageCircledChevronIcon.rgba"
SIZE = 128
SUPERSAMPLING = 4


def to_pixels(points):
    extent = 1.45
    scale = SIZE * SUPERSAMPLING / (extent * 2)
    centre = SIZE * SUPERSAMPLING / 2
    return [
        (centre + x * scale, centre - y * scale)
        for x, y in points
    ]


large_size = SIZE * SUPERSAMPLING
image = Image.new("RGBA", (large_size, large_size), (0, 0, 0, 0))
draw = ImageDraw.Draw(image)
colour = (247, 252, 255, 255)
radius = 1.25
draw.ellipse(to_pixels([(-radius, radius), (radius, -radius)]), fill=colour)
draw.polygon(
    to_pixels([
        (-0.43, 0.57),
        (-0.17, 0.83),
        (0.69, 0.0),
        (-0.17, -0.83),
        (-0.43, -0.57),
        (0.16, 0.0),
    ]),
    fill=(0, 0, 0, 0),
)

image = image.resize((SIZE, SIZE), Image.Resampling.LANCZOS)

# Unity raw textures are bottom-up; preserve the PNG's displayed orientation.
raw_image = image.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
with RESOURCE_PATH.open("wb") as output:
    output.write(b"AYCI")
    output.write(struct.pack("<II", SIZE, SIZE))
    output.write(raw_image.tobytes())

print(RESOURCE_PATH)
