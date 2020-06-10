using GACourseAndExamSchedule.Algorithm;
using GACourseAndExamSchedule.Data.Reader;
using GACourseAndExamSchedule.SpannedDataGrid;
using StatisticsRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GACourseAndExamSchedule.Views
{
    #region Classes

    public partial class ResultForm : Form
    {
        #region Constants and Fields

        private string PARAMETER_CROSSOVER_PROBABILITY = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.crossoverProbability");
        private string PARAMETER_NUMBER_OF_CROSSOVER_POINTS = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.numberOfCrossoverPoints");
        private string PARAMETER_MUTAITION_PROBABILITY = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.mutationProbability");
        private string PARAMETER_MUTAITION_SIZE = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.mutationSize");
        private string PARAMETER_NUMBER_OF_CHROMOSOMES = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.numberOfChromosomes");
        private string PARAMETER_TRACK_BEST = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.trackBest");
        private string PARAMETER_REPLACE_BY_GENERATION = System.Configuration.ConfigurationManager.AppSettings.Get("algorithm.replaceByGeneration");

        public static CreateDataGridViews _createGridView;
        public static Algorithm.Algorithm _algorithm;
        public static ThreadState _state = new ThreadState();
        public static Setting _setting = new Setting(Environment.ProcessorCount > 1);

        Data.Writer.ExcelDataWriter _excelWriter = new Data.Writer.ExcelDataWriter();
        StatisticsRecorderBase _statisticsRecorder;

        CourseClassReader courseClassReader;
        CourseReader courseReader = new CourseReader();
        PrelectorReader prelectorReader = new PrelectorReader();
        RoomReader roomReader = new RoomReader();
        StudentGroupReader studentGroupReader = new StudentGroupReader();

        List<Room> rooms;
        List<Course> courses;
        List<Prelector> prelectors;
        List<StudentGroup> studentGroups;
        List<CourseClass> courseClasses;

        int StartedTick = 0;
        object TimerControler = new object();
        private bool _isExamProblem = false;

        #endregion

        #region Constructors

        public ResultForm(bool isExamProblem)
        {
            InitializeComponent();
            _isExamProblem = isExamProblem;
            courseClassReader = new CourseClassReader(isExamProblem);
        }

        private void ResultForm_Load(object sender, EventArgs e)
        {
            rooms = roomReader.GetRooms();
            courses = courseReader.GetCourses();
            prelectors = prelectorReader.GetPrelectors();
            studentGroups = studentGroupReader.GetStudentGroups();

            courseClassReader.ResetData();
            if (_isExamProblem)
            {
                courseClasses = courseClassReader.GetCourseClasses();
            } else
            {
                courseClasses = courseClassReader.GetCourseClassesWithoutRoomSplit();
            }

            prelectors = prelectorReader.UpdateCourseClasses(courseClasses);
            studentGroups = studentGroupReader.UpdateCourseClasses(courseClasses);

            Configuration.GetInstance.InitializeDate(prelectors, studentGroups, courses, rooms, courseClasses);

            btnPause.Enabled = false;
            btnStop.Enabled = false;

            if (Algorithm.Configuration.GetInstance.GetNumberOfRooms() > 0)
            {
                _createGridView = new CreateDataGridViews(Configuration.GetInstance.Rooms, this);
                Schedule prototype = new Schedule(int.Parse(PARAMETER_NUMBER_OF_CROSSOVER_POINTS), int.Parse(PARAMETER_MUTAITION_SIZE),
                                                    int.Parse(PARAMETER_CROSSOVER_PROBABILITY), int.Parse(PARAMETER_MUTAITION_PROBABILITY), _isExamProblem);
                Schedule.ScheduleObserver sso = new Schedule.ScheduleObserver();
                sso.SetWindow(_createGridView);

                _algorithm = new Algorithm.Algorithm(int.Parse(PARAMETER_NUMBER_OF_CHROMOSOMES), int.Parse(PARAMETER_REPLACE_BY_GENERATION),
                                                        int.Parse(PARAMETER_TRACK_BEST), prototype, sso, _isExamProblem);

                _state = ThreadState.Unstarted;
                timerWorkingSet.Start();
            }
            else
            {
                MessageBox.Show("Not found any room!", "Number of Rooms Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                timerWorkingSet.Stop();
                _algorithm = null;
                Dispose();
                return;
            }

            if (Configuration.GetInstance.GetNumberOfCourseClasses() <= 0)
            {
                btnStart.Enabled = false;
            }
        }
        private void ResultForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_state == ThreadState.Running)
            {
                _state = ThreadState.Stopped;
                Algorithm.Algorithm.GetInstance().Stop();
                StartedTick = 0;
            }
            else if (_state == ThreadState.Suspended)
            {
                _state = ThreadState.Stopped;
                Algorithm.Algorithm.GetInstance().Resume();
                Algorithm.Algorithm.GetInstance().Stop();
            }
        }

        private void ResultForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            if (Algorithm.Algorithm.GetInstance().MultiThreads != null)
            {
                if (Algorithm.Algorithm.GetInstance().MultiThreads.Length > 0)
                {
                    for (int th = 0; th < Algorithm.Algorithm.GetInstance().MultiThreads.Length; th++)
                    {
                        if (Algorithm.Algorithm.GetInstance().MultiThreads[th].IsAlive)
                            Algorithm.Algorithm.GetInstance().MultiThreads[th].Abort();
                    }
                }
                Algorithm.Algorithm.GetInstance().MultiThreads = null;
            }
        }

        #endregion

        #region Event Handler
        private void timerWorkingSet_Tick(object sender, EventArgs e)
        {
            lblGeneration.Text = Algorithm.Algorithm.GetInstance().GetCurrentGeneration().ToString();

            if (_state == ThreadState.Running || _state == ThreadState.WaitSleepJoin || _state == ThreadState.Suspended)
            {
                int timeLenght = (Environment.TickCount - StartedTick) / 1000;

                string S = (timeLenght % 60).ToString();
                string M = ((timeLenght / 60) % 60).ToString();
                string H = (timeLenght / 3600).ToString();
                S = (S.Length > 1) ? S : S.Insert(0, "0");
                M = (M.Length > 1) ? M : M.Insert(0, "0");
                H = (H.Length > 1) ? H : H.Insert(0, "0");
                lblTime.Text = string.Format("{0}:{1}:{2}", H, M, S);
            }

            Monitor.Enter(TimerControler);
            Monitor.Exit(TimerControler);

            if (Algorithm.Algorithm._state == AlgorithmState.AS_CRITERIA_STOPPED)
            {
                btnStop_Click(sender, e);
                Algorithm.Algorithm._state = AlgorithmState.AS_USER_STOPPED;
                MessageBox.Show("Çizelge hazırlandı!.", "Algoritma Tamamlandı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _statisticsRecorder = new StatisticsRecorderBase();
            if (_state == ThreadState.Unstarted || _state == ThreadState.Stopped)
            {
                if (Algorithm.Algorithm.GetInstance().Start())
                {
                    lblFitness.Text = "0.00000";
                    btnPause.Enabled = true;
                    btnStop.Enabled = true;
                    btnStart.Enabled = false;
                    btnPause.Text = "&Duraklat";
                    _state = ThreadState.Running;
                    StartedTick = Environment.TickCount;
                }
            }
            else if (_state == ThreadState.Suspended)
            {
                if (Algorithm.Algorithm.GetInstance().Resume())
                {
                    btnPause.Enabled = true;
                    btnStop.Enabled = true;
                    btnStart.Enabled = false;
                    btnStart.Text = "&Başlat";
                    btnPause.Text = "&Duraklat";
                    _state = ThreadState.Running;
                }
            }
            btnPauseTimer.Enabled = true;
            statisticsTimer.Start();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (_state == ThreadState.Running)
            {
                if (Algorithm.Algorithm.GetInstance().Pause())
                {
                    timerWorkingSet.Stop();
                    _state = ThreadState.Suspended;
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    btnPause.Text = "&Duraklat";
                    btnStop.Enabled = true;
                    btnStart.Text = "&Devam Ettir";
                    if (ResultForm._setting.AutoSave_OnStopped)
                    {
                        Save(Algorithm.Algorithm.GetInstance().GetBestChromosome());
                    }
                }
            }
            else if (_state == ThreadState.Stopped || _state == ThreadState.Aborted)
            {
                Save(Algorithm.Algorithm.GetInstance().GetBestChromosome());
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnPause.Text = "&Duraklat";
                btnStop.Enabled = false;
            }
            this.Cursor = Cursors.Default;
            btnPauseTimer.Enabled = false;
            statisticsTimer.Stop();
        }

        private void Save(Schedule schedule)
        {
            _excelWriter.CreateExcelTables(schedule, schedule.GetClasses().ToList(), _isExamProblem);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnStart.Text = "&Başlat";
            btnPause.Enabled = false;
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnPauseTimer.Enabled = false;
            if (_state == ThreadState.Running)
            {
                _state = ThreadState.Stopped;
                Algorithm.Algorithm.GetInstance().Stop();
                StartedTick = 0;
            }
            else if (_state == ThreadState.Suspended)
            {
                _state = ThreadState.Stopped;
                Algorithm.Algorithm.GetInstance().Resume();
                Algorithm.Algorithm.GetInstance().Stop();
            }
            if (ResultForm._setting.AutoSave_OnStopped)
            {
                Save(Algorithm.Algorithm.GetInstance().GetBestChromosome());
            }
            this.Cursor = Cursors.Default;
            statisticsTimer.Stop();
        }
        private void btnPauseTimer_Click(object sender, EventArgs e)
        {
            if (btnPauseTimer.Text == "Süreyi Duraklat")
            {
                timerWorkingSet.Stop();
                btnPauseTimer.Text = "Süreyi Sürdür";
            }
            else
            {
                timerWorkingSet.Start();
                btnPauseTimer.Text = "Süreyi Duraklat";
            }
        }

        private void statisticsTimer_Tick(object sender, EventArgs e)
        {
            int _timeSec = ConvertTimeToSeconds(lblTime.Text);
            int _generation = int.Parse(lblGeneration.Text);
            float _fitness = float.Parse(lblFitness.Text);

            _statisticsRecorder.InsertData(_timeSec, _generation, _fitness);
        }

        #endregion

        #region Private Methods

        private int ConvertTimeToSeconds(string timeStr)
        {
            string _hour = timeStr.Substring(0, 2).Replace(":", "");
            string _min = timeStr.Substring(timeStr.IndexOf(":") + 1, 2).Replace(":", "");
            string _sec = timeStr.Substring(timeStr.LastIndexOf(":")).Replace(":", "");

            return int.Parse(_sec) + (int.Parse(_min) * 60) + (int.Parse(_hour) * 3600);
        }
        
        #endregion

    }

    public struct Setting
    {
        public Setting(bool Parallel)
        {
            Display_RealTime = true;
            Parallel_Process = Parallel;
            AutoSave_OnStopped = true;
            Show_ActivityMonitor = false;
            Fragmental_Classes = false;
        }

        public bool Display_RealTime;
        public bool Parallel_Process;
        public bool AutoSave_OnStopped;
        public bool Show_ActivityMonitor;
        public bool Fragmental_Classes;
    };

    #endregion
}
