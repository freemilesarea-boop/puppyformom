using System;
using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Systems
{
    /// <summary>
    /// Converts elapsed run time * scroll speed into the metres counter that drives
    /// progression, milestones, the cutscene and the ending.
    /// </summary>
    public class DistanceManager : MonoBehaviour
    {
        public float DistanceUnits { get; private set; }
        public int Meters { get; private set; }
        public float CurrentSpeed { get; private set; }

        /// <summary>Difficulty level — rises by 1 every <see cref="GameConfig.MetersPerLevel"/> metres.</summary>
        public int Level { get; private set; }

        /// <summary>Fired when the integer metre value changes.</summary>
        public event Action<int> OnMetersChanged;
        /// <summary>Fired once when a milestone metre threshold is first crossed.</summary>
        public event Action<int> OnMilestoneReached;
        /// <summary>Fired when the difficulty level increases.</summary>
        public event Action<int> OnLevelUp;

        private bool _running;
        private bool _milestone1, _milestone2, _milestone3, _cutscene, _ending;

        private void Awake() => ServiceLocator.Register(this);
        private void OnDestroy() => ServiceLocator.Unregister<DistanceManager>();

        public void ResetRun()
        {
            DistanceUnits = 0f;
            Meters = 0;
            Level = 0;
            CurrentSpeed = GameConfig.StartScrollSpeed;
            _running = false;
            _milestone1 = _milestone2 = _milestone3 = _cutscene = _ending = false;
            OnMetersChanged?.Invoke(0);
        }

        public void SetRunning(bool running) => _running = running;

        private void Update()
        {
            if (!_running) return;

            // Speed slowly ramps with distance for rising difficulty.
            CurrentSpeed = Mathf.Min(
                GameConfig.MaxScrollSpeed,
                GameConfig.StartScrollSpeed + Meters * GameConfig.SpeedGainPerMeter);

            DistanceUnits += CurrentSpeed * Time.deltaTime;
            int newMeters = Mathf.FloorToInt(DistanceUnits * GameConfig.MetersPerWorldUnit);
            if (newMeters != Meters)
            {
                Meters = newMeters;
                OnMetersChanged?.Invoke(Meters);
                ServiceLocator.Get<ScoreManager>()?.Recompute(Meters);

                int newLevel = Meters / GameConfig.MetersPerLevel;
                if (newLevel != Level) { Level = newLevel; OnLevelUp?.Invoke(Level); }

                CheckMilestones();
            }
        }

        private void CheckMilestones()
        {
            if (!_milestone1 && Meters >= GameConfig.Milestone1) { _milestone1 = true; OnMilestoneReached?.Invoke(GameConfig.Milestone1); }
            if (!_milestone2 && Meters >= GameConfig.Milestone2) { _milestone2 = true; OnMilestoneReached?.Invoke(GameConfig.Milestone2); }
            if (!_milestone3 && Meters >= GameConfig.Milestone3) { _milestone3 = true; OnMilestoneReached?.Invoke(GameConfig.Milestone3); }
            if (!_cutscene && Meters >= GameConfig.CutsceneDistance) { _cutscene = true; OnMilestoneReached?.Invoke(GameConfig.CutsceneDistance); }
            if (!_ending && Meters >= GameConfig.EndingDistance) { _ending = true; OnMilestoneReached?.Invoke(GameConfig.EndingDistance); }
        }
    }
}
