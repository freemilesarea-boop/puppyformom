using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Systems
{
    /// <summary>
    /// Thin PlayerPrefs wrapper for persistent progression. Survives across scenes (DontDestroyOnLoad).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private const string KeyHighScore = "pfm_highscore";
        private const string KeyBestDistance = "pfm_bestdistance";
        private const string KeyTotalBones = "pfm_totalbones";
        private const string KeyRemoveAds = "pfm_removeads";
        private const string KeyEndingSeen = "pfm_endingseen";
        private const string KeyEndlessUnlocked = "pfm_endless";
        private const string KeyPhotoPieces = "pfm_photopieces";
        private const string KeySelectedSkin = "pfm_skin";

        public int HighScore => PlayerPrefs.GetInt(KeyHighScore, 0);
        public int BestDistance => PlayerPrefs.GetInt(KeyBestDistance, 0);
        public int TotalBones => PlayerPrefs.GetInt(KeyTotalBones, 0);
        public int PhotoPieces => PlayerPrefs.GetInt(KeyPhotoPieces, 0);
        public bool RemoveAds => PlayerPrefs.GetInt(KeyRemoveAds, 0) == 1;
        public bool EndingSeen => PlayerPrefs.GetInt(KeyEndingSeen, 0) == 1;
        public bool EndlessUnlocked => PlayerPrefs.GetInt(KeyEndlessUnlocked, 0) == 1;
        public string SelectedSkin => PlayerPrefs.GetString(KeySelectedSkin, "Loui");

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<SaveManager>();
        }

        /// <summary>Records the result of a finished run, keeping bests.</summary>
        public void RecordRun(int score, int distanceMeters, int bonesCollected, int photoPieces)
        {
            if (score > HighScore) PlayerPrefs.SetInt(KeyHighScore, score);
            if (distanceMeters > BestDistance) PlayerPrefs.SetInt(KeyBestDistance, distanceMeters);
            PlayerPrefs.SetInt(KeyTotalBones, TotalBones + bonesCollected);
            PlayerPrefs.SetInt(KeyPhotoPieces, Mathf.Min(PhotoPieces + photoPieces, 99));
            PlayerPrefs.Save();
        }

        public void SetRemoveAds(bool value) { PlayerPrefs.SetInt(KeyRemoveAds, value ? 1 : 0); PlayerPrefs.Save(); }
        public void SetEndingSeen() { PlayerPrefs.SetInt(KeyEndingSeen, 1); PlayerPrefs.SetInt(KeyEndlessUnlocked, 1); PlayerPrefs.Save(); }
        public void SetSelectedSkin(string skin) { PlayerPrefs.SetString(KeySelectedSkin, skin); PlayerPrefs.Save(); }

        public void ResetAll()
        {
            PlayerPrefs.DeleteKey(KeyHighScore);
            PlayerPrefs.DeleteKey(KeyBestDistance);
            PlayerPrefs.DeleteKey(KeyTotalBones);
            PlayerPrefs.DeleteKey(KeyPhotoPieces);
            PlayerPrefs.DeleteKey(KeyEndingSeen);
            PlayerPrefs.DeleteKey(KeyEndlessUnlocked);
            PlayerPrefs.Save();
        }
    }
}
