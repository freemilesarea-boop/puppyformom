using System;
using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Systems
{
    /// <summary>
    /// Tracks the current run's bones, photo pieces and computed score.
    /// Score = bones * BoneScoreValue + distanceMeters / MeterScoreDivisor.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public int Bones { get; private set; }
        public int PhotoPieces { get; private set; }
        public int SmellCount { get; private set; }
        public int Score { get; private set; }

        public event Action<int> OnBonesChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnPhotoPieceCollected;

        private void Awake() => ServiceLocator.Register(this);
        private void OnDestroy() => ServiceLocator.Unregister<ScoreManager>();

        public void ResetRun()
        {
            Bones = 0;
            PhotoPieces = 0;
            SmellCount = 0;
            Score = 0;
            OnBonesChanged?.Invoke(Bones);
            OnScoreChanged?.Invoke(Score);
        }

        public void AddBones(int amount)
        {
            Bones += amount;
            OnBonesChanged?.Invoke(Bones);
            Recompute(GetDistanceMeters());
        }

        public void AddSmell() => SmellCount++;

        public void AddPhotoPiece()
        {
            PhotoPieces++;
            OnPhotoPieceCollected?.Invoke(PhotoPieces);
            Recompute(GetDistanceMeters());
        }

        /// <summary>Called by DistanceManager as distance advances so score stays live.</summary>
        public void Recompute(int distanceMeters)
        {
            int newScore = Bones * GameConfig.BoneScoreValue + distanceMeters / GameConfig.MeterScoreDivisor;
            if (newScore != Score)
            {
                Score = newScore;
                OnScoreChanged?.Invoke(Score);
            }
        }

        private int GetDistanceMeters()
        {
            var dm = ServiceLocator.Get<DistanceManager>();
            return dm != null ? dm.Meters : 0;
        }
    }
}
