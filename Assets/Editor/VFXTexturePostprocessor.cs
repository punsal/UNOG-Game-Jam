using UnityEditor;
using UnityEngine;

/// <summary>
/// Enforces pixel-art import settings for VFX textures only.
/// Scoped strictly to Assets/Art/VFX/ so unrelated sprites are untouched.
/// </summary>
public class VFXTexturePostprocessor : AssetPostprocessor
{
    private const string VFXPath = "Assets/Art/VFX/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(VFXPath))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);
    }
}
