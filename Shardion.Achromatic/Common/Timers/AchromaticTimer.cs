using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using LiteDB;

namespace Shardion.Achromatic.Common.Timers
{
    public sealed class AchromaticTimer
    {
        private static readonly ConcurrentDictionary<Guid, AchromaticTimer> _memoryTimers = [];
        private static Timer? _timerProcessingTimer;
        private static Timer? _timerManagementTimer;
        private static LiteDatabase? _db;

        public static event Func<AchromaticTimer, Task>? TimerExpired;

        private static void ProcessTimers(object? maybeTimer, ElapsedEventArgs args)
        {
            if (maybeTimer is not Timer timer)
            {
                throw new InvalidOperationException("yo");
            }
            DateTimeOffset? closestTime = null;
            foreach (KeyValuePair<Guid, AchromaticTimer> memoryTimer in _memoryTimers.Where((timer) => !timer.Value.Completed))
            {
                TimeSpan durationSpan = memoryTimer.Value.CompletionTime.Subtract(DateTimeOffset.UtcNow);
                if (durationSpan.Ticks <= 0)
                {
                    Console.WriteLine($"utcnow: {DateTimeOffset.UtcNow}, ticks: {DateTimeOffset.UtcNow.Ticks}, completiontime: {memoryTimer.Value.CompletionTime}, ticks: {memoryTimer.Value.CompletionTime.Ticks}");
                    // Timer processing absolutely cannot block for accuracy reasons, so we
                    // invoke the expiry event on a different thread
                    Task.Run(() => TimerExpired?.Invoke(memoryTimer.Value));
                    Task.Run(() =>
                    {
                        memoryTimer.Value.Completed = true;
                        _db?.GetCollection<AchromaticTimer>("timers").Update(memoryTimer.Value);
                    });
                }
                if (closestTime == null || memoryTimer.Value.CompletionTime < closestTime)
                {
                    closestTime = memoryTimer.Value.CompletionTime;
                }
            }
            if (closestTime is not DateTimeOffset sleepTime)
            {
                timer.Stop();
                timer.Interval = 1;
            }
            else
            {
                TimeSpan sleepSpan = sleepTime.Subtract(DateTimeOffset.UtcNow);
                if (sleepSpan.Ticks < 0)
                {
                    // Spin until timer completes
                    timer.Interval = 50;
                    timer.Start();
                }
                else
                {
                    timer.Interval = sleepSpan.TotalMilliseconds;
                    timer.Start();
                }
            }
        }

        private static void ManageTimers(object? maybeTimer, ElapsedEventArgs args)
        {
            if (maybeTimer is not Timer timer)
            {
                throw new InvalidOperationException("yo");
            }
            if (_db is null)
            {
                // Spin until DB is available
                timer.Interval = 50;
                timer.Start();
            }
            else
            {
                // Load near timers from DB to memory
                ILiteCollection<AchromaticTimer> dbTimers = _db.GetCollection<AchromaticTimer>("timers");
                foreach (AchromaticTimer dbNearTimer in dbTimers.Find((timer) => timer.CompletionTime < DateTime.UtcNow.AddMinutes(30)))
                {
                    if (!_memoryTimers.ContainsKey(dbNearTimer.TimerId))
                    {
                        _memoryTimers[dbNearTimer.TimerId] = dbNearTimer;
                        if (_timerProcessingTimer is not null)
                        {
                            // Wake up processor to process newly-near timers immediately and adjust
                            // its interval based on them
                            _timerProcessingTimer.Start();
                        }
                    }
                }

                // Destroy completed DB timers
                _ = dbTimers.DeleteMany((timer) => timer.Completed);

                // Destroy completed memory timers
                foreach (KeyValuePair<Guid, AchromaticTimer> memoryTimer in _memoryTimers.Where((timer) => timer.Value.Completed))
                {
                    _ = _memoryTimers.TryRemove(memoryTimer);
                }

                // Sleep for 15 minutes (instead of 30, which is the near timer criterion)
                // Ensures we never miss near timers if they were added to the DB at least 30 minutes before expiry
                // due to skew between management thread wake-ups and determination of near status
                timer.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
                timer.Start();
            }
        }

        [BsonId]
        public Guid TimerId { get; set; } = Guid.NewGuid();
        public required DateTimeOffset CompletionTime { get; set; }
        // Flag to make sure timers never get triggered twice
        public bool Completed { get; set; }

        // Simple data for event subscribers to match on
        public required string Identifier { get; set; }
        // Complex data to determine what should actually be done
        public required BsonDocument Document { get; set; }

        public AchromaticTimer()
        {
        }

        public void Start()
        {
            if (_db is null)
            {
                throw new InvalidOperationException("No DB loaded to store timers into, did you forget to manually start the timer threads?");
            }
            if (Completed)
            {
                // Logic error that should be made apparent and fixed instead of silently causing issues
                throw new InvalidOperationException("Cannot start a timer that has already been completed");
            }

            if (CompletionTime < DateTimeOffset.UtcNow.AddMinutes(30))
            {
                _memoryTimers[TimerId] = this;
            }
            _db.GetCollection<AchromaticTimer>("timers").Insert(this);

            StartOrResumeTimerControllers(null);
        }

        public static void StartOrResumeTimerControllers(LiteDatabase? db)
        {
            _db ??= db;

            if (_timerProcessingTimer is null)
            {
                Timer timerProcessingTimer = new()
                {
                    AutoReset = false,
                    Enabled = false,
                    Interval = 1,
                };
                timerProcessingTimer.Elapsed += ProcessTimers;
                _timerProcessingTimer = timerProcessingTimer;
                _timerProcessingTimer.Start();
            }
            else
            {
                _timerProcessingTimer.Start();
            }
            if (_timerManagementTimer is null)
            {
                Timer timerManagementTimer = new()
                {
                    AutoReset = false,
                    Enabled = false,
                    Interval = 1,
                };
                timerManagementTimer.Elapsed += ManageTimers;
                _timerManagementTimer = timerManagementTimer;
                _timerManagementTimer.Start();
            }
            else
            {
                _timerManagementTimer.Start();
            }
        }
    }
}
