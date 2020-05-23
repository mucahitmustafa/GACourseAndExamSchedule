using GACourseAndExamSchedule.Algorithm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GACourseAndExamSchedule.SpannedDataGrid
{
    public class CreateDataGridViews
    {
        #region Properties_Base

        private Schedule _schedule;
        private bool _running;
        private Form _resultWindow;

        #endregion

        #region Properties

        Dictionary<int, Room> roomList = new Dictionary<int, Room>();

        Dictionary<int, DataGridView> dgvList = new Dictionary<int, DataGridView>();
        public Dictionary<int, DataGridView> DgvList { get { return dgvList; } }

        static Point LastDGV_Location;
        static int Distance = 20 + 582; // 362: data grid view height, 20: space

        #endregion

        #region Constructors

        public CreateDataGridViews(Dictionary<int, Room> rooms, Form rFrm)
        {
            _schedule = null;
            _running = false;
            _resultWindow = rFrm;
            roomList = rooms;
            LastDGV_Location = new Point(12, 25);
            Create_FirstTime();

            foreach (KeyValuePair<int, DataGridView> kvp in DgvList)
            {
                _resultWindow.Controls["pnlRooms"].Controls.Add(kvp.Value);
            }
            _resultWindow.Controls["pnlRooms"].Height -= 70;
        }

        ~CreateDataGridViews()
        {
            _schedule = null;
            _resultWindow.Dispose();
            roomList.Clear();
            dgvList.Clear();
        }

        #endregion

        #region Methods

        private void Create_FirstTime()
        {
            foreach (KeyValuePair<int, Room> ri in roomList)
            {
                string _headerText = $"{ri.Value.Name}";
                if (ri.Value.IsLab) _headerText = _headerText + " (Lab)";

                dgvList.Add(ri.Key, Standard_dgv(LastDGV_Location, "dgv_" + ri.Value.ID.ToString(), _headerText));
                dgvList[ri.Key].ClearSelection();
                LastDGV_Location.Y += Distance;
            }
        }

        private DataGridView Standard_dgv(Point location, string Name, string headerText)
        {
            DataGridView dgv = new DataGridView();
            dgv.ScrollBars = ScrollBars.None;
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumnEx TimeSpans = new DataGridViewTextBoxColumnEx();
            DataGridViewTextBoxColumnEx MON = new DataGridViewTextBoxColumnEx();
            DataGridViewTextBoxColumnEx TUE = new DataGridViewTextBoxColumnEx();
            DataGridViewTextBoxColumnEx WED = new DataGridViewTextBoxColumnEx();
            DataGridViewTextBoxColumnEx THUR = new DataGridViewTextBoxColumnEx();
            DataGridViewTextBoxColumnEx FRI = new DataGridViewTextBoxColumnEx();

            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            TimeSpans.DefaultCellStyle = dataGridViewCellStyle2;
            TimeSpans.Frozen = true;
            TimeSpans.HeaderText = headerText;
            TimeSpans.Name = "TimeSpan";
            TimeSpans.ReadOnly = true;
            TimeSpans.SortMode = DataGridViewColumnSortMode.NotSortable;
            TimeSpans.ToolTipText = headerText;
            
            MON.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            MON.DefaultCellStyle = dataGridViewCellStyle3;
            MON.FillWeight = 90F;
            MON.HeaderText = "PAZARTESİ";
            MON.Name = "PZT";
            MON.SortMode = DataGridViewColumnSortMode.NotSortable;
            
            TUE.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TUE.DefaultCellStyle = dataGridViewCellStyle3;
            TUE.FillWeight = 90F;
            TUE.HeaderText = "SALI";
            TUE.Name = "SAL";
            TUE.SortMode = DataGridViewColumnSortMode.NotSortable;
           
            WED.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            WED.DefaultCellStyle = dataGridViewCellStyle3;
            WED.FillWeight = 90F;
            WED.HeaderText = "ÇARŞAMBA";
            WED.Name = "ÇAR";
            WED.SortMode = DataGridViewColumnSortMode.NotSortable;
            
            THUR.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            THUR.DefaultCellStyle = dataGridViewCellStyle3;
            THUR.FillWeight = 90F;
            THUR.HeaderText = "PERŞEMBE";
            THUR.Name = "PER";
            THUR.SortMode = DataGridViewColumnSortMode.NotSortable;
            
            FRI.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            FRI.DefaultCellStyle = dataGridViewCellStyle3;
            FRI.FillWeight = 90F;
            FRI.HeaderText = "CUMA";
            FRI.Name = "CUM";
            FRI.SortMode = DataGridViewColumnSortMode.NotSortable;
            
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
            dgv.BorderStyle = BorderStyle.Fixed3D;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Sunken;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Times New Roman", 11F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.Columns.AddRange(new DataGridViewColumn[] {
                TimeSpans,
                MON,
                TUE,
                WED,
                THUR,
                FRI});
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = SystemColors.Window;
            dataGridViewCellStyle10.Font = new Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            dataGridViewCellStyle10.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = DataGridViewTriState.True;
            dgv.DefaultCellStyle = dataGridViewCellStyle10;
            dgv.GridColor = SystemColors.ActiveCaption;
            dgv.Location = location;
            dgv.Name = Name;
            dgv.RightToLeft = RightToLeft.No;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            dgv.RowHeadersVisible = false;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 60;
            dgv.RowTemplate.Resizable = DataGridViewTriState.False;
            dgv.CausesValidation = true;
            dgv.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            dgv.Size = new Size(748, 582);
            dgv.ReadOnly = true;
            dgv.RightToLeft = RightToLeft.No;
            dgv.CellContentClick += new DataGridViewCellEventHandler(dgv_CellContentClick);
            dgv.CellMouseUp += new DataGridViewCellMouseEventHandler(dgv_CellMouseUp);
            dgv.MultiSelect = false;
            
            for (int i = 9; i < 18; i++)
            {
                dgv.Rows.Add(i.ToString() + " - " + (i + 1).ToString());
            }
            return dgv;
        }

        private void dgv_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            ((DataGridView)sender).ClearSelection();
        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ((DataGridView)sender).ClearSelection();
        }

        public static void MergeRows(DataGridView gridView)
        {
            int _lenght = 1;
            int _row = 0;
            bool Repeated = false;
            Random _rnd = new Random();
            for (int columnIndex = 0; columnIndex < gridView.ColumnCount; ++columnIndex)
            {
                for (int rowIndex = 0; rowIndex < gridView.RowCount - 1; ++rowIndex)
                {
                    _row = rowIndex;
                    _lenght = 1;
                Loop:
                    if (gridView[columnIndex, rowIndex].Value != null && gridView[columnIndex, rowIndex + 1].Value != null)
                    {
                        if (gridView[columnIndex, rowIndex].Value.ToString() == gridView[columnIndex, rowIndex + 1].Value.ToString())
                        {
                            _lenght++;
                            if (rowIndex + 1 < gridView.RowCount - 1)
                            {
                                rowIndex++;
                                Repeated = true;
                                goto Loop;
                            }
                            else
                            {
                                ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                            }
                        }
                        else if (Repeated)
                        {
                            ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                            Repeated = false;
                        }
                    }
                    else if (Repeated)
                    {
                        ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                        Repeated = false;
                    }
                }
            }
        }

        public static void MergeRows(DataGridView gridView, Color mergedCells_Color)
        {
            int _lenght = 1;
            int _row = 0;
            bool Repeated = false;
            Random _rnd = new Random();
            for (int columnIndex = 0; columnIndex < gridView.ColumnCount; ++columnIndex)
            {
                for (int rowIndex = 0; rowIndex < gridView.RowCount - 1; ++rowIndex)
                {
                    _row = rowIndex;
                    _lenght = 1;
                Loop:
                    if (gridView[columnIndex, rowIndex].Value != null && gridView[columnIndex, rowIndex + 1].Value != null)
                    {
                        if (gridView[columnIndex, rowIndex].Value.ToString() == gridView[columnIndex, rowIndex + 1].Value.ToString())
                        {
                            _lenght++;
                            if (rowIndex + 1 < gridView.RowCount - 1)
                            {
                                rowIndex++;
                                Repeated = true;
                                goto Loop;
                            }
                            else
                            {
                                ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                            }
                        }
                        else if (Repeated)
                        {
                            ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                            Repeated = false;
                        }
                    }
                    else if (Repeated)
                    {
                        ((DataGridViewTextBoxCellEx)gridView[columnIndex, _row]).RowSpan = _lenght;
                        Repeated = false;
                    }
                }
            }
        }

        object Locker = new object();
        public void SetSchedule(Schedule schedule, bool showGraphical)
        {
            _schedule = schedule.MakeCopy(false);
            if (Monitor.TryEnter(Locker, 500))
            {
                SetText(schedule.Fitness.ToString());
                Monitor.Exit(Locker);
            }
            else return;
            if (showGraphical)
            {
                foreach (KeyValuePair<int, DataGridView> it in dgvList)
                {
                    ClearDataGridView(it.Value);
                }
                int numberOfRooms = Configuration.GetInstance.GetNumberOfRooms();
                int daySize = schedule.day_Hours * numberOfRooms;
                Random rand = new Random();
                foreach (KeyValuePair<CourseClass, int> it in schedule.GetClasses().ToList())
                {
                    int pos = it.Value;
                    int day = pos / daySize;
                    int time = pos % daySize;
                    int room = time / schedule.day_Hours;
                    time = time % schedule.day_Hours;

                    int dur = it.Key.Duration;

                    CourseClass cc = it.Key;
                    Room r = Configuration.GetInstance.GetRoomById(room);
                    string groups_Name = "";
                    foreach (var gs in cc.StudentGroups)
                    {
                        groups_Name += gs.Name + "  ";
                    }
                    groups_Name = groups_Name.Trim();

                    ((DataGridViewTextBoxCellEx)dgvList[r.ID][day + 1, time]).RowSpan = cc.Duration;
                    dgvList[r.ID][day + 1, time].Value = string.Format(CultureInfo.CurrentCulture,
                        "{0}\r\n{1}\r\n{2}", cc.Course.Name, cc.Prelector.Name, groups_Name);
                }
            }
        }

        public void SetNewState(AlgorithmState state)
        {
            _running = state == AlgorithmState.AS_RUNNING;
        }

        public void OnFileStop()
        {
            Algorithm.Algorithm.GetInstance().Stop();
        }

        #endregion

        #region Safely Thread Codes

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (_resultWindow.Controls["lblFitness"].InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                _resultWindow.Invoke(d, new object[] { text });
            }
            else
            {
                _resultWindow.Controls["lblFitness"].Text = text;
            }
        }

        delegate void ClearCallback(DataGridView dgv);

        private void ClearDataGridView(DataGridView dgv)
        {
            if (_resultWindow.Controls["pnlRooms"].Controls[dgv.Name].InvokeRequired)
            {
                ClearCallback d = new ClearCallback(ClearDataGridView);
                _resultWindow.Controls["pnlRooms"].Invoke(d, new object[] { dgv });
            }
            else
            {
                dgv.Rows.Clear();
                for (int i = 9; i < 18; i++)
                {
                    dgv.Rows.Add(string.Format(CultureInfo.CurrentCulture, "{0} - {1}", i, (i + 1)));
                }
            }
        }

        #endregion
    }
}
