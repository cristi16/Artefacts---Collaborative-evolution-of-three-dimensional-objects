using UnityEngine;
using System.Collections;

public static class DistanceFunctions
{
    public static Vector3 GetPointFromCenter(int x, int y, int z, ArtefactEvaluator.VoxelVolume volume, Vector3 center)
    {
        return new Vector3(Mathf.Abs(x - center.x), Mathf.Abs(y - center.y), Mathf.Abs(z - center.z));
    }

    public static Vector3 GetPointFromCenter(int x, int y, int z, ArtefactEvaluator.VoxelVolume volume)
    {
        return new Vector3(Mathf.Abs(x - volume.width / 2f), Mathf.Abs(y - volume.height / 2f), Mathf.Abs(z - volume.length / 2f));
    }

    public static float DistanceToCenter(int x, int y, int z, ArtefactEvaluator.VoxelVolume volume)
    {
        var point = GetPointFromCenter(x, y, z, volume);
        var maxDistance = Mathf.Max(volume.width, volume.height, volume.length) * Mathf.Sqrt(3) / 2f;
        var normalized = point.sqrMagnitude/ (maxDistance * maxDistance);
        normalized = normalized * 2 - 1;
        return normalized;
    }

    public static float SphereDistance(int x, int y, int z, ArtefactEvaluator.VoxelVolume volume, float radius, Vector3 center)
    {
        var point = GetPointFromCenter(x, y, z, volume, center);
        var distance = point.sqrMagnitude / (radius * radius);
        if (distance > 1)
            distance *= 100f;
        return distance;
    }

    public static float BoxDistance(int x, int y, int z, ArtefactEvaluator.VoxelVolume volume, Vector3 boxSize, Vector3 center)
    {
        var point = GetPointFromCenter(x, y, z, volume, center);
        var distanceToBox = (center + boxSize) - point;

        var maxDimension = Mathf.Max(distanceToBox.x, distanceToBox.y, distanceToBox.z);

        if (maxDimension == distanceToBox.x)
            maxDimension /= boxSize.x;
        if (maxDimension == distanceToBox.y)
            maxDimension /= boxSize.y;
        if (maxDimension == distanceToBox.z)
            maxDimension /= boxSize.z;

        if (point.x > boxSize.x || point.y > boxSize.y || point.z > boxSize.z)
            return 100f;
        else
            return maxDimension;

    }
}
