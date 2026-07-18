import struct
import sys
from pathlib import Path

import bpy


OUTPUT_PATH = Path(sys.argv[-1]).resolve()
OBJECT_NAME = "CycleBlueprintArrows"

obj = bpy.data.objects[OBJECT_NAME]
mesh = obj.data
mesh.calc_loop_triangles()

vertices = []
normals = []
for triangle in mesh.loop_triangles:
    for vertex_index in triangle.vertices:
        position = obj.matrix_world @ mesh.vertices[vertex_index].co
        normal = (obj.matrix_world.to_3x3() @ triangle.normal).normalized()

        # Blender is Z-up; PlateUp/Unity is Y-up.
        vertices.append((position.x, position.z, -position.y))
        normals.append((normal.x, normal.z, -normal.y))

OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
with OUTPUT_PATH.open("wb") as output:
    output.write(b"AYCB")
    output.write(struct.pack("<I", len(vertices)))
    for position, normal in zip(vertices, normals):
        output.write(struct.pack("<6f", *position, *normal))

print(f"Exported {len(vertices)} vertices to {OUTPUT_PATH}")
