using ExcelDataReader;
using GACourseAndExamSchedule.Algorithm;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;

namespace GACourseAndExamSchedule.Data.Reader
{
    public class PrelectorReader
    {
        #region Constants and Fields

        private string PATH_EXCEL_DATA;
        private readonly string FILENAME_PROFESSOR = ConfigurationManager.AppSettings.Get("data.input.filename.prelectors");

        private const string COLUMN_NAME_ID = "ID";
        private const string COLUMN_NAME_NAME = "İsim";
        private const string COLUMN_NAME_SCHEDULE = "İş Günleri";

        private string _filePath;
        private List<Prelector> _prelectors = new List<Prelector>();

        private int _idColumnNumber = 0;
        private int _nameColumnNumber = 0;
        private int _scheduleColumnNumber = 0;

        private readonly string[] DAYS = { "PAZ", "SAL", "ÇAR", "PER", "CUM" };

        #endregion

        #region Constructors

        public PrelectorReader()
        {
            PATH_EXCEL_DATA = ConfigurationManager.AppSettings.Get("data.input.location");

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = PATH_EXCEL_DATA + (PATH_EXCEL_DATA.EndsWith("\\") ? "" : "\\") + FILENAME_PROFESSOR;
            }
        }

        #endregion

        #region Public Methods

        public void ResetData()
        {
            if (_prelectors != null) _prelectors.Clear();
        }

        public List<Prelector> GetPrelectors()
        {
            CollectPrelectors();

            return _prelectors;
        }

        public Prelector GetPrelectorByName(string name)
        {
            CollectPrelectors();

            return _prelectors.Find(_p => _p.Name.Equals(name));
        }

        public Prelector GetPrelectorById(int id)
        {
            CollectPrelectors();

            return _prelectors.Find(_p => _p.ID.Equals(id));
        }

        public List<Prelector> UpdateCourseClasses(List<CourseClass> courseClasses)
        {
            _prelectors.ForEach(_p =>
            {
                courseClasses.FindAll(_c => _c.Prelector.Equals(_p)).ForEach(_c =>
                {
                    _p.AddCourseClass(_c);
                });
            });

            return _prelectors;
        }

        #endregion

        #region Private Methods

        private void CollectPrelectors()
        {
            if (_prelectors != null && _prelectors.Count != 0) return;

            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read()) { }
                    } while (reader.NextResult());

                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = true,
                        FilterSheet = (tableReader, sheetIndex) => true,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            FilterRow = (rowReader) => {
                                return true;
                            },
                            FilterColumn = (rowReader, columnIndex) => {
                                return true;
                            }
                        }
                    });

                    FindColumnIndexes(result.Tables[0].Columns);

                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        bool[] _scheduleDays = new bool[5];
                        string _scheduleStr = row.ItemArray[_scheduleColumnNumber].ToString().Replace(" ", "");
                        if (string.IsNullOrEmpty(_scheduleStr))
                        {
                            for (int i = 0; i < _scheduleDays.Length; i++)
                            {
                                _scheduleDays.SetValue(true, i);
                            }
                        }
                        else
                        {
                            string[] _scheduleDayList = _scheduleStr.Split(',');
                            List<string> _days = new List<string>();
                            foreach (string _d in _scheduleDayList)
                            {
                                _days.Add(_d.ToUpper().Substring(0, 3));
                            }

                            for (int i = 0; i < DAYS.Length; i++)
                            {
                                _scheduleDays[i] = _days.Contains(DAYS[i]);
                            }
                        }


                        _prelectors.Add(new Prelector
                        (
                            id: int.Parse(row.ItemArray[_idColumnNumber].ToString()),
                            name: row.ItemArray[_nameColumnNumber].ToString(),
                            scheduleDays: _scheduleDays
                        ));
                    }
                }
            }
        }

        private void FindColumnIndexes(DataColumnCollection columnCollection)
        {
            if ((_idColumnNumber == _nameColumnNumber) ||
                (_idColumnNumber == _scheduleColumnNumber) ||
                (_nameColumnNumber == _scheduleColumnNumber))
            {
                for (int i = 0; i < columnCollection.Count; i++)
                {
                    DataColumn column = columnCollection[i];
                    switch (column.ColumnName)
                    {
                        case COLUMN_NAME_ID:
                            _idColumnNumber = i;
                            break;
                        case COLUMN_NAME_NAME:
                            _nameColumnNumber = i;
                            break;
                        case COLUMN_NAME_SCHEDULE:
                            _scheduleColumnNumber = i;
                            break;
                    }
                }
            }
        }

        #endregion

    }
}
