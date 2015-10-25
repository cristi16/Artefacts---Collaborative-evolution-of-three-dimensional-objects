using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpNeat.Core;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.SpeciationStrategies;
using SharpNeat.Utility;
using System.Collections;

namespace SharpNeat.EvolutionAlgorithms
{
    /// <summary>
    /// Interactive evolution implementation of the NEAT evolution algorithm. 
    /// Incorporates:
    ///     - Creating offspring via both sexual and asexual reproduction.
    /// </summary>
    /// <typeparam name="TGenome">The genome type that the algorithm will operate on.</typeparam>
    public class NeatInteractiveEvolutionAlgorithm<TGenome> : AbstractInteractiveEvolutionAlgorithm<TGenome>
        where TGenome : class, IGenome<TGenome>
    {
        NeatEvolutionAlgorithmParameters _eaParams;

        readonly FastRandom _rng = new FastRandom();
        readonly NeatAlgorithmStats _stats;

        #region Constructors

        /// <summary>
        /// Constructs with the default NeatEvolutionAlgorithmParameters
        /// </summary>
        public NeatInteractiveEvolutionAlgorithm()
        {
            _eaParams = new NeatEvolutionAlgorithmParameters();
            _stats = new NeatAlgorithmStats(_eaParams);
        }

        /// <summary>
        /// Constructs with the provided NeatEvolutionAlgorithmParameters and ISpeciationStrategy.
        /// </summary>
        public NeatInteractiveEvolutionAlgorithm(NeatEvolutionAlgorithmParameters eaParams)
        {
            _eaParams = eaParams;
            _stats = new NeatAlgorithmStats(_eaParams);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of all current genomes. The current population of genomes.
        /// </summary>
        public IList<TGenome> GenomeList
        {
            get { return _genomeList; }
        }

        /// <summary>
        /// Gets the algorithm statistics object.
        /// </summary>
        public NeatAlgorithmStats Statistics
        {
            get { return _stats; }
        }

        #endregion

        #region Public Methods [Initialization]

        /// <summary>
        /// Initializes the evolution algorithm with the provided initial population of genomes.
        /// </summary>
        /// <param name="genomeList">An initial genome population.</param>
        public override void Initialize(List<TGenome> genomeList)
        {
            base.Initialize( genomeList);
            Initialize();
        }

        /// <summary>
        /// Initializes the evolution algorithm with the provided IGenomeFactory that can be used to create an initial population of genomes.
        /// </summary>
        /// <param name="genomeFactory">The factory that was used to create the genomeList and which is therefore referenced by the genomes.</param>
        /// <param name="populationSize">The number of genomes to create for the initial population.</param>
        public override void Initialize(IGenomeFactory<TGenome> genomeFactory, int populationSize)
        {
            base.Initialize(genomeFactory, populationSize);
            Initialize();
        }

        /// <summary>
        /// Code common to both public Initialize methods.
        /// </summary>
        private void Initialize()
        {
        }

        #endregion

        #region Evolution Algorithm Main Method [PerformOneGeneration]

        /// <summary>
        /// Progress forward by one generation. Perform one generation/iteration of the evolution algorithm.
        /// </summary>
        protected override IEnumerator PerformOneGeneration()
        {
            // Create offspring.
            _genomeList = CreateAsexualOffspring(1);

            // Update stats and store reference to best genome.
            UpdateStats();

            yield return null;
        }

        #endregion

        #region Private Methods [High Level Algorithm Methods. CreateOffspring]

        /// <summary>
        /// Create the required number of offspring genomes from asexual reproduction
        /// </summary>
        private List<TGenome> CreateAsexualOffspring(int offspringCount)
        {
            List<TGenome> offspringList = new List<TGenome>(offspringCount);

            for (int i = 0; i < GenomeList.Count; i++)
            {
                TGenome offspring = GenomeList[i].CreateOffspring(_currentGeneration);
                offspringList.Add(offspring);
            }
            _stats._asexualOffspringCount += (ulong) offspringCount;
            _stats._totalOffspringCount += (ulong)offspringCount;
            return offspringList;
        }

        /// <summary>
        /// Create the required number of offspring genomes from sexual reproduction
        /// </summary>
        private List<TGenome> CreateSexualOffspring(int offspringCount)
        {
            List<TGenome> offspringList = new List<TGenome>(offspringCount);

            for (int i = 0; i < GenomeList.Count; i++)
            {
                TGenome offspring = GenomeList[i].CreateOffspring(GenomeList[_rng.NextInt() % GenomeList.Count] , _currentGeneration);
                offspringList.Add(offspring);
            }
            _stats._sexualOffspringCount += (ulong)offspringCount;
            _stats._totalOffspringCount += (ulong)offspringCount;
            return offspringList;
        }
        #endregion

        #region Private Methods [Low Level Helper Methods]

        /// <summary>
        /// Updates the NeatAlgorithmStats object.
        /// </summary>
        private void UpdateStats()
        {
            _stats._generation = _currentGeneration;

            //complexity stats.
            double totalComplexity = _genomeList[0].Complexity;
            double maxComplexity = totalComplexity;

            int count = _genomeList.Count;
            for (int i = 1; i < count; i++)
            {
                totalComplexity += _genomeList[i].Complexity;
                maxComplexity = Math.Max(maxComplexity, _genomeList[i].Complexity);
            }

            _stats._maxComplexity = maxComplexity;
            _stats._meanComplexity = totalComplexity / count;

            // Moving averages.
            _stats._prevComplexityMA = _stats._complexityMA.Mean;
            _stats._complexityMA.Enqueue(_stats._meanComplexity);
        }

        #endregion
    }
}
