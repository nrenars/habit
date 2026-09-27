using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HabitManager
{
    public class Habit
    {
        private const int MaxNameLength = 100;

        private readonly List<HabitCompletion> _completions = new();

        public Guid Id { get; }
        public string UserId { get; }
        public string Name { get; private set; }
        public string Note { get; private set; }
        public HabitStatus Status { get; private set; }
        public Frequency HabitFrequency { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime EditedAt { get; private set; }

        public IReadOnlyList<HabitCompletion> Completions => _completions.AsReadOnly();

        public Habit(string userId, string name, Frequency frequency, string note = "")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId nedrīkst būt tukšs.", nameof(userId));

            Id = Guid.NewGuid();
            UserId = userId;
            HabitFrequency = frequency;
            Note = note ?? "";
            Status = HabitStatus.Active;
            CreatedAt = DateTime.Now;
            EditedAt = CreatedAt;

            SetName(name);
        }

        [JsonConstructor]
        public Habit(Guid id, string userId, string name, string note, HabitStatus status,
                      Frequency habitFrequency, DateTime createdAt, DateTime editedAt,
                      List<HabitCompletion> completions)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Note = note ?? "";
            Status = status;
            HabitFrequency = habitFrequency;
            CreatedAt = createdAt;
            EditedAt = editedAt;
            _completions = completions ?? new List<HabitCompletion>();
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ieraduma nosaukums nedrīkst būt tukšs.");
            if (name.Length > MaxNameLength)
                throw new ArgumentException($"Nosaukums nedrīkst pārsniegt {MaxNameLength} simbolus.");

            Name = name;
            Touch();
        }

        public void SetNote(string note)
        {
            Note = note ?? "";
            Touch();
        }

        public void SetFrequency(Frequency frequency)
        {
            HabitFrequency = frequency;
            Touch();
        }

        public void Archive()
        {
            Status = HabitStatus.Archived;
            Touch();
        }

        public void Restore()
        {
            Status = HabitStatus.Active;
            Touch();
        }

        public HabitCompletion MarkCompleted(DateTime date)
        {
            if (Status == HabitStatus.Archived)
                throw new InvalidOperationException("Arhivētu ieradumu nevar atzīmēt kā izpildītu.");

            var day = date.Date;
            if (_completions.Any(c => c.Date == day))
                throw new InvalidOperationException("Šim ieradumam šajā datumā jau ir izpildes ieraksts.");

            var completion = new HabitCompletion(Id, day);
            _completions.Add(completion);
            return completion;
        }

        public bool IsCompletedOn(DateTime date) => _completions.Any(c => c.Date == date.Date);

        public bool IsCompletedToday() => IsCompletedOn(DateTime.Today);

        private void Touch() => EditedAt = DateTime.Now;

        public override string ToString() =>
            $"[{Id}] {Name} ({Status}, {HabitFrequency}) — Note: {Note}";
    }
}
