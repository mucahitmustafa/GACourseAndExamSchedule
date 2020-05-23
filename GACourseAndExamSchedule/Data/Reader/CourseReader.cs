using ExcelDataReader;
using GACourseAndExamSchedule.Algorithm;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;

namespace GACourseAndExamSchedule.Data.Reader
{
    public class CourseReader
    {
        #region Constants and Fields

        private string PATH_EXCEL_DATA;
        private readonly string FILENAME_COURSE = ConfigurationManager.AppSettings.Get("data.input.filename.courses");

        private const string COLUMN_NAME_ID = "ID";
        private const string COLUMN_NAME_NAME = "İsim";

        private string _filePath;
        private List<Course> _courses = new List<Course>();

        private int _idColumnNumber = 0;
        private int _nameColumnNumber = 0;

        #endregion

        #region Constructors

        public CourseReader()
        {
            PATH_EXCEL_DATA = ConfigurationManager.AppSettings.Get("data.input.location");

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = PATH_EXCEL_DATA + (PATH_EXCEL_DATA.EndsWith("\\") ? "" : "\\") + FILENAME_COURSE;
            }
        }

        #endregion

        #region Public Methods

        public void ResetData()
        {
            if (_courses != null) _courses.Clear();
        }

        public List<Course> GetCourses()
        {
            CollectCourses();

            return _courses;
        }

        public Course GetCourseByName(string name)
        {
            CollectCourses();

            return _courses.Find(_c => _c.Name.Equals(name));
        }

        public Course GetCourseById(int id)
        {
            CollectCourses();

            return _courses.Find(_c => _c.ID.Equals(id));
        }

        #endregion

        #region Private Methods

        private void CollectCourses()
        {
            if (_courses != null && _courses.Count != 0) return;

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
                        _courses.Add(new Course
                        {
                            ID = int.Parse(row.ItemArray[_idColumnNumber].ToString()),
                            Name = row.ItemArray[_nameColumnNumber].ToString()
                        });
                    }
                }
            }
        }

        private void FindColumnIndexes(DataColumnCollection columnCollection)
        {
            if ((_idColumnNumber == _nameColumnNumber))
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
                    }
                }
            }
        }

        #endregion

    }
}
