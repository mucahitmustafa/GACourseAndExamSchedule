using System.Collections.Generic;

namespace GACourseAndExamSchedule.Algorithm
{
    public class StudentGroup
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public int Degree { get; set; }
        public int MaxHourInDay { get; set; }
        public int MaxDifficultyInDay { get; set; }
        public int StudentCount { get; set; }
        public List<CourseClass> CourseClasses { get; set; }

        public StudentGroup()
        {

        }

        public StudentGroup(int id, string branch, int degree, int count, int maxHourInDay, int maxDiffInDay)
        {
            ID = id;
            Name = $"{branch} {degree}";
            Branch = branch;
            Degree = degree;
            StudentCount = count;
            MaxHourInDay = maxHourInDay;
            MaxDifficultyInDay = maxDiffInDay;
        }

        public void AddCourseClass(CourseClass courseClass)
        {
            if (CourseClasses == null) CourseClasses = new List<CourseClass>();
            CourseClasses.Add(courseClass);
        }

        public static bool operator ==(StudentGroup lSG, StudentGroup rSG)
        {
            return (lSG.ID == rSG.ID);
        }

        public static bool operator !=(StudentGroup lSG, StudentGroup rSG)
        {
            return (lSG.ID != rSG.ID);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType()) return false;

            return this.ID == ((StudentGroup)obj).ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
