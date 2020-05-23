namespace GACourseAndExamSchedule.Algorithm
{
    public class Room
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public bool IsLab { get; set; }

        public Room() { }
        
        public Room(int id, string name, bool isLab, int capacity)
        {
            ID = id;
            Name = name;
            IsLab = isLab;
            Capacity = capacity;
        }

    }
}
