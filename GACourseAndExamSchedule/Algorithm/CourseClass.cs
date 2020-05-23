using System;
using System.Collections.Generic;

namespace GACourseAndExamSchedule.Algorithm
{
    public class CourseClass
    {

        public int ID { get; set; }
        public int StudentCount { get; set; }
        public bool RequiresLab { get; set; }
        public int Duration { get; set; }
        public Course Course { get; set; }
        public Prelector Prelector { get; set; }
        public List<StudentGroup> StudentGroups { get; set; }

        public int Difficulty { get; set; }


        public CourseClass()
        {

        }

        public CourseClass(Prelector prelector, Course course, List<StudentGroup> groups, bool requiresLab,
            int duration, int id, int difficulty, int roomCount)
        {
            Prelector = prelector;
            Course = course;
            StudentGroups = groups;
            RequiresLab = requiresLab;
            Duration = duration;
            ID = id;
            Difficulty = difficulty;

            StudentCount = 0;
            Prelector.AddCourseClass(this);

            foreach (StudentGroup group in groups)
            {
                group.AddCourseClass(this);
                StudentCount += group.StudentCount;
            }

            StudentCount = StudentCount / roomCount;
        }

        
        public bool GroupsOverlap(CourseClass courseClass)
        {
            foreach (StudentGroup group1 in StudentGroups)
            {
                foreach (StudentGroup group2 in courseClass.StudentGroups)
                {
                    if (group1 == group2 && Course != courseClass.Course) return true;
                }
            }
            return false;
        }

        public bool BranchsOverlaps(CourseClass courseClass)
        {
            foreach (StudentGroup group1 in StudentGroups)
            {
                foreach (StudentGroup group2 in courseClass.StudentGroups)
                {
                    if (group1.Branch == group2.Branch && Course != courseClass.Course) return true;
                }
            }
            return false;
        }

        public bool SequentialGroupsOverlap(CourseClass courseClass)
        {
            foreach (StudentGroup group1 in StudentGroups)
            {
                foreach (StudentGroup group2 in courseClass.StudentGroups)
                {
                    if (group1 == group2 || ((group1.Branch == group2.Branch) && (Math.Abs(group1.Degree - group2.Degree) == 1))) return true;
                }
            }
            return false;
        }

        public bool PrelectorOverlaps(CourseClass courseClass)
        {
            return Prelector == courseClass.Prelector;
        }

    }
}
