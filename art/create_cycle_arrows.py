import math
from pathlib import Path

import bpy
from mathutils import Vector


OUTPUT_DIR = Path(__file__).resolve().parent
BLEND_PATH = OUTPUT_DIR / "CycleBlueprintArrows.blend"


def arrow_polygon(start_degrees, end_degrees, radius=1.0, width=0.22, head_width=0.42, head_length=0.52):
    start = math.radians(start_degrees)
    end = math.radians(end_degrees)
    steps = 20

    outer = []
    inner = []
    for index in range(steps + 1):
        angle = start + (end - start) * index / steps
        outer.append(Vector(((radius + width) * math.cos(angle), (radius + width) * math.sin(angle))))
        inner.append(Vector(((radius - width) * math.cos(angle), (radius - width) * math.sin(angle))))

    radial = Vector((math.cos(end), math.sin(end)))
    tangent = Vector((-math.sin(end), math.cos(end)))
    centre = radial * radius
    head_outer = centre + radial * head_width
    tip = centre + tangent * head_length
    head_inner = centre - radial * head_width

    return outer + [head_outer, tip, head_inner] + list(reversed(inner))


def create_extruded_polygon(name, points, thickness=0.30):
    count = len(points)
    half = thickness / 2
    vertices = [(point.x, point.y, -half) for point in points]
    vertices += [(point.x, point.y, half) for point in points]

    faces = [tuple(reversed(range(count))), tuple(range(count, count * 2))]
    for index in range(count):
        next_index = (index + 1) % count
        faces.append((index, next_index, next_index + count, index + count))

    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()

    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)

    bevel = obj.modifiers.new("Soft PlateUp Edges", "BEVEL")
    bevel.width = 0.075
    bevel.segments = 3
    bevel.limit_method = "ANGLE"

    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    obj.select_set(False)
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

arrow_a = create_extruded_polygon("Cycle Arrow A", arrow_polygon(-20, 100))
arrow_b = create_extruded_polygon("Cycle Arrow B", arrow_polygon(160, 280))
arrow_a.data.materials.append(glow)
arrow_b.data.materials.append(glow)

bpy.ops.object.select_all(action="DESELECT")
arrow_a.select_set(True)
arrow_b.select_set(True)
bpy.context.view_layer.objects.active = arrow_a
bpy.ops.object.join()
icon = bpy.context.object
icon.name = "CycleBlueprintArrows"
icon.data.name = "CycleBlueprintArrows"
icon.scale.x = -1
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
icon["design"] = "Two curved arrows for the Cycle Blueprints PlateUp tile"
icon["intended_material"] = "Glowing Blue Soft"
icon["mirrored_for_original_spin"] = True

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
