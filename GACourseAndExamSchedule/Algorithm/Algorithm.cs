using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GACourseAndExamSchedule.Algorithm
{
    #region Classes

    public enum AlgorithmState
    {
        AS_USER_STOPPED,
        AS_CRITERIA_STOPPED,
        AS_RUNNING,
        AS_SUSPENDED
    };

    public class Algorithm
    {

        #region Constants and Fields

        public static readonly int PARAMETER_CROSSOVER_PROBABILITY = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.crossoverProbability"));
        public static readonly int PARAMETER_MUTAITION_PROBABILITY = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.mutationProbability"));
        public static readonly int PARAMETER_MUTAITION_SIZE = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.mutationSize"));
        public static readonly int PARAMETER_NUMBER_OF_CHROMOSOMES = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.numberOfChromosomes"));
        public static readonly int PARAMETER_NUMBER_OF_CROSSOVER_POINTS = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.numberOfCrossoverPoints"));
        public static readonly int PARAMETER_REPLACE_BY_GENERATION = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.replaceByGeneration"));
        public static readonly int PARAMETER_TRACK_BEST = int.Parse(ConfigurationManager.AppSettings.Get("algorithm.trackBest"));

        private static bool _isExamProblem = false;

        #endregion

        #region Public Properties

        private int _numberOfChromosomes = 1000;
        public int NumberOfChromosomes
        {
            get { return _numberOfChromosomes; }
            set
            {
                if (value < 2) value = 2;
                _numberOfChromosomes = value;
            }
        }

        private int _trackBest = 50;
        public int TrackBest
        {
            get { return _trackBest; }
            set
            {
                if (value < 1) value = 1;
                if (value >= NumberOfChromosomes) value = NumberOfChromosomes - 1;
                _trackBest = value;
            }
        }

        private int _replaceByGeneration = 180;
        public int ReplaceByGeneration
        {
            get { return _replaceByGeneration; }
            set
            {
                if (value < 1) value = 1;
                else if (value > NumberOfChromosomes - TrackBest)
                    value = NumberOfChromosomes - TrackBest;
                _replaceByGeneration = value;
            }
        }

        public int NumberOfCrossoverPoints
        {
            get { return _prototype.NumberOfCrossoverPoints; }
            set
            {
                if (value < 2) value = 2;
                _prototype.NumberOfCrossoverPoints = value;
            }
        }

        public int MutationSize
        {
            get { return _prototype.MutationSize; }
            set
            {
                if (value < 2) value = 2;
                _prototype.MutationSize = value;
            }
        }

        public int CrossoverProbability
        {
            get { return _prototype.CrossoverProbability; }
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _prototype.CrossoverProbability = value;
            }
        }

        public int MutationProbability
        {
            get { return _prototype.MutationProbability; }
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _prototype.MutationProbability = value;
            }
        }
        #endregion

        #region Properties

        public Thread[] MultiThreads = null;
        public int numCore = 0;

        object Locker0 = new object(); // for Lock _state of Algorithm
        object Locker1 = new object(); // for Lock _Chromosome jobs
        object Locker2 = new object(); // for Lock _bestChromosome & _bestFlags job's

        private Schedule[] _chromosomes;

        private bool[] _bestFlags;

        private int[] _bestChromosomes;

        private int _currentBestSize;

        private Schedule.ScheduleObserver _observer;

        public Schedule _prototype;

        private int _currentGeneration;

        internal static AlgorithmState _state;

        private static Algorithm _instance;

        #endregion

        #region Constructors

        public static Algorithm GetInstance()
        {
            if (_instance == null)
            {
                Schedule prototype = new Schedule(PARAMETER_NUMBER_OF_CROSSOVER_POINTS, PARAMETER_MUTAITION_SIZE, PARAMETER_CROSSOVER_PROBABILITY, PARAMETER_MUTAITION_PROBABILITY, _isExamProblem);

                _instance = new Algorithm(PARAMETER_NUMBER_OF_CHROMOSOMES, PARAMETER_REPLACE_BY_GENERATION, PARAMETER_TRACK_BEST, prototype, new Schedule.ScheduleObserver(), _isExamProblem);
            }

            return _instance;
        }

        public static void FreeInstance()
        {
            if (_instance != null)
            {
                _instance._prototype = null;
                _instance._observer = null;
                _instance = null;
            }
        }

        public Algorithm(int numberOfChromosomes, int replaceByGeneration, int trackBest,
            Schedule prototype, Schedule.ScheduleObserver observer, bool isExamProblem)
        {
            _isExamProblem = isExamProblem;
            NumberOfChromosomes = numberOfChromosomes;
            TrackBest = trackBest;
            ReplaceByGeneration = replaceByGeneration;
            _currentBestSize = 0;
            _prototype = prototype;
            _observer = observer;
            _currentGeneration = 0;
            _state = AlgorithmState.AS_USER_STOPPED;

            _chromosomes = new Schedule[NumberOfChromosomes];
            _bestFlags = new bool[NumberOfChromosomes];

            _bestChromosomes = new int[TrackBest];

            for (int i = _chromosomes.Length - 1; i >= 0; --i)
            {
                _chromosomes[i] = null;
                _bestFlags[i] = false;
            }
            _instance = this;

            #region Find number of Active CPU or CPU core's for this Programs
            long Affinity_Dec = System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity.ToInt64();
            string Affinity_Bin = Convert.ToString(Affinity_Dec, 2);
            foreach (char anyOne in Affinity_Bin.ToCharArray())
                if (anyOne == '1') numCore++;
            #endregion
        }

        ~Algorithm()
        {
            Array.Clear(_chromosomes, 0, _chromosomes.Length);
            _chromosomes = null;
            MultiThreads = null;
        }

        #endregion

        #region GA Methods

        public bool Start()
        {
            #region Start by initialize new population 
            if (_prototype == null)
                return false;

            if (Monitor.TryEnter(Locker0, 10))
            {
                if (_state == AlgorithmState.AS_RUNNING)
                {
                    Monitor.Exit(Locker0);
                    return false;
                }
                _state = AlgorithmState.AS_RUNNING;
                Monitor.Exit(Locker0);
            }
            else return false;

            if (_observer != null)
            {
                _observer.EvolutionStateChanged(_state);
            }

            if (!Views.ResultForm._setting.Parallel_Process)
            {
                ClearBest_Sequence();

                for (int i = 0; i < _chromosomes.Length; i++)
                {
                    if (_chromosomes[i] != null)
                        _chromosomes[i] = null;

                    _chromosomes[i] = _prototype.MakeNewFromPrototype();
                    AddToBest_Sequence(i);
                }
            }
            else
            {
                ClearBest_Parallel();

                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = numCore;
                Parallel.For(0, _chromosomes.Length, options, i =>
                {
                    if (_chromosomes[i] != null)
                        _chromosomes[i] = null;

                    _chromosomes[i] = _prototype.MakeNewFromPrototype();
                    AddToBest_Parallel(i);
                });
            }

            _currentGeneration = 0;
            #endregion

            if (Views.ResultForm._setting.Parallel_Process)
            {
                MultiThreads = new Thread[numCore];
                for (int cpu = 0; cpu < numCore; ++cpu)
                {
                    MultiThreads[cpu] = new Thread(new ParameterizedThreadStart(GA_Start));
                    MultiThreads[cpu].Name = "MultiThread_" + cpu.ToString();
                    MultiThreads[cpu].Start(true as object);
                }
                System.Diagnostics.Process.GetCurrentProcess().Threads.AsParallel();
            }
            else
            {
                MultiThreads = new Thread[1];
                MultiThreads[0] = new Thread(new ParameterizedThreadStart(GA_Start));
                MultiThreads[0].Name = "MultiThread_0";
                MultiThreads[0].Start((object)false);
            }
            return true;
        }

        private void GA_Start(object Parallel_Mutex_On)
        {
            if ((Boolean)Parallel_Mutex_On)
            {
                #region GA for Mutex On

                while (true)
                {
                    if (_state == AlgorithmState.AS_CRITERIA_STOPPED || _state == AlgorithmState.AS_USER_STOPPED)
                    {
                        break;
                    }
                    else if (_state == AlgorithmState.AS_SUSPENDED)
                    {
                        if (Thread.CurrentThread.IsAlive)
                            Thread.CurrentThread.Suspend();
                    }

                    Schedule best = GetBestChromosome();

                    if (best.Fitness >= 1)
                    {
                        _state = AlgorithmState.AS_CRITERIA_STOPPED;
                        break;
                    }


                    Schedule[] offspring;
                    offspring = new Schedule[_replaceByGeneration];
                    Random rand = new Random();
                    for (int j = 0; j < _replaceByGeneration; j++)
                    {
                        Schedule p1;
                        Schedule p2;

                        lock (Locker1)
                        {
                            p1 = _chromosomes[(rand.Next() % _chromosomes.Length)].MakeCopy(false);
                        }
                        lock (Locker1)
                        {
                            p2 = _chromosomes[(rand.Next() % _chromosomes.Length)].MakeCopy(false);
                        }

                        offspring[j] = p1.Crossover(p2);
                        lock (Locker1)
                        {
                            offspring[j].Mutation();
                            offspring[j].CalculateFitness();
                        }
                    }

                    for (int j = 0; j < _replaceByGeneration; j++)
                    {
                        int ci;
                        do
                        {
                            ci = rand.Next() % _chromosomes.Length;
                        } while (IsInBest(ci));

                        lock (Locker1)
                        {
                            _chromosomes[ci] = null;
                            _chromosomes[ci] = offspring[j];
                        }
                        AddToBest_Parallel(ci);
                    }

                    if (best != GetBestChromosome())
                    {
                        lock (Locker1)
                        {
                            _observer.NewBestChromosome(GetBestChromosome(), Views.ResultForm._setting.Display_RealTime);
                        }
                    }
                    _currentGeneration++;
                }

                if (_observer != null)
                {
                    lock (Locker0)
                    {
                        _observer.EvolutionStateChanged(_state);
                    }
                }
                Thread.CurrentThread.Abort();
                #endregion
            }
            else
            {
                #region GA for Mutex Off

                while (true)
                {
                    if (_state == AlgorithmState.AS_CRITERIA_STOPPED || _state == AlgorithmState.AS_USER_STOPPED)
                    {
                        break;
                    }
                    else if (_state == AlgorithmState.AS_SUSPENDED)
                    {
                        if (Thread.CurrentThread.IsAlive)
                            Thread.CurrentThread.Suspend();
                    }

                    Schedule best = GetBestChromosome();

                    if (best.Fitness >= 1)
                    {
                        _state = AlgorithmState.AS_CRITERIA_STOPPED;
                        break;
                    }

                    Schedule[] offspring;
                    offspring = new Schedule[_replaceByGeneration];
                    Random rand = new Random();
                    for (int j = 0; j < _replaceByGeneration; j++)
                    {
                        Schedule p1 = _chromosomes[(rand.Next() % _chromosomes.Length)];
                        Schedule p2 = _chromosomes[(rand.Next() % _chromosomes.Length)];

                        offspring[j] = p1.Crossover(p2);
                        offspring[j].Mutation();
                        offspring[j].CalculateFitness();
                    }

                    for (int j = 0; j < _replaceByGeneration; j++)
                    {
                        int ci;
                        do
                        {
                            ci = rand.Next() % _chromosomes.Length;

                        } while (IsInBest(ci));

                        _chromosomes[ci] = null;
                        _chromosomes[ci] = offspring[j];

                        AddToBest_Sequence(ci);
                    }

                    if (best != GetBestChromosome())
                    {
                        _observer.NewBestChromosome(GetBestChromosome(), Views.ResultForm._setting.Display_RealTime);
                    }
                    _currentGeneration++;
                }

                if (_observer != null)
                {
                    _observer.EvolutionStateChanged(_state);
                }

                Thread.CurrentThread.Abort();
                #endregion
            }
        }

        public void Stop()
        {
            if (_state == AlgorithmState.AS_RUNNING)
            {
                _state = AlgorithmState.AS_USER_STOPPED;
            }
            for (int cpu = 0; cpu < MultiThreads.Length; cpu++)
                MultiThreads[cpu].Abort();

            _observer.NewBestChromosome(GetBestChromosome(), true);
        }

        public bool Resume()
        {
            try
            {
                if (_state == AlgorithmState.AS_SUSPENDED)
                {
                    if (Views.ResultForm._setting.Parallel_Process) // For Multi Process
                    {
                        _state = AlgorithmState.AS_RUNNING;

                        for (int cpu = 0; cpu < MultiThreads.Length; cpu++)
                            if (MultiThreads[cpu].ThreadState == ThreadState.Suspended)
                                MultiThreads[cpu].Resume();

                        System.Diagnostics.Process.GetCurrentProcess().Threads.AsParallel();
                    }
                    else
                    {
                        _state = AlgorithmState.AS_RUNNING;

                        if (MultiThreads[0].ThreadState == ThreadState.Suspended)
                            MultiThreads[0].Resume();
                    }
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
                Views.ResultForm._state = ThreadState.Stopped;
                _state = AlgorithmState.AS_USER_STOPPED;
                Thread.Sleep(1000);
                for (int cpu = 0; cpu < MultiThreads.Length; cpu++)
                    if (MultiThreads[cpu].IsAlive)
                        MultiThreads[cpu].Abort();
                return false;
            }
        }

        public bool Pause()
        {
            if (_state == AlgorithmState.AS_RUNNING)
            {
                if (Monitor.TryEnter(Locker0, 10))
                {
                    _state = AlgorithmState.AS_SUSPENDED;
                    Monitor.Exit(Locker0);

                    _observer.NewBestChromosome(GetBestChromosome(), true);
                    return true;
                }
                else return false;
            }
            return false;
        }

        public Schedule GetBestChromosome()
        {
            return _chromosomes[_bestChromosomes[0]];
        }

        public int GetCurrentGeneration() { return _currentGeneration; }

        public Schedule.ScheduleObserver GetObserver() { return _observer; }

        private void AddToBest_Parallel(int chromosomeIndex)
        {
            lock (Locker1)
            {
                if ((_currentBestSize == _bestChromosomes.Length &&
                    _chromosomes[_bestChromosomes[_currentBestSize - 1]].Fitness >=
                    _chromosomes[chromosomeIndex].Fitness) || _bestFlags[chromosomeIndex])
                    return;
            }

            int i = _currentBestSize;
            for (; i > 0; i--)
            {
                if (i < _bestChromosomes.Length)
                {
                    Monitor.Enter(Locker1);
                    if (_chromosomes[_bestChromosomes[i - 1]].Fitness >
                        _chromosomes[chromosomeIndex].Fitness)
                    {
                        Monitor.Exit(Locker1);
                        break;
                    }
                    Monitor.Exit(Locker1);
                    lock (Locker2)
                    {
                        _bestChromosomes[i] = _bestChromosomes[i - 1];
                    }
                }
                else
                    lock (Locker2)
                    {
                        _bestFlags[_bestChromosomes[i - 1]] = false;
                    }
            }

            lock (Locker2)
            {
                _bestChromosomes[i] = chromosomeIndex;
                _bestFlags[chromosomeIndex] = true;
            }

            if (_currentBestSize < _bestChromosomes.Length)
                _currentBestSize++;
        }

        private void AddToBest_Sequence(int chromosomeIndex)
        {
            if ((_currentBestSize == _bestChromosomes.Length &&
                 _chromosomes[_bestChromosomes[_currentBestSize - 1]].Fitness >=
                 _chromosomes[chromosomeIndex].Fitness) || _bestFlags[chromosomeIndex])
                return;

            int i = _currentBestSize;
            for (; i > 0; i--)
            {
                if (i < _bestChromosomes.Length)
                {
                    if (_chromosomes[_bestChromosomes[i - 1]].Fitness >
                        _chromosomes[chromosomeIndex].Fitness)
                        break;

                    _bestChromosomes[i] = _bestChromosomes[i - 1];
                }
                else
                    _bestFlags[_bestChromosomes[i - 1]] = false;
            }

            _bestChromosomes[i] = chromosomeIndex;
            _bestFlags[chromosomeIndex] = true;

            if (_currentBestSize < _bestChromosomes.Length)
                _currentBestSize++;
        }

        private bool IsInBest(int chromosomeIndex)
        {
            return _bestFlags[chromosomeIndex];
        }

        private void ClearBest_Parallel()
        {
            lock (Locker2)
            {
                ParallelOptions option = new ParallelOptions() { MaxDegreeOfParallelism = numCore };
                Parallel.For(_bestFlags.Length - 1, -1, option, i =>
                {
                    _bestFlags[i] = false;
                });
            }
            _currentBestSize = 0;
        }

        private void ClearBest_Sequence()
        {
            for (int i = _bestFlags.Length - 1; i >= 0; --i)
                _bestFlags[i] = false;

            _currentBestSize = 0;
        }

        #endregion

    }

    #endregion
}
