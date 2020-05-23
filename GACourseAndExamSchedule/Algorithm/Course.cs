using System;
using System.Collections.Generic;
using System.Linq;
namespace GACourseAndExamSchedule.Algorithm
{
    public class Course
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public Course()
        {

        }

        public Course(int id, string name)
        {
            ID = id;
            Name = name;
        }

    }
}
