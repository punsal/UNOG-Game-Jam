using UnityEngine;

/// <summary>Snaps world positions to the pixel grid without allocating.</summary>
public static class PixelPositionUtility
{
    public const float DefaultPixelsPerUnit = 16f;

    public static Vector3 Snap(Vector3 worldPosition, float pixelsPerUnit = DefaultPixelsPerUnit)
    {
        worldPosition.x = Mathf.Round(worldPosition.x * pixelsPerUnit) / pixelsPerUnit;
        worldPosition.y = Mathf.Round(worldPosition.y * pixelsPerUnit) / pixelsPerUnit;
        return worldPosition;
    }

    /// <summary>Quantizes a scalar (e.g. a scale or offset) to whole pixel steps.</summary>
    public static float SnapScalar(float value, float pixelsPerUnit = DefaultPixelsPerUnit)
    {
        return Mathf.Round(value * pixelsPerUnit) / pixelsPerUnit;
    }
}
