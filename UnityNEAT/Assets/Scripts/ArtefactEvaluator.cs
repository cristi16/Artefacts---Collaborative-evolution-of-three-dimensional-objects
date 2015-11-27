using System;
using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public class ArtefactEvaluator 
{
    #region Data structs

    public enum InputType { DistanceToCenter, Sphere, Box, Combined }

    [Serializable]
    public struct VoxelVolume
    {
        public int width;
        public int height;
        public int length;
    }

    public struct EvaluationInfo
    {
        public float[,,] cleanOutput;
        public float[,,] processedOutput;
        public float[,,] distanceToCenter;
        public float minOutputValue;
        public float maxOutputValue;     
    }

    #endregion Data structs

    public static InputType DefaultInputType = InputType.DistanceToCenter;
    public static Color artefactColor;

    public static Mesh Evaluate(IBlackBox phenome, VoxelVolume volume, out EvaluationInfo evaluationInfo)
    {
        var processedOutput = new float[volume.width, volume.height, volume.length];
        var cleanOutput = new float[volume.width, volume.height, volume.length];
        var distanceToCenter = new float[volume.width, volume.height, volume.length];
        var minOutputValue = 1f;
        var maxOutputValue = -1f;

        float r = 0, g = 0, b = 0;

        for (int x = 0; x < volume.width; x++)
        {
            for (int y = 0; y < volume.height; y++)
            {
                for (int z = 0; z < volume.length; z++)
                {
                    AssingInput(phenome, volume, x, y, z);

                    // activate phenome
                    phenome.Activate();

                    // store output values
                    ISignalArray outputArr = phenome.OutputSignalArray;
                    processedOutput[x, y, z] = (float)outputArr[0];
                    cleanOutput[x, y, z] = (float)outputArr[0];

                    if (phenome.OutputSignalArray.Length >= 4)
                    {
                        r += (float)outputArr[1];
                        g += (float)outputArr[2];
                        b += (float)outputArr[3];
                    }

                    // store min and max output values
                    if (processedOutput[x, y, z] < minOutputValue)
                        minOutputValue = processedOutput[x, y, z];
                    if (processedOutput[x, y, z] > maxOutputValue)
                        maxOutputValue = processedOutput[x, y, z];

                    // Clamps the final shape within a sphere
                    //distanceToCenter[x, y, z] = DistanceFunctions.DistanceToCenter(x, y, z, volume);
                    //if (DistanceFunctions.DistanceToCenter(x, y, z, volume) > -0.5f)
                    //{
                    //    cleanOutput[x, y, z] = -1;
                    //    processedOutput[x, y, z] = -1;
                    //}

                    // border should have negative values
                    if (x == 0 || x == volume.width - 1 || y == 0 || y == volume.height - 1 || z == 0 || z == volume.length - 1)
                        processedOutput[x, y, z] = -1f;
                }
            }
        }

        r /= volume.width*volume.width*volume.width;
        g /= volume.width*volume.width*volume.width;
        b /= volume.width*volume.width*volume.width;
        artefactColor = new Color((r + 1f) / 2f, (g + 1f) / 2f, (b + 1f) / 2f);
        //Debug.Log(artefactColor);

        //Debug.Log("Output in range [" + minOutputValue + ", " + maxOutputValue + "]");

        // TODO: need to find a workaround for this. It results in cubes. In multiplayer we don't want to spawn cubes as seeds.
        // Either save network to disk, look and see what's wrong or maybe if this happens on the client it could request another regeneration.
        if (Mathf.Approximately(minOutputValue, maxOutputValue))
        {
            Debug.LogWarning("All output values are the same! Min equals max!");
        }

        for (int index00 = 1; index00 < processedOutput.GetLength(0) - 1; index00++)
            for (int index01 = 1; index01 < processedOutput.GetLength(1) - 1; index01++)
                for (int index02 = 1; index02 < processedOutput.GetLength(2) - 1; index02++)
                {
                    // if initially, border value at 0,0,0 was maxFill, we invert all values so minFill is on the border
                    if (Mathf.Approximately(cleanOutput[0, 0, 0], maxOutputValue))
                        processedOutput[index00, index01, index02] = minOutputValue + (maxOutputValue - processedOutput[index00, index01, index02]);
                    // based on a threshold ( middle value between min and max) we change the value to either 0 or 1. This give a look instead of smooth details
                    processedOutput[index00, index01, index02] = processedOutput[index00, index01, index02] < minOutputValue + (maxOutputValue - minOutputValue) / 2f ? 0f : 1f;
                }

        // fill evaluation info struct with data
        evaluationInfo = new EvaluationInfo();
        evaluationInfo.cleanOutput = cleanOutput.Clone() as float[,,];
        evaluationInfo.processedOutput = processedOutput.Clone() as float[,,];
        evaluationInfo.distanceToCenter = distanceToCenter.Clone() as float[,,];
        evaluationInfo.minOutputValue = minOutputValue;
        evaluationInfo.maxOutputValue = maxOutputValue;

        // Apply marching cubes on processed output and return Mesh
        Profiler.BeginSample("MarchingCubes");
        var mesh = MarchingCubes.CreateMesh(processedOutput);
        Profiler.EndSample();

        return mesh;
    }

    private static void AssingInput(IBlackBox phenome, VoxelVolume volume, int x, int y, int z)
    {
        ISignalArray inputArr = phenome.InputSignalArray;
        // input values for x,y,z coord in [-1, 1] range
        inputArr[0] = Mathf.Abs((float)x / (volume.width - 1) * 2 - 1);
        inputArr[1] = Mathf.Abs((float)y / (volume.height - 1) * 2 - 1);
        inputArr[2] = Mathf.Abs((float)z / (volume.length - 1) * 2 - 1);

        var sphereDistance = DistanceFunctions.SphereDistance(x, y, z, volume, volume.width / 4f, new Vector3(volume.width / 2f, volume.height / 2f, volume.length / 2f));
        var boxSize = new Vector3(6, volume.height / 6f, volume.length / 5f);
        var boxDistance = DistanceFunctions.BoxDistance(x, y, z, volume, boxSize, new Vector3(volume.width / 2f, volume.height / 2f, volume.length / 2f));

        switch (DefaultInputType)
        {
            case InputType.Sphere:
                inputArr[3] = sphereDistance;
                break;
            case InputType.Box:
                inputArr[3] = boxDistance;
                break;
            case InputType.Combined:
                inputArr[3] = Mathf.Min(sphereDistance, boxDistance);
                break;
            case InputType.DistanceToCenter:
                inputArr[3] = DistanceFunctions.DistanceToCenter(x, y, z, volume);
                break;
        }
    }
}
