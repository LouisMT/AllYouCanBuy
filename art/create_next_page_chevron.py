import math
from pathlib import Path

import bpy
from mathutils import Vector
from mathutils.geometry import tessellate_polygon


OUTPUT_DIR = Path(__file__).resolve().parent
BLEND_PATH = OUTPUT_DIR / "NextPageCircledChevron.blend"
CAMERA_INCLINE_DEGREES = 25


def create_extruded_polygon(name, points, thickness):
    points = list(reversed(points))
    count = len(points)
    half = thickness / 2
    vertices = [(point.x, point.y, -half) for point in points]
    vertices += [(point.x, point.y, half) for point in points]

    faces = []
    for triangle in tessellate_polygon([points]):
        indices = list(triangle)
        faces.append(tuple(reversed(indices)))
        faces.append(tuple(index + count for index in indices))
    for index in range(count):
        next_index = (index + 1) % count
        faces.append((index, next_index, next_index + count, index + count))

    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj


def make_material(name, base_color, metallic=0.0, roughness=0.35, emission=None, emission_strength=0.0):
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*base_color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness

    emission_input = shader.inputs.get("Emission Color") or shader.inputs.get("Emission")
    if emission_input is not None and emission is not None:
        emission_input.default_value = (*emission, 1.0)
    strength_input = shader.inputs.get("Emission Strength")
    if strength_input is not None:
        strength_input.default_value = emission_strength
    return material


def add_beveled_cube(name, location, scale, material, bevel_width):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    bevel = obj.modifiers.new("Rounded Corners", "BEVEL")
    bevel.width = bevel_width
    bevel.segments = 4
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    return obj


def point_camera(camera, target):
    camera.rotation_euler = (Vector(target) - camera.location).to_track_quat("-Z", "Y").to_euler()


bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)

glow = make_material(
    "Blueprint Symbol Glow",
    base_color=(0.63, 0.86, 1.0),
    metallic=0.05,
    roughness=0.22,
    emission=(0.30, 0.68, 1.0),
    emission_strength=1.7,
)
tile_material = make_material(
    "Preview Tile",
    base_color=(0.055, 0.075, 0.10),
    metallic=0.0,
    roughness=0.52,
)

bpy.ops.mesh.primitive_cylinder_add(vertices=96, radius=1.25, depth=0.30)
icon = bpy.context.object
icon.name = "NextPageCircledChevron"
icon.data.name = "NextPageCircledChevron"
icon.data.materials.append(glow)

cutter = create_extruded_polygon(
    "Chevron Cutout",
    [
        Vector((-0.43, 0.57)),
        Vector((-0.17, 0.83)),
        Vector((0.69, 0.0)),
        Vector((-0.17, -0.83)),
        Vector((-0.43, -0.57)),
        Vector((0.16, 0.0)),
    ],
    thickness=0.80,
)
boolean = icon.modifiers.new("Inset Chevron", "BOOLEAN")
boolean.operation = "DIFFERENCE"
boolean.solver = "EXACT"
boolean.object = cutter
bpy.context.view_layer.objects.active = icon
bpy.ops.object.modifier_apply(modifier=boolean.name)
bpy.data.objects.remove(cutter, do_unlink=True)

bevel = icon.modifiers.new("Soft PlateUp Edges", "BEVEL")
bevel.width = 0.055
bevel.segments = 3
bevel.limit_method = "ANGLE"
bpy.context.view_layer.objects.active = icon
bpy.ops.object.modifier_apply(modifier=bevel.name)

icon.rotation_euler.x = math.radians(90 + CAMERA_INCLINE_DEGREES)
bpy.context.view_layer.objects.active = icon
bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)

icon["design"] = "Right-facing circled chevron for the Next Page PlateUp tile"
icon["intended_material"] = "Inherited from the reroll dice"
icon["orientation"] = f"Inclined {CAMERA_INCLINE_DEGREES} degrees toward the PlateUp camera"

tile = add_beveled_cube(
    "Preview Tile Base",
    location=(0.0, 0.0, -0.34),
    scale=(1.72, 1.72, 0.18),
    material=tile_material,
    bevel_width=0.14,
)

bpy.ops.object.light_add(type="AREA", location=(2.8, -2.4, 4.8))
key = bpy.context.object
key.name = "Key Light"
key.data.energy = 850
key.data.shape = "DISK"
key.data.size = 3.0

bpy.ops.object.light_add(type="AREA", location=(-3.0, 1.5, 2.8))
fill = bpy.context.object
fill.name = "Fill Light"
fill.data.energy = 500
fill.data.color = (0.32, 0.55, 1.0)
fill.data.size = 2.5

bpy.ops.object.camera_add(location=(3.7, -4.4, 3.8))
camera = bpy.context.object
camera.name = "Preview Camera"
camera.data.lens = 52
point_camera(camera, (0.0, 0.0, 0.0))
bpy.context.scene.camera = camera

world = bpy.context.scene.world
world.use_nodes = True
world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.012, 0.018, 0.03, 1.0)
world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.35

scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 700
scene.render.resolution_y = 700
scene.render.resolution_percentage = 100
scene.render.film_transparent = False

bpy.data.objects.remove(tile, do_unlink=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))

print(BLEND_PATH)
