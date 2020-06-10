using GACourseAndExamSchedule.SpannedDataGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GACourseAndExamSchedule.Algorithm
{
    #region Classes

    public class Schedule
    {
        #region Constants and Fields

        public const int DAY_HOURS = 9;
        public const int DAYS_COUNT = 5;
        public int day_Hours { get { return DAY_HOURS; } }
        public int day_count { get { return DAYS_COUNT; } }

        private const int NUMBER_OF_SCORES = 50;

        public int NumberOfCrossoverPoints { get; set; }
        public int MutationSize { get; set; }
        public int CrossoverProbability { get; set; }
        public int MutationProbability { get; set; }
        public float Fitness { get; set; }
        public bool[] Criteria { get; set; }

        List<CourseClass>[] _slots;
        Dictionary<CourseClass, int> _classes = new Dictionary<CourseClass, int>();
        public Dictionary<CourseClass, int> GetClasses() { return _classes; }

        private bool _isExamProblem = false;

        #endregion

        #region Constructors

        public Schedule(int numberOfCrossoverPoints, int mutationSize, int crossoverProbability, int mutationProbability, bool isExamProblem)
        {
            _isExamProblem = isExamProblem;
            MutationSize = mutationSize;
            NumberOfCrossoverPoints = numberOfCrossoverPoints;
            CrossoverProbability = crossoverProbability;
            MutationProbability = mutationProbability;
            Fitness = 0;

            _slots = new List<CourseClass>[(DAYS_COUNT * DAY_HOURS * Configuration.GetInstance.GetNumberOfRooms())];
            for (int p = 0; p < (DAYS_COUNT * DAY_HOURS * Configuration.GetInstance.GetNumberOfRooms()); p++)
                _slots[p] = new List<CourseClass>();

            Criteria = new bool[(Configuration.GetInstance.GetNumberOfCourseClasses() * 8)];
        }

        public Schedule(Schedule c, bool setupOnly)
        {
            if (setupOnly)
            {
                _slots = new List<CourseClass>[(DAYS_COUNT * DAY_HOURS * Configuration.GetInstance.GetNumberOfRooms())];
                for (int ptr = 0; ptr < (DAYS_COUNT * DAY_HOURS * Configuration.GetInstance.GetNumberOfRooms()); ptr++)
                    _slots[ptr] = new List<CourseClass>();

                Criteria = new bool[(Configuration.GetInstance.GetNumberOfCourseClasses() * 8)];
            }
            else
            {
                _slots = c._slots;
                _classes = c._classes;
                Criteria = c.Criteria;
                Fitness = c.Fitness;
            }
            _isExamProblem = c._isExamProblem;

            NumberOfCrossoverPoints = c.NumberOfCrossoverPoints;
            MutationSize = c.MutationSize;
            CrossoverProbability = c.CrossoverProbability;
            MutationProbability = c.MutationProbability;
        }

        #endregion

        #region Public Methods

        public Schedule MakeCopy(bool setupOnly)
        {
            return new Schedule(this, setupOnly);
        }

        public Schedule MakeNewFromPrototype()
        {
            Schedule newChromosome = new Schedule(this, true);

            Random _rnd = new Random();
            List<CourseClass> _ccS = Configuration.GetInstance.GetCourseClasses().OrderBy(x => x.Course.ID).ToList();
            int _numberOfRooms = Configuration.GetInstance.GetNumberOfRooms();
            List<int> _labIds = Configuration.GetInstance.Rooms.Where(x => x.Value.IsLab).Select(x => x.Key).ToList();
            List<int> _roomIdsThatsNotLab = Configuration.GetInstance.Rooms.Where(x => !x.Value.IsLab).Select(x => x.Key).ToList();
            int _day = 0, _time = 0;
            foreach (CourseClass _cc in _ccS)
            {
                int _dur = _cc.Duration;
                int _roomId = 0;
                if (_cc.RequiresLab)
                {
                    _roomId = _labIds[_rnd.Next() % _labIds.Count];
                } else
                {
                    _roomId = _roomIdsThatsNotLab[_rnd.Next() % _roomIdsThatsNotLab.Count];
                }
                int _index = _ccS.IndexOf(_cc);
                if (!_isExamProblem || _index == 0 || _cc.Course != _ccS[_index - 1].Course)
                {
                    _day = _rnd.Next() % DAYS_COUNT;
                    _time = _rnd.Next() % (DAY_HOURS + 1 - _dur);
                }
                int _pos = (_roomId * DAY_HOURS * DAYS_COUNT) + (_day * DAY_HOURS) + _time;

                for (int i = _dur - 1; i >= 0; i--)
                    newChromosome._slots[_pos + i].Add(_cc);

                if (newChromosome._classes == null)
                {
                    newChromosome._classes = new Dictionary<CourseClass, int>();
                }
                newChromosome._classes.Add(_cc, _pos);
            }

            newChromosome.CalculateFitness();

            return newChromosome;
        }

        public Schedule Crossover(Schedule parent2)
        {
            Random _rnd = new Random();

            if (_rnd.Next() % 100 > CrossoverProbability)
                return new Schedule(this, false);

            Schedule _child = new Schedule(this, true);

            int _size = _classes.Count;
            bool[] cp = new bool[_size];

            for (int i = NumberOfCrossoverPoints; i > 0; i--)
            {
                while (true)
                {
                    int p = _rnd.Next() % _size;
                    if (!cp[p])
                    {
                        cp[p] = true;
                        break;
                    }
                }
            }

            List<KeyValuePair<CourseClass, int>> _parent1ccs = _classes.ToList<KeyValuePair<CourseClass, int>>();

            List<KeyValuePair<CourseClass, int>> _parent2ccs = parent2._classes.ToList<KeyValuePair<CourseClass, int>>();

            bool first = (_rnd.Next() % 2 == 0);
            for (int i = 0; i < _size; i++)
            {
                if (first)
                {
                    _child._classes.Add(_parent1ccs[i].Key, _parent1ccs[i].Value);
                    for (int j = _parent1ccs[i].Key.Duration - 1; j >= 0; j--)
                        _child._slots[_parent1ccs[i].Value + j].Add(_parent1ccs[i].Key);
                }
                else
                {
                    _child._classes.Add(_parent2ccs[i].Key, _parent2ccs[i].Value);
                    for (int j = _parent2ccs[i].Key.Duration - 1; j >= 0; j--)
                        _child._slots[_parent2ccs[i].Value + j].Add(_parent2ccs[i].Key);
                }

                if (cp[i])
                    first = !first;
            }

            return _child;
        }

        public void Mutation()
        {
            Random _rnd = new Random();

            if (_rnd.Next() % 100 > MutationProbability)
                return;

            int _numberOfClasses = _classes.Count;
            for (int i = MutationSize; i > 0; i--)
            {
                int mpos = _rnd.Next() % _numberOfClasses;
                KeyValuePair<CourseClass, int> it = _classes.ToList<KeyValuePair<CourseClass, int>>()[mpos];

                int _pos1 = it.Value;
                CourseClass _cc1 = it.Key;

                int _nr = Configuration.GetInstance.GetNumberOfRooms();
                List<int> _labIds = Configuration.GetInstance.Rooms.Where(x => x.Value.IsLab).Select(x => x.Key).ToList();
                List<int> _roomIdsThatsNotLab = Configuration.GetInstance.Rooms.Where(x => !x.Value.IsLab).Select(x => x.Key).ToList();
                int _dur = _cc1.Duration;
                int _day = _rnd.Next() % DAYS_COUNT;
                int _roomId = 0;
                if (_cc1.RequiresLab)
                {
                    _roomId = _labIds[_rnd.Next() % _labIds.Count];
                }
                else
                {
                    _roomId = _roomIdsThatsNotLab[_rnd.Next() % _roomIdsThatsNotLab.Count];
                }
                int _time = _rnd.Next() % (DAY_HOURS + 1 - _dur);
                int _pos2 = (_roomId * DAY_HOURS * DAYS_COUNT) + (_day * DAY_HOURS) + _time;
                if (_slots[_pos2].Count > 1)
                {
                    _roomId = _rnd.Next() % _nr;
                    _pos2 = (_roomId * DAY_HOURS * DAYS_COUNT) + (_day * DAY_HOURS) + _time;
                }

                for (int j = _dur - 1; j >= 0; j--)
                {
                    List<CourseClass> cl = _slots[_pos1 + j];
                    _slots[_pos1 + j].Remove(_cc1);
                    _slots[_pos2 + j].Add(_cc1);
                }

                _classes[_cc1] = _pos2;

                if (_isExamProblem)
                {
                    List<KeyValuePair<CourseClass, int>> _classesWithSameCourse = _classes.Where(x => x.Key.Course == it.Key.Course).ToList();
                    if (_classesWithSameCourse.Count > 0)
                    {
                        foreach (KeyValuePair<CourseClass, int> sameCourse in _classesWithSameCourse)
                        {
                            if (sameCourse.Key == it.Key) continue;

                            int _posOld = sameCourse.Value;
                            int _newRoom = _rnd.Next() % _nr;
                            int _posNew = (_newRoom * DAY_HOURS * DAYS_COUNT) + (_day * DAY_HOURS) + _time;

                            for (int j = _dur - 1; j >= 0; j--)
                            {
                                List<CourseClass> cl = _slots[_posOld + j];
                                foreach (CourseClass It in cl)
                                {
                                    if (It == sameCourse.Key)
                                    {
                                        cl.Remove(It);
                                        break;
                                    }
                                }

                                _slots[_posNew + j].Add(sameCourse.Key);
                            }

                            _classes[sameCourse.Key] = _posNew;
                        }

                    }
                }
            }            
        }

        public void CalculateFitness()
        {
            if (_isExamProblem)
            {
                CalculateExamScheduleFitness();
            } else
            {
                CalculateCourseScheduleFitness();
            }
        }

        #endregion

        #region Private Methods

        public void CalculateExamScheduleFitness()
        {
            int _score = 0;

            int _numberOfRooms = Configuration.GetInstance.GetNumberOfRooms();
            int _daySize = DAY_HOURS * DAYS_COUNT;
            int _ci = 0;

            foreach (KeyValuePair<CourseClass, int> it in _classes.ToList())
            {
                int _pos = it.Value;
                int _roomId = _pos / _daySize;
                int _dayTime = _pos % _daySize;
                int _day = _dayTime / DAY_HOURS;
                int _time = _dayTime % DAY_HOURS;
                int _dur = it.Key.Duration;

                CourseClass _cc = it.Key;
                Room _room = Configuration.GetInstance.GetRoomById(_roomId);

                #region Score 1 (check for room overlapping of classes)                                                                          [+10]

                bool _overlapping = false;
                for (int i = _dur - 1; i >= 0; i--)
                {
                    if (_slots[_pos + i].Count > 1)
                    {
                        _overlapping = true;
                        break;
                    }
                }

                if (!_overlapping)
                    _score += 10;

                Criteria[_ci + 0] = !_overlapping;

                #endregion

                #region Score 2 (does current room have enough seats)                                                                            [+7]

                Criteria[_ci + 1] = _room.Capacity >= _cc.StudentCount;
                if (Criteria[_ci + 1])
                    _score += 7;

                #endregion

                #region Score 3 (does current room fair)                                                                                         [+3]

                Criteria[_ci + 2] = _cc.RequiresLab.Equals(_room.IsLab);
                if (Criteria[_ci + 2])
                    _score += 3;

                #endregion

                #region Score 4 and 5 (check for overlapping of classes for branches and student groups)                                         [+8][+10]
                
                List<CourseClass> _courseClassesOnSameTime = new List<CourseClass>();
                for (int j = 0; j < _numberOfRooms; j++) 
                {
                    for (int i = 0; i < _dur; i++)
                    {
                        foreach (CourseClass cc in _slots[(j * DAYS_COUNT * DAY_HOURS) + (_day * DAY_HOURS) + _time + i])
                        {
                            if (cc != _cc && !_courseClassesOnSameTime.Contains(cc)) _courseClassesOnSameTime.Add(cc);
                        }
                    }
                }

                bool _bra = false, _gro = false;
                foreach (CourseClass it_cc in _courseClassesOnSameTime)
                {
                    if (_cc != it_cc)
                    {
                        if (!_bra && _cc.BranchsOverlaps(it_cc))
                            _bra = true;

                        if (!_gro && _cc.GroupsOverlap(it_cc))
                            _gro = true;

                        if (_bra && _gro) break;
                    }
                }

                if (!_bra)
                    _score += 8;
                Criteria[_ci + 3] = !_bra;

                if (!_gro)
                    _score += 10;
                Criteria[_ci + 4] = !_gro;

                #endregion

                #region Score 6 (check for same course exams in same time)                                                                       [+7]

                List<CourseClass> _courseClassesWithSameCourse = _classes.Keys.Where(x => x.Course.ID == _cc.Course.ID).ToList();
                _courseClassesWithSameCourse.Remove(_cc);

                bool _sameExamsNotInSameTime = false;
                if (_courseClassesWithSameCourse.Count > 0)
                {
                    foreach (CourseClass it_cc in _courseClassesWithSameCourse)
                    {
                        if (!_sameExamsNotInSameTime && !_courseClassesOnSameTime.Contains(it_cc)) _sameExamsNotInSameTime = true;
                    }
                }

                if (!_sameExamsNotInSameTime)
                    _score += 7;
                Criteria[_ci + 5] = !_sameExamsNotInSameTime;

                #endregion

                #region Score 7 (check difficulty limit in one day for student groups)                                                           [+3]

                List<CourseClass> _courseClassesOnSameDay = new List<CourseClass>();
                for (int k = 0; k < _numberOfRooms; k++)
                {
                    for (int t = 0; t < DAY_HOURS; t++)
                    {
                        foreach (CourseClass cc in _slots[(DAYS_COUNT * DAY_HOURS * k) + (_day * DAY_HOURS) + t])
                        {
                            if (!_courseClassesOnSameDay.Contains(cc)) _courseClassesOnSameDay.Add(cc);
                        }
                    }
                }

                bool _limitExceeded = false;
                foreach (StudentGroup group in _cc.StudentGroups)
                {
                    List<CourseClass> _courseClassesOnSameDayForGroup = new List<CourseClass>();
                    int _diffInDay = 0;
                    foreach (CourseClass cc_it in _courseClassesOnSameDay)
                    {
                        if (_limitExceeded) break;

                        if (!_courseClassesOnSameDayForGroup.Contains(cc_it) && cc_it.StudentGroups.Contains(group))
                        {
                            _courseClassesOnSameDayForGroup.Add(cc_it);
                            _diffInDay += cc_it.Difficulty;
                        }
                    }

                    if (!_limitExceeded && _diffInDay > group.MaxDifficultyInDay)
                    {
                        _limitExceeded = true;
                        break;
                    }
                }

                if (!_limitExceeded)
                {
                    _score += 3;
                }
                Criteria[_ci + 6] = !_limitExceeded;


                #endregion

                #region Score 8 (check this exam day in prelector schedule table)                                                                [+2]

                Criteria[_ci + 7] = _cc.Prelector.ScheduleDays[_day];
                if (Criteria[_ci + 7]) _score += 2;

                #endregion

                _ci += 8;
            }

            Fitness = (float)_score / (Configuration.GetInstance.GetNumberOfCourseClasses() * NUMBER_OF_SCORES);
        }

        public void CalculateCourseScheduleFitness()
        {
            int _score = 0;
            int _numberOfRooms = Configuration.GetInstance.GetNumberOfRooms();
            int _daySize = DAY_HOURS * DAYS_COUNT;
            int _ci = 0;
            foreach (KeyValuePair<CourseClass, int> it in _classes.ToList())
            {
                int _pos = it.Value;
                int _roomId = _pos / _daySize;
                int _dayTime = _pos % _daySize;
                int _day = _dayTime / DAY_HOURS;
                int _time = _dayTime % DAY_HOURS;
                int _dur = it.Key.Duration;

                CourseClass _cc = it.Key;
                Room _room = Configuration.GetInstance.GetRoomById(_roomId);

                #region Score 1 (check for room overlapping of classes)                                                                     [+10]

                bool _overlapping = false;
                for (int i = 0; i < _dur; i++)
                {
                    if (_slots[_pos + i].Count > 1)
                    {
                        _overlapping = true;
                        break;
                    }
                }

                if (!_overlapping)
                    _score += 10;

                Criteria[_ci + 0] = !_overlapping;

                #endregion

                #region Score 2 (does current room have enough seats)                                                                       [+7]

                Criteria[_ci + 1] = _room.Capacity >= _cc.StudentCount;
                if (Criteria[_ci + 1]) _score += 7;

                #endregion

                #region Score 3 (does current room fair)                                                                                    [+4]

                Criteria[_ci + 2] = _cc.RequiresLab.Equals(_room.IsLab);
                if (Criteria[_ci + 2]) _score += 4;

                #endregion

                #region Score 4 and 5 and 6 (check overlapping of classes for prelectors and student groups and sequential student groups)  [+10][+10][+5]

                bool _pre = false, _gro = false, _seqGro = false;
                List<CourseClass> _courseClassesOnSameTime = new List<CourseClass>();
                for (int j = 0; j < _numberOfRooms; j++)
                {
                    for (int i = 0; i < _dur; i++) 
                    {
                        List<CourseClass> _ccs = _slots[(j * DAYS_COUNT * DAY_HOURS) + (_day * DAY_HOURS) + _time + i];
                        foreach(CourseClass cc in _ccs)
                        {
                            if (cc.ID != _cc.ID && !_courseClassesOnSameTime.Contains(cc)) _courseClassesOnSameTime.Add(cc);
                        }
                    }
                }

                foreach (CourseClass it_cc in _courseClassesOnSameTime)
                {
                    if (!_pre && _cc.PrelectorOverlaps(it_cc))
                        _pre = true;

                    if (!_gro && _cc.GroupsOverlap(it_cc))
                        _gro = true;

                    if (!_seqGro && _cc.SequentialGroupsOverlap(it_cc))
                        _seqGro = true;

                    if (_pre && _gro && _seqGro) break;
                }

                if (!_pre)
                    _score += 10;
                Criteria[_ci + 3] = !_pre;

                if (!_gro)
                    _score += 10;
                Criteria[_ci + 4] = !_gro;

                if (!_seqGro)
                    _score += 5;
                Criteria[_ci + 5] = !_seqGro;

                #endregion

                #region Score 7 (check course limit in one day for student groups)                                                          [+2]

                List<CourseClass> _courseClassesOnSameDay = new List<CourseClass>();
                for (int k = 0; k < _numberOfRooms; k++)
                {
                    for (int t = 0; t < DAY_HOURS; t++)
                    {
                        foreach(CourseClass cc in _slots[(DAYS_COUNT * DAY_HOURS * k) + (_day * DAY_HOURS) + t])
                        {
                            if (!_courseClassesOnSameDay.Contains(cc)) _courseClassesOnSameDay.Add(cc);
                        }
                    }
                }

                bool _limitExceeded = false;
                foreach (StudentGroup group in _cc.StudentGroups)
                {
                    List<CourseClass> _courseClassesInSameDayForGroup = new List<CourseClass>();
                    int _totalCourseHourInADayForGroup = 0;
                    foreach (CourseClass cc_it in _courseClassesOnSameDay)
                    {
                        if (_limitExceeded) break;

                        if (!_courseClassesInSameDayForGroup.Contains(cc_it) && cc_it.StudentGroups.Contains(group))
                        {
                            _courseClassesInSameDayForGroup.Add(cc_it);
                            _totalCourseHourInADayForGroup += cc_it.Duration;
                        }
                    }

                    if (!_limitExceeded && _totalCourseHourInADayForGroup > group.MaxHourInDay)
                    {
                        _limitExceeded = true;
                        break;
                    }
                }

                if (!_limitExceeded)
                {
                    _score += 2;
                }
                Criteria[_ci + 6] = !_limitExceeded;

                #endregion

                #region Score 8 (check this class day in prelector schedule table)                                                          [+2]

                Criteria[_ci + 7] = _cc.Prelector.ScheduleDays[_day];
                if (Criteria[_ci + 7]) _score += 2;

                #endregion

                _ci += 8;
            }

            Fitness = (float)_score / (Configuration.GetInstance.GetNumberOfCourseClasses() * NUMBER_OF_SCORES);
        }

        #endregion

        public class ScheduleObserver
        {
            private static CreateDataGridViews _window;

            public void NewBestChromosome(Schedule newChromosome, bool showGraphical)
            {
                showGraphical = newChromosome.Fitness > 0.8;
                if (_window.DgvList.Count > 0)
                    _window.SetSchedule(newChromosome, showGraphical);
            }

            public void EvolutionStateChanged(AlgorithmState newState)
            {
                if (_window != null)
                    _window.SetNewState(newState);
            }

            public void SetWindow(CreateDataGridViews window)
            { _window = window; }
        }
    }

    #endregion
}
