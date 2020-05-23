using System.Collections.Generic;
using System.Linq;

namespace GACourseAndExamSchedule.Algorithm
{
    public class Configuration
    {
        static Configuration _instance = new Configuration();
        public static Configuration GetInstance { get { return _instance; } }


        private Dictionary<int, Prelector> _prelectors = new Dictionary<int, Prelector>();

        private Dictionary<int, StudentGroup> _studentGroups = new Dictionary<int, StudentGroup>();

        private Dictionary<int, Course> _courses = new Dictionary<int, Course>();

        private Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        public Dictionary<int, Room> Rooms
        {
            get { return _rooms; }
        }

        private List<CourseClass> _courseClasses = new List<CourseClass>();

        private bool _isEmpty;

        public Configuration()
        {
            _isEmpty = true;
        }

        ~Configuration()
        {
            _prelectors.Clear();
            _studentGroups.Clear();
            _courses.Clear();
            _rooms.Clear();
            _courseClasses.Clear();
        }

        public void InitializeDate(List<Prelector> prelectors, List<StudentGroup> studentGroups, List<Course> courses, List<Room> rooms, List<CourseClass> courseClasses)
        {
            _prelectors.Clear();
            _studentGroups.Clear();
            _courses.Clear();
            _rooms.Clear();
            _courseClasses.Clear();
            prelectors.ForEach(_val =>
            {
                _prelectors.Add(_val.ID, _val);
            });

            studentGroups.ForEach(_val =>
            {
                _studentGroups.Add(_val.ID, _val);
            });

            courses.ForEach(_val =>
            {
                _courses.Add(_val.ID, _val);
            });

            rooms.ForEach(_val =>
            {
                _rooms.Add(_val.ID, _val);
            });

            _courseClasses = courseClasses;
            _isEmpty = false;
        }

        public Prelector GetPrelectorById(int id)
        {
            if (_prelectors.ContainsKey(id))
                return _prelectors[id];
            return null;
        }

        public int GetNumberOfPrelectors() { return (int)_prelectors.Count; }

        public StudentGroup GetStudentGroupById(int id)
        {
            if (_studentGroups.ContainsKey(id))
                return _studentGroups[id];
            return null;
        }

        public int GetNumberOfStudentGroups() { return (int)_studentGroups.Count; }

        public Course GetCourseById(int id)
        {
            if (_courses.ContainsKey(id))
                return _courses[id];
            return null;
        }

        public int GetNumberOfCourses() { return (int)_courses.Count; }

        public Room GetRoomById(int id)
        {
            if (_rooms.ContainsKey(id))
                return _rooms[id];
            return null;
        }

        public int GetNumberOfRooms() { return (int)_rooms.Count; }

        public List<CourseClass> GetCourseClasses() { return _courseClasses; }

        public List<CourseClass> GetCourseClassesWithCourse(Course course) { 
            return _courseClasses.Where(cc => cc.Course == course).ToList(); 
        }

        public int GetNumberOfCourseClasses() { return (int)_courseClasses.Count; }

        public bool IsEmpty() { return _isEmpty; }
    }

}
