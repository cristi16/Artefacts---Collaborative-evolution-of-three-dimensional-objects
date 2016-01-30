using SharpNeat.Core;
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;

public class EvolutionHelper
{
    public int InputCount { get; private set; }
    public int OutputCount { get; private set; }

    public CppnGenomeFactory GenomeFactory;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 4;

    private static EvolutionHelper _instance;
    public static EvolutionHelper Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            else
            {
                return _instance = new EvolutionHelper(k_numberOfInputs, k_numberOfOutputs);
            }
        }
    }

    public EvolutionHelper(int inputCount, int outputCount)
    {
        InputCount = inputCount;
        OutputCount = outputCount;

        GenomeFactory = CreateGenomeFactory();
    }

    public NeatGenome CreateInitialGenome()
    {
        return GenomeFactory.CreateGenome(0);
    }

    public NeatGenome MutateGenome(NeatGenome genome)
    {
        return genome.CreateOffspring(genome.BirthGeneration + 1);
    }

    private CppnGenomeFactory CreateGenomeFactory()
    {
        var neatGenomeParams = new NeatGenomeParameters();
        neatGenomeParams.FeedforwardOnly = true;
        return new CppnGenomeFactory(InputCount, OutputCount, DefaultActivationFunctionLibrary.CreateLibraryCppn(), neatGenomeParams);
    }
}
