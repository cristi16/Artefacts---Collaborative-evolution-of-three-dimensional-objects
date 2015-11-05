using SharpNeat.Core;
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;

public class EvolutionHelper 
{
    public static int InputCount { get; private set; }
    public static int OutputCount { get; private set; }

    private CppnGenomeFactory genomeFactory;

    public EvolutionHelper(int inputCount, int outputCount)
    {
        InputCount = inputCount;
        OutputCount = outputCount;

        genomeFactory = CreateGenomeFactory();
    }

    public NeatGenome CreateInitialGenome()
    {
        return genomeFactory.CreateGenome(0);
    }

    public NeatGenome MutateGenome(NeatGenome genome)
    {
        return genome.CreateOffspring(genome.BirthGeneration + 1);
    }

    public static CppnGenomeFactory CreateGenomeFactory()
    {
        var neatGenomeParams = new NeatGenomeParameters();
        neatGenomeParams.FeedforwardOnly = true;
        return new CppnGenomeFactory(InputCount, OutputCount, DefaultActivationFunctionLibrary.CreateLibraryCppn(), neatGenomeParams);
    }
}
