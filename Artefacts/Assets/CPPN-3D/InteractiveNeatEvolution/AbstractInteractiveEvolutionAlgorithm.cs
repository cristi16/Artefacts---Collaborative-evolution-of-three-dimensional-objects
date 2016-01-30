using System;
using System.Collections.Generic;
using System.Threading;
using SharpNeat.Core;
using UnityEngine;
using System.Collections;

namespace SharpNeat.EvolutionAlgorithms
{
    /// <summary>
    /// Abstract class providing some common/baseline data and methods for an interactive evolution algorithm
    /// </summary>
    /// <typeparam name="TGenome">The genome type that the algorithm will operate on.</typeparam>
    public abstract class AbstractInteractiveEvolutionAlgorithm<TGenome> where TGenome : class, IGenome<TGenome>
    {
        #region Instance Fields

        protected List<TGenome> _genomeList;
        protected int _populationSize;

        // Algorithm state data.
        private bool _isInitialized = false;
        protected uint _currentGeneration;

        #endregion

        #region Events

        /// <summary>
        /// Notifies listeners that another step in the evolution has been performed.
        /// </summary>
        public Action UpdateEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current generation.
        /// </summary>
        public uint CurrentGeneration
        {
            get { return _currentGeneration; }
        }

        #endregion

        #region IEvolutionAlgorithm<TGenome> Members

        /// <summary>
        /// Initializes the evolution algorithm with the provided  initial population of genomes.
        /// </summary>
        /// <param name="genomeList">An initial genome population.</param>
        public virtual void Initialize(List<TGenome> genomeList)
        {
            _currentGeneration = 0;
            _genomeList = genomeList;
            _populationSize = _genomeList.Count;
            _isInitialized = true;
        }

        /// <summary>
        /// Initializes the evolution algorithm with the provided IGenomeFactory that can be used to create an initial population of genomes.
        /// </summary>
        /// <param name="genomeFactory">The factory that was used to create the genomeList and which is therefore referenced by the genomes.</param>
        /// <param name="populationSize">The number of genomes to create for the initial population.</param>
        public virtual void Initialize(IGenomeFactory<TGenome> genomeFactory, int populationSize)
        {
            _currentGeneration = 0;
            _genomeList = genomeFactory.CreateGenomeList(populationSize, _currentGeneration);
            _populationSize = populationSize;
            _isInitialized = true;
        }

        /// <summary>
        /// Evolves and determines the next generation of gemones from the current one
        /// </summary>
        public void EvolveOneStep()
        {
            if (_isInitialized == false)
            {
                Debug.LogError("Trying to start EA before initializing it with a population");
            }
            else
            {
                Coroutiner.StartCoroutine(PerformEvolution());
            }
        }

        #endregion

        #region Private/Protected Methods [Evolution Algorithm]

        private IEnumerator PerformEvolution()
        {
            _currentGeneration++;
            yield return Coroutiner.StartCoroutine(PerformOneGeneration());
            if (UpdateEvent != null)
                UpdateEvent();
        }

        /// <summary>
        /// Progress forward by one generation. Perform one generation/cycle of the evolution algorithm.
        /// </summary>
        protected abstract IEnumerator PerformOneGeneration();

        #endregion
    }
}
