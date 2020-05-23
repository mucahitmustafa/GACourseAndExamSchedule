using ExcelDataReader;
using GACourseAndExamSchedule.Algorithm;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;

namespace GACourseAndExamSchedule.Data.Reader
{
    public class StudentGroupReader
    {
        #region Constants and Fields

        private string PATH_EXCEL_DATA;
        private readonly string FILENAME_GROUP = ConfigurationManager.AppSettings.Get("data.input.filename.studentGroups");

        private const string COLUMN_NAME_ID = "ID";
        private const string COLUMN_NAME_BRANCH = "Bölüm";
        private const string COLUMN_NAME_DEGREE = "Sınıf";
        private const string COLUMN_NAME_COUNT = "Öğrenci Sayısı";
        private const string COLUMN_NAME_MAX_HOUR = "Günlük Max Ders Saati";
        private const string COLUMN_NAME_MAX_DIFF = "Günlük Max Sınav Zorluğu";

        private string _filePath;
        private List<StudentGroup> _studentGroups = new List<StudentGroup>();

        private int _idColumnNumber = 0;
        private int _branchColumnNumber = 0;
        private int _degreeColumnNumber = 0;
        private int _countColumnNumber = 0;
        private int _maxHourColumnNumber = 0;
        private int _maxDiffColumnNumber = 0;

        #endregion

        #region Constructors

        public StudentGroupReader()
        {
            PATH_EXCEL_DATA = ConfigurationManager.AppSettings.Get("data.input.location");

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = PATH_EXCEL_DATA + (PATH_EXCEL_DATA.EndsWith("\\") ? "" : "\\") + FILENAME_GROUP;
            }
        }

        #endregion

        #region Public Methods

        public void ResetData()
        {
            if (_studentGroups != null) _studentGroups.Clear();
        }

        public List<StudentGroup> GetStudentGroups()
        {
            CollectStudentGroups();

            return _studentGroups;
        }

        public List<string> GetBranchs()
        {
            CollectStudentGroups();
            
            List<string> _branchs = new List<string>();

            _studentGroups.ForEach(g => {
                if (!_branchs.Contains(g.Branch)) _branchs.Add(g.Branch);
            });

            return _branchs;
        }

        public StudentGroup GetStudentGroupByName(string name)
        {
            CollectStudentGroups();

            return _studentGroups.Find(_s => _s.Name.Equals(name));
        }

        public StudentGroup GetStudentGroupByBranch(string branch)
        {
            CollectStudentGroups();

            return _studentGroups.Find(_s => _s.Branch.Equals(branch));
        }

        public StudentGroup GetStudentGroupById(int id)
        {
            CollectStudentGroups();

            return _studentGroups.Find(_s => _s.ID.Equals(id));
        }

        public List<StudentGroup> UpdateCourseClasses(List<CourseClass> courseClasses)
        {
            _studentGroups.ForEach(_sg =>
            {
                courseClasses.FindAll(_cc => _cc.StudentGroups.FindAll(_g => _g.Equals(_sg)).Count > 0).ForEach(_c =>
                {
                    _sg.AddCourseClass(_c);
                });
            });

            return _studentGroups;
        }

        #endregion

        #region Private Methods

        private void CollectStudentGroups()
        {
            if (_studentGroups != null && _studentGroups.Count != 0) return;

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
                        _studentGroups.Add(new StudentGroup
                        (
                            id: int.Parse(row.ItemArray[_idColumnNumber].ToString()),
                            branch: row.ItemArray[_branchColumnNumber].ToString(),
                            degree: int.Parse(row.ItemArray[_degreeColumnNumber].ToString()),
                            count: int.Parse(row.ItemArray[_countColumnNumber].ToString()),
                            maxHourInDay: int.Parse(row.ItemArray[_maxHourColumnNumber].ToString()),
                            maxDiffInDay: int.Parse(row.ItemArray[_maxDiffColumnNumber].ToString())
                        ));
                    }
                }
            }
        }

        private void FindColumnIndexes(DataColumnCollection columnCollection)
        {
            if ((_idColumnNumber == _branchColumnNumber) ||
                (_idColumnNumber == _degreeColumnNumber) ||
                (_idColumnNumber == _countColumnNumber) ||
                (_idColumnNumber == _maxHourColumnNumber) ||
                (_idColumnNumber == _maxDiffColumnNumber) ||

                (_branchColumnNumber == _degreeColumnNumber) ||
                (_branchColumnNumber == _countColumnNumber) ||
                (_branchColumnNumber == _maxHourColumnNumber) ||
                (_branchColumnNumber == _maxDiffColumnNumber) ||

                (_degreeColumnNumber == _countColumnNumber) ||
                (_degreeColumnNumber == _maxHourColumnNumber) ||
                (_degreeColumnNumber == _maxDiffColumnNumber) ||

                (_countColumnNumber == _maxHourColumnNumber) ||
                (_degreeColumnNumber == _maxDiffColumnNumber) ||

                (_countColumnNumber == _maxDiffColumnNumber))
            {
                for (int i = 0; i < columnCollection.Count; i++)
                {
                    DataColumn column = columnCollection[i];
                    switch (column.ColumnName)
                    {
                        case COLUMN_NAME_ID:
                            _idColumnNumber = i;
                            break;
                        case COLUMN_NAME_BRANCH:
                            _branchColumnNumber = i;
                            break;
                        case COLUMN_NAME_DEGREE:
                            _degreeColumnNumber = i;
                            break;
                        case COLUMN_NAME_COUNT:
                            _countColumnNumber = i;
                            break;
                        case COLUMN_NAME_MAX_HOUR:
                            _maxHourColumnNumber = i;
                            break;
                        case COLUMN_NAME_MAX_DIFF:
                            _maxDiffColumnNumber = i;
                            break;
                    }
                }
            }
        }

        #endregion

    }
}
