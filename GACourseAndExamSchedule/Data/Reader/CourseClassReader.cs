﻿using ExcelDataReader;
using GACourseAndExamSchedule.Algorithm;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace GACourseAndExamSchedule.Data.Reader
{
    public class CourseClassReader
    {
        #region Constants and Fields

        private string PATH_EXCEL_DATA;
        private readonly string FILENAME_COURSE_CLASS = ConfigurationManager.AppSettings.Get("data.input.filename.courseClasses");

        private const string COLUMN_NAME_COURSE = "Ders";
        private const string COLUMN_NAME_PROFESSOR = "Öğretim Görevlisi";
        private const string COLUMN_NAME_COURSE_DURATION = "Ders Süresi";
        private const string COLUMN_NAME_EXAM_DURATION = "Sınav Süresi";
        private const string COLUMN_NAME_GROUPS = "Öğrenci Grupları";
        private const string COLUMN_NAME_REQ_LAB = "Laboratuvar Gereksinimi (E - H)";
        private const string COLUMN_NAME_DIFFICULTY = "Zorluk";
        private const string COLUMN_NAME_ROOM_COUNT = "Sınıf Sayısı";

        private const string TEXT_BOOLEAN = "E";

        private string _filePath;
        private List<CourseClass> _courseClasses = new List<CourseClass>();
        private List<CourseClass> _courseClassesWithoutRoomSplit = new List<CourseClass>();

        private int _courseColumnNumber = 0;
        private int _prelectorColumnNumber = 0;
        private int _courseDurationColumnNumber = 0;
        private int _examDurationColumnNumber = 0;
        private int _groupsColumnNumber = 0;
        private int _reqLabColumnNumber = 0;
        private int _difficultyColumnNumber = 0;
        private int _roomCountColumnNumber = 0;

        private bool _isExamProblem;

        StudentGroupReader _studentGroupReader = new StudentGroupReader();
        PrelectorReader _prelectorReader = new PrelectorReader();
        CourseReader _courseReader = new CourseReader();

        #endregion

        #region Constructors

        public CourseClassReader(bool isExamProblem)
        {
            _isExamProblem = isExamProblem;
            PATH_EXCEL_DATA = ConfigurationManager.AppSettings.Get("data.input.location");

            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = PATH_EXCEL_DATA + (PATH_EXCEL_DATA.EndsWith("\\") ? "" : "\\") + FILENAME_COURSE_CLASS;
            }
        }

        #endregion

        #region Public Methods

        public void ResetData()
        {
            if (_courseClasses != null) _courseClasses.Clear();
        }

        public List<CourseClass> GetCourseClasses()
        {
            CollectCourseClasss();

            return _courseClasses;
        }

        public List<CourseClass> GetCourseClassesWithoutRoomSplit()
        {
            CollectCourseClasss();

            List<string> _calculatedCcs = new List<string>();
            foreach (CourseClass cc in _courseClasses)
            {
                string _ccKey = $"{cc.Course.ID}-{cc.Prelector.ID}-{cc.StudentGroups.Count}-{cc.StudentGroups[0].ID}";
                if (!_calculatedCcs.Contains(_ccKey))
                {
                    _calculatedCcs.Add(_ccKey);
                    _courseClassesWithoutRoomSplit.Add(cc);
                }
            }

            return _courseClassesWithoutRoomSplit;
        }

        public List<CourseClass> GetPrelectorCourses(int professorId)
        {
            CollectCourseClasss();

            return _courseClasses.FindAll(_c => _c.Prelector.ID.Equals(professorId));
        }

        public List<CourseClass> GetGroupsCourses(int groupId)
        {
            CollectCourseClasss();

            return _courseClasses.FindAll(_c => _c.StudentGroups.FindAll(g => g.ID.Equals(groupId)).Count > 0);
        }


        public CourseClass GetCourseClassById(int id)
        {
            CollectCourseClasss();

            return _courseClasses.Find(_r => _r.ID.Equals(id));
        }

        #endregion

        #region Private Methods

        private void CollectCourseClasss()
        {
            if (_courseClasses != null && _courseClasses.Count != 0) return;

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

                    int _ccId = 0;
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        List<int> _groupIds = row.ItemArray[_groupsColumnNumber].ToString().Split(',').Select(int.Parse).ToList();
                        List<StudentGroup> _studentGroups = new List<StudentGroup>();
                        _groupIds.ForEach(_id => {
                            _studentGroups.Add(_studentGroupReader.GetStudentGroupById(_id));
                        });

                        int _roomCount = int.Parse(row.ItemArray[_roomCountColumnNumber].ToString());
                        if (!_isExamProblem || _roomCount == 0) _roomCount = 1;
                        for (int i = 0; i < _roomCount; i++)
                        {
                            int _dur = _isExamProblem ? int.Parse(row.ItemArray[_examDurationColumnNumber].ToString()) : int.Parse(row.ItemArray[_courseDurationColumnNumber].ToString());

                            _courseClasses.Add(new CourseClass
                                (
                                    id: _ccId,
                                    duration: _dur,
                                    difficulty: int.Parse(row.ItemArray[_difficultyColumnNumber].ToString()),
                                    requiresLab: row.ItemArray[_reqLabColumnNumber].ToString().ToUpper().Equals(TEXT_BOOLEAN),
                                    groups: _studentGroups,
                                    course: _courseReader.GetCourseById(int.Parse(row.ItemArray[_courseColumnNumber].ToString())),
                                    prelector: _prelectorReader.GetPrelectorById(int.Parse(row.ItemArray[_prelectorColumnNumber].ToString())),
                                    roomCount: _roomCount
                                ));
                            _ccId++;
                        }
                    }
                }
            }
        }

        private void FindColumnIndexes(DataColumnCollection columnCollection)
        {
            if ((_courseColumnNumber == _prelectorColumnNumber) ||
                (_courseColumnNumber == _courseDurationColumnNumber) ||
                (_courseColumnNumber == _examDurationColumnNumber) ||
                (_courseColumnNumber == _groupsColumnNumber) ||
                (_courseColumnNumber == _reqLabColumnNumber) ||
                (_courseColumnNumber == _difficultyColumnNumber) ||
                (_courseColumnNumber == _roomCountColumnNumber) ||

                (_prelectorColumnNumber == _courseDurationColumnNumber) ||
                (_prelectorColumnNumber == _examDurationColumnNumber) ||
                (_prelectorColumnNumber == _groupsColumnNumber) ||
                (_prelectorColumnNumber == _reqLabColumnNumber) ||
                (_prelectorColumnNumber == _difficultyColumnNumber) ||
                (_prelectorColumnNumber == _roomCountColumnNumber) ||

                (_courseDurationColumnNumber == _examDurationColumnNumber) ||
                (_courseDurationColumnNumber == _groupsColumnNumber) ||
                (_courseDurationColumnNumber == _reqLabColumnNumber) ||
                (_courseDurationColumnNumber == _difficultyColumnNumber) ||
                (_courseDurationColumnNumber == _roomCountColumnNumber) ||

                (_examDurationColumnNumber == _groupsColumnNumber) ||
                (_examDurationColumnNumber == _reqLabColumnNumber) ||
                (_examDurationColumnNumber == _difficultyColumnNumber) ||
                (_examDurationColumnNumber == _roomCountColumnNumber) ||

                (_groupsColumnNumber == _reqLabColumnNumber) ||
                (_groupsColumnNumber == _difficultyColumnNumber) ||
                (_groupsColumnNumber == _roomCountColumnNumber) ||

                (_reqLabColumnNumber == _difficultyColumnNumber) ||
                (_reqLabColumnNumber == _roomCountColumnNumber) ||

                (_difficultyColumnNumber == _roomCountColumnNumber))
            {
                for (int i = 0; i < columnCollection.Count; i++)
                {
                    DataColumn column = columnCollection[i];
                    switch (column.ColumnName)
                    {
                        case COLUMN_NAME_COURSE:
                            _courseColumnNumber = i;
                            break;
                        case COLUMN_NAME_PROFESSOR:
                            _prelectorColumnNumber = i;
                            break;
                        case COLUMN_NAME_COURSE_DURATION:
                            _courseDurationColumnNumber = i;
                            break;
                        case COLUMN_NAME_EXAM_DURATION:
                            _examDurationColumnNumber = i;
                            break;
                        case COLUMN_NAME_GROUPS:
                            _groupsColumnNumber = i;
                            break;
                        case COLUMN_NAME_REQ_LAB:
                            _reqLabColumnNumber = i;
                            break;
                        case COLUMN_NAME_DIFFICULTY:
                            _difficultyColumnNumber = i;
                            break;
                        case COLUMN_NAME_ROOM_COUNT:
                            _roomCountColumnNumber = i;
                            break;
                    }
                }
            }
        }

        #endregion

    }
}
