using System;
using System.IO;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    internal static class CycleBlueprintArrowsMesh
    {
        private const string ResourceName = "AllYouCanBuy.Assets.CycleBlueprintArrows.mesh";

        internal static Mesh Create()
        {
            using var stream = typeof(CycleBlueprintArrowsMesh).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Missing embedded mesh resource {ResourceName}");
            using var reader = new BinaryReader(stream);

            if (reader.ReadByte() != 'A' || reader.ReadByte() != 'Y' ||
                reader.ReadByte() != 'C' || reader.ReadByte() != 'B')
            {
                throw new InvalidDataException("Invalid Cycle Blueprints mesh header");
            }

            var vertexCount = reader.ReadInt32();
            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var triangles = new int[vertexCount];

            for (var i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                triangles[i] = i;
            }

            var mesh = new Mesh
            {
                name = "Cycle Blueprint Arrows",
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
