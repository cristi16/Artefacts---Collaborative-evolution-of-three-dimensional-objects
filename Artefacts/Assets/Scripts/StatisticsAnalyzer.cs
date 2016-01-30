using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StatisticsAnalyzer : MonoBehaviour
{
    List<Statistics> stats = new List<Statistics>();
    //string directory = @"E:\Thesis data\Friday 04-12\Statistics";
    string directory = @"E:\Thesis data\Tuesday 08-12\Statistics";

    void Start()
    {
        var dirs = Directory.GetDirectories(directory);

        foreach (var dir in dirs)
        {
            stats.Add(Statistics.Deserialize(dir + @"\statistics.txt"));
        }
        // number of generated artefacts, maximum generation reached
        //Plot1();

        // number of artefacts resulted from mutation, number of artefacts resulted from crossover
        //Plot2();

        // Color distribution for session i
        for (int i = 0; i < stats.Count; i++)
        {
            Plot3(i + 3, stats[i], i + 1);
        }

        // Number of artefacts evolved per user
        for (int i = 0; i < stats.Count; i++)
        {
            Plot4(stats[i], i + 1);
        }

        // Number of artefacts per generation
        for (int i = 0; i < stats.Count; i++)
        {
            Plot5(stats[i], i + 1);
        }

        // Number of distinct users that interacted with planted artefacts
        for (int i = 0; i < stats.Count; i++)
        {
            Plot6(stats[i], i + 1);
        }

        // Number of seeds picked up per session
        Plot7();

        // Average number of replanted seeds per session
        Plot8();

        // Positions of artefacs
        for (int i = 0; i < stats.Count; i++)
        {
            Plot9(stats[i], i + 1);
        }

        // Avg no of distinct users
        Plot10();
    }

    void Plot10()
    {
        var data = ", Average number of distinct users that interacted with artefacts\n";

        int i = 1;
        foreach (var stat in stats)
        {
            data += "session " + i + ", " + stat.artefacts.Sum(artefact => artefact.Value.usersInteracted.Distinct().Count()) / (float)stat.artefacts.Count + "\n";
            i++;
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\AvgNoOfDistinctUsers";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + ".csv", data);
    }

    void Plot9(Statistics stat, int sessionId)
    {
        var data = "X position, Z position\n";

        foreach (var artefact in stat.artefacts)
        {
            data += artefact.Value.spawnPosition.x.ToString("F2") + "," + artefact.Value.spawnPosition.y.ToString("F2") + "\n";
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\Position Of Artefacts";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + sessionId + ".csv", data);
    }

    void Plot8()
    {
        var data = ", average number of replanted seeds\n";

        int i = 1;
        foreach (var stat in stats)
        {
            var avgNoReplanted = stat.artefacts.Select(x => x.Value.numberOfSeedsReplanted).Average();
            data += "session " + i + ", " + avgNoReplanted + "\n";
            i++;
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\AvgNoOfReplantedSeeds";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + ".csv", data);
    }

    void Plot7()
    {
        var data = ", number of planted artefacts, number of seeds picked up\n";

        int i = 1;
        foreach (var stat in stats)
        {
            var totalOfPickedUpSeeds = stat.players.Select(x => x.Value.numberOfSeedsPickedUp).Sum();
            data += "session " + i + ", " + stat.totalOfPlantedArtefacts + "," + totalOfPickedUpSeeds + "\n";
            i++;
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\NoOfSeedsPickedUp";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + ".csv", data);
    }

    void Plot6(Statistics stat, int sessionId)
    {
        var data = ", Number of distinct users that interacted per planted artefact\n";

        foreach (var artefact in stat.artefacts)
        {
            data += artefact.Value.generation + "," + artefact.Value.usersInteracted.Distinct().Count() + "\n";
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\NoOfDistinctUsersPerArtefact";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + sessionId + ".csv", data);
    }

    void Plot5(Statistics stat, int sessionId)
    {
        var data = "Generation, Number of planted artefacts\n";

        Dictionary<uint, uint> generationDistribution = new Dictionary<uint, uint>();

        foreach (var artefact in stat.artefacts)
        {
            if (generationDistribution.ContainsKey(artefact.Value.generation))
            {
                generationDistribution[artefact.Value.generation] = generationDistribution[artefact.Value.generation] + 1;
            }
            else
            {
                generationDistribution.Add(artefact.Value.generation, 1);
            }
        }

        foreach (var pair in generationDistribution)
        {
            data += pair.Key + "," + pair.Value + "\n";
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\NoOfArtPerGeneration";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + sessionId + ".csv", data);
    }

    void Plot4(Statistics stat, int sessionId)
    {
        var data = ", Number of planted artefacts, Number of planted artefacts the user has not interacted with in its lineage before\n";

        int userCount = 1;
        foreach (var player in stat.players)
        {
            var newObjectsCount = player.Value.plantedObjects.Count(plantedObject => stat.artefacts[plantedObject].usersInteracted.Count(y => y == player.Key) == 1);

            Debug.Log(player.Value.plantedObjects.Count + " ---- " + newObjectsCount);

            data += "user " + userCount + "," + player.Value.plantedObjects.Count + "," + newObjectsCount + "\n";
            userCount++;
        }

        var path = directory.Remove(directory.LastIndexOf(@"\")) + @"\NoOfArtPerUser";
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        File.WriteAllText(path + @"\data" + sessionId + ".csv", data);
    }

    void Plot1()
    {
        var header = ", number of generated artefacts, maximum generation reached\n";

        WriteToPerSession(1, header, (Statistics stat) => stat.totalOfPlantedArtefacts + ", " + stat.maxGeneration);
    }

    void Plot2()
    {
        var header = ", number of artefacts resulted from mutation, number of artefacts resulted from crossover\n";

        WriteToPerSession(2, header, (Statistics stat) => stat.players.Select(x=> x.Value.numberOfMutations).Sum() + ", " + stat.players.Select(x => x.Value.numberOfCrossovers).Sum());
    }

    void Plot3(int id, Statistics stat, int sessionId)
    {
        var data = ", , , Color distribution for session " + sessionId + "\n";
        Dictionary<Vector3, int> colorCount = new Dictionary<Vector3, int>();

        foreach (var artefact in stat.artefacts)
        {
            if (colorCount.ContainsKey(artefact.Value.color))
            {
                colorCount[artefact.Value.color] = colorCount[artefact.Value.color] + 1;
            }
            else
            {
                colorCount.Add(artefact.Value.color, 1);
            }
        }

        foreach (var colorPair in colorCount)
        {
            data += FloatToColor(colorPair.Key.x) + "," + FloatToColor(colorPair.Key.y) + "," +
                    FloatToColor(colorPair.Key.z) + "," + colorPair.Value + "\n";
        }
        File.WriteAllText(directory.Remove(directory.LastIndexOf(@"\")) + @"\data" + id + ".csv", data);
    }

    void WriteToPerSession(int id, string header, Func<Statistics, string> func)
    {
        string data = string.Empty;

        data += header;

        int i = 1;
        foreach (var stat in stats)
        {
            data += "session " + i + ", " + func(stat) + "\n";
            Debug.Log(stat.artefacts.Count);
            i++;
        }

        File.WriteAllText(directory.Remove(directory.LastIndexOf(@"\")) + @"\data" + id + ".csv", data);
    }

    int FloatToColor(float value)
    {
        return (int) (value*255);
    }
}
