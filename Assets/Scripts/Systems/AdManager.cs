using System;
using System.Collections;
using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Systems
{
    /// <summary>
    /// MOCK ad manager. Simulates rewarded and interstitial ads with a short delay and
    /// console logs so the full monetization flow is wired and testable without a real SDK.
    /// Swap the bodies for AdMob / Unity Ads / LevelPlay before release.
    /// Rule: never Pay-To-Win. Ads only grant revives and cosmetic bonus bones.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        private int _gamesSinceInterstitial;
        private bool _adInProgress;

        private void Awake() => ServiceLocator.Register(this);
        private void OnDestroy() => ServiceLocator.Unregister<AdManager>();

        private bool AdsRemoved
        {
            get
            {
                var save = ServiceLocator.Get<SaveManager>();
                return save != null && save.RemoveAds;
            }
        }

        /// <summary>Rewarded ads are always available (even with Remove Ads) — they are opt-in.</summary>
        public void ShowRewardedAd(Action onReward, Action onSkipped = null)
        {
            if (_adInProgress) { onSkipped?.Invoke(); return; }
            StartCoroutine(SimulateAd(1.0f, () =>
            {
                Debug.Log("[AdManager] (MOCK) Rewarded ad completed -> granting reward.");
                onReward?.Invoke();
            }));
        }

        /// <summary>Call after each game over. Shows an interstitial once per N games unless ads removed.</summary>
        public void NotifyGameFinishedAndMaybeShowInterstitial(Action onClosed = null)
        {
            _gamesSinceInterstitial++;
            if (AdsRemoved || _gamesSinceInterstitial < GameConfig.InterstitialEveryNGames)
            {
                onClosed?.Invoke();
                return;
            }
            _gamesSinceInterstitial = 0;
            StartCoroutine(SimulateAd(0.8f, () =>
            {
                Debug.Log("[AdManager] (MOCK) Interstitial shown.");
                onClosed?.Invoke();
            }));
        }

        private IEnumerator SimulateAd(float seconds, Action done)
        {
            _adInProgress = true;
            Debug.Log("[AdManager] (MOCK) ...playing ad...");
            // Use realtime so it works even while the game is paused (Time.timeScale == 0).
            yield return new WaitForSecondsRealtime(seconds);
            _adInProgress = false;
            done?.Invoke();
        }
    }
}
