using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using System.Collections.Generic;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNEAT.Core;
using System;
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.Network;

public class MeshEvolutionExperiment
{
#region Private Fields

    NeatEvolutionAlgorithmParameters _eaParams;
    NeatGenomeParameters _neatGenomeParams;
    string _name;
    int _populationSize;
    NetworkActivationScheme _activationScheme;
    string _description;
    int _inputCount;
    int _outputCount;
    private IGenomeDecoder<NeatGenome, IBlackBox> _genomeDecoder;

#endregion Private Fields

    #region Properties

    public string Name
    {
        get { return _name; }
    }

    public string Description
    {
        get { return _description; }
    }

    public int InputCount
    {
        get { return _inputCount; }
    }

    public int OutputCount
    {
        get { return _outputCount; }
    }

    public int DefaultPopulationSize
    {
        get { return _populationSize; }
    }

    public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
    {
        get { return _eaParams; }
    }

    public NeatGenomeParameters NeatGenomeParameters
    {
        get { return _neatGenomeParams; }
    }

    public IGenomeDecoder<NeatGenome, IBlackBox> GenomeDecoder
    {
        get { return _genomeDecoder; }   
    }

    #endregion Properties

    public void Initialize(string name, XmlElement xmlConfig)
    {
        Initialize(name, xmlConfig, 4, 1);
    }

    public void Initialize(string name, XmlElement xmlConfig, int input, int output)
    {
        _name = name;
        _populationSize = XmlUtils.GetValueAsInt(xmlConfig, "PopulationSize");
        _activationScheme = ExperimentUtils.CreateActivationScheme(xmlConfig, "Activation");
        _description = XmlUtils.TryGetValueAsString(xmlConfig, "Description");

        _eaParams = new NeatEvolutionAlgorithmParameters();
        _neatGenomeParams = new NeatGenomeParameters();
        _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;

        _inputCount = input;
        _outputCount = output;
    }

    public List<NeatGenome> LoadPopulation(XmlReader xr)
    {
        CppnGenomeFactory genomeFactory = (CppnGenomeFactory)CreateGenomeFactory();
        return NeatGenomeXmlIO.ReadCompleteGenomeList(xr, true, genomeFactory);
    }

    public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
    {
        NeatGenomeXmlIO.WriteComplete(xw, genomeList, true);
    }

    public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
    {
        return new NeatGenomeDecoder(_activationScheme);
    }

    public IGenomeFactory<NeatGenome> CreateGenomeFactory()
    {
        return new CppnGenomeFactory(InputCount, OutputCount, DefaultActivationFunctionLibrary.CreateLibraryCppn(), _neatGenomeParams);
    }

    public NeatInteractiveEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(string fileName)
    {
        List<NeatGenome> genomeList = null;
        IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();
        try
        {
            if (fileName.Contains("/.pop.xml"))
            {
                throw new Exception();
            }
            using (XmlReader xr = XmlReader.Create(fileName))
            {
                genomeList = LoadPopulation(xr);
            }
        }
        catch (Exception e1)
        {
            //Utility.Log(fileName + " Error loading genome from file!\nLoading aborted.\n"
            //                          + e1.Message + "\nJoe: " + fileName);

            genomeList = genomeFactory.CreateGenomeList(_populationSize, 0);

        }
        return CreateEvolutionAlgorithm(genomeFactory, genomeList);
    }

    public NeatInteractiveEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
    {
        return CreateEvolutionAlgorithm(_populationSize);
    }

    public NeatInteractiveEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
    {
        IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();

        List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

        return CreateEvolutionAlgorithm(genomeFactory, genomeList);
    }

    public NeatInteractiveEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
    {
        _genomeDecoder = CreateGenomeDecoder();

        var ea = new NeatInteractiveEvolutionAlgorithm< NeatGenome>(_eaParams);

        ea.Initialize(genomeList);

        return ea;
    }
}