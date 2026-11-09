using System.Text.Json.Serialization;

namespace HabitManager{

    public class Habit{

        private const int MaxNameLength = 100;

        private readonly List<HabitCompletion> _completions = new();

        public Guid Id { get; }
        public string UserId { get; }
        public string Name { get; private set; }
        public string Note { get; private set; }
        public HabitStatus Status { get; private set; }
        public Frequency HabitFrequency { get; private set; }
        public WeekDays ScheduledDays { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime EditedAt { get; private set; }

        public IReadOnlyList<HabitCompletion> Completions => _completions.AsReadOnly();

        public bool CanCompleteToday => !IsCompletedToday() && !IsArchived() && IsScheduledOn(DateTime.Today);
        public bool Completed => IsCompletedToday();

        public bool CanEdit => !IsArchived();
        public bool Archived => IsArchived();

        public Habit(string userId, string name, Frequency frequency, string note = "",
                     WeekDays scheduledDays = WeekDays.All){

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId nedrīkst būt tukšs.", nameof(userId));

            ValidateName(name);
            ValidateDays(scheduledDays);

            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
            HabitFrequency = frequency;
            ScheduledDays = scheduledDays;
            Note = note ?? "";
            Status = HabitStatus.Active;
            CreatedAt = DateTime.Now;
            EditedAt = CreatedAt;
        }

        [JsonConstructor]
        public Habit(Guid id, string userId, string name, string note, HabitStatus status,
                      Frequency habitFrequency, WeekDays scheduledDays, DateTime createdAt, DateTime editedAt,
                      IReadOnlyList<HabitCompletion> completions){
            Id = id;
            UserId = userId;
            Name = name;
            Note = note ?? "";
            Status = status;
            HabitFrequency = habitFrequency;
            // vecie JSON faili bez šī lauka -> visas dienas
            ScheduledDays = scheduledDays == WeekDays.None ? WeekDays.All : scheduledDays;
            CreatedAt = createdAt;
            EditedAt = editedAt;
            _completions = completions?.ToList() ?? new List<HabitCompletion>();
        }

        private static void ValidateName(string name){
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ieraduma nosaukums nedrīkst būt tukšs.");
            if (name.Length > MaxNameLength)
                throw new ArgumentException($"Nosaukums nedrīkst pārsniegt {MaxNameLength} simbolus.");
        }

        private static void ValidateDays(WeekDays days){
            if (days == WeekDays.None)
                throw new ArgumentException("Jāizvēlas vismaz viena nedēļas diena.");
            if ((days & ~WeekDays.All) != 0)
                throw new ArgumentException("Nederīgas nedēļas dienas.");
        }

        public void SetName(string name){
            ValidateName(name);
            Name = name;
            UpdateEditedAt();
        }

        public void SetNote(string note){
            Note = note ?? "";
            UpdateEditedAt();
        }

        public void SetFrequency(Frequency frequency){
            HabitFrequency = frequency;
            UpdateEditedAt();
        }

        public void SetScheduledDays(WeekDays days){
            ValidateDays(days);
            ScheduledDays = days;
            UpdateEditedAt();
        }

        // Visu validē pirms mainīšanas, lai kļūdas gadījumā nepaliek daļēji izmainīts ieradums
        public void Update(string name, string note, Frequency frequency, WeekDays scheduledDays){
            if (IsArchived())
                throw new InvalidOperationException("Arhivētu ieradumu nevar labot.");

            ValidateName(name);
            ValidateDays(scheduledDays);

            Name = name;
            Note = note ?? "";
            HabitFrequency = frequency;
            ScheduledDays = scheduledDays;
            UpdateEditedAt();
        }

        public void Archive(){
            Status = HabitStatus.Archived;
            UpdateEditedAt();
        }

        public void Restore(){
            Status = HabitStatus.Active;
            UpdateEditedAt();
        }

        public HabitCompletion MarkCompleted(DateTime date){
            if (Status == HabitStatus.Archived)
                throw new InvalidOperationException("Arhivētu ieradumu nevar atzīmēt kā izpildītu.");

            var day = date.Date;

            if (!IsScheduledOn(day))
                throw new InvalidOperationException("Šajā nedēļas dienā ieradums nav ieplānots.");
            if (_completions.Any(c => c.Date == day))
                throw new InvalidOperationException("Šim ieradumam šajā datumā jau ir izpildes ieraksts.");

            var completion = new HabitCompletion(Id, day);
            _completions.Add(completion);
            return completion;
        }

        public bool IsScheduledOn(DateTime date) => (ScheduledDays & date.DayOfWeek.ToWeekDays()) != 0;

        public bool IsCompletedOn(DateTime date) => _completions.Any(c => c.Date == date.Date);

        public bool IsCompletedToday() => IsCompletedOn(DateTime.Today);

        public bool IsArchived() => (Status == HabitStatus.Archived);

        private void UpdateEditedAt() => EditedAt = DateTime.Now;

        public override string ToString() => $"[{Id}] {Name} ({Status}, {HabitFrequency}, {ScheduledDays}) — Note: {Note}";

    }
}
