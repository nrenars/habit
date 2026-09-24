using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager
{
    public class OneTimeHabit : Habit
    {
        public DateTime Deadline { get; set; }
        public bool Done { get; set; } = false;

        public override string ToString()
        {
            return base.ToString() + $", Deadline: {Deadline}, Done: {Done}";
        }
    }
}
