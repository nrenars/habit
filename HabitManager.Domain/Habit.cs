namespace HabitManager
{
    public class Habit
    {
        private int _ID;

        public String Name { get; set; }

        public int ID => _ID;

        private string _note;

        public string Note
        {
            get { return _note; }
            set { 
                if (value != null && value.Length > 5) 
                {
                    _note = value; 
                }
            }
        }

        public Habit() { }

        public Habit(int id, string name, string note)
        {
            _ID = id;
            Name = name;
            Note = note;
        }

        public override string ToString() { return $"ID: {_ID}, Name: {Name}, Note: {Note}"; }

      
    }
}
