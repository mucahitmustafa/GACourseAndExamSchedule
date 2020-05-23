using System.Collections.Generic;

namespace GACourseAndExamSchedule.Algorithm
{
    public class Prelector
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<CourseClass> CourseClasses { get; set; }
        public bool[] ScheduleDays { get; set; }

        public Prelector()
        {

        }

        public Prelector(int id, string name, bool[] scheduleDays)
        {
            ID = id;
            Name = name;
            ScheduleDays = scheduleDays;
        }

        public void AddCourseClass(CourseClass courseClass)
        {
            if (CourseClasses == null) CourseClasses = new List<CourseClass>();
            CourseClasses.Add(courseClass);
        }

        public static bool operator ==(Prelector p1, Prelector p2)
        {
            return p1.ID == p2.ID;
        }

        public static bool operator !=(Prelector p1, Prelector p2)
        {
            return p1.ID != p2.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType()) return false;

            return this.ID == ((Prelector)obj).ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
