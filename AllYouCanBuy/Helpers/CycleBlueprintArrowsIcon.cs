using System;
using System.IO;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    internal static class CycleBlueprintArrowsIcon
    {
        private const string ResourceName = "AllYouCanBuy.Assets.CycleBlueprintArrowsIcon.rgba";
        private const float PixelsPerUnit = 25.6f;

        internal static Texture2D CreateTexture()
        {
            CycleBlueprintClientLog.Info($"Opening embedded icon resource {ResourceName}");
            using var stream = typeof(CycleBlueprintArrowsIcon).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Missing embedded icon resource {ResourceName}");
            using var reader = new BinaryReader(stream);

            if (reader.ReadByte() != 'A' || reader.ReadByte() != 'Y' ||
                reader.ReadByte() != 'C' || reader.ReadByte() != 'I')
            {
                throw new InvalidDataException("Invalid Cycle Blueprints icon header");
            }

            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            CycleBlueprintClientLog.Info($"Reading cycle icon resource; bytes={stream.Length}; size={width}x{height}");
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Cycle Blueprint Arrows Icon",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.LoadRawTextureData(reader.ReadBytes(width * height * 4));
            texture.Apply(false, true);
            CycleBlueprintClientLog.Info("Cycle icon texture loaded and made non-readable");
            return texture;
        }

        internal static Sprite CreateSprite(Texture2D texture)
        {
            CycleBlueprintClientLog.Info(
                $"Creating cycle icon sprite from texture={texture.width}x{texture.height}; pixelsPerUnit={PixelsPerUnit}"
            );
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit
            );
            sprite.name = "Cycle Blueprint Arrows Icon";
            return sprite;
        }
    }
}
