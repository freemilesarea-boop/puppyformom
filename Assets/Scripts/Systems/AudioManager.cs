using UnityEngine;
using PuppyForMom.Core;

namespace PuppyForMom.Systems
{
    public enum Sfx { Jump, Collect, Hit, Milestone, Button }

    /// <summary>
    /// Generates simple placeholder beeps at runtime so the MVP has audio feedback
    /// without shipping any audio files. Replace clips with real assets later.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _source;
        private AudioClip _jump, _collect, _hit, _milestone, _button;
        public bool Muted { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register(this);
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            _jump = MakeTone(660f, 0.10f, 0.35f);
            _collect = MakeTone(880f, 0.08f, 0.30f);
            _hit = MakeTone(140f, 0.22f, 0.45f);
            _milestone = MakeChirp(520f, 1040f, 0.25f, 0.35f);
            _button = MakeTone(440f, 0.06f, 0.25f);
        }

        private void OnDestroy() => ServiceLocator.Unregister<AudioManager>();

        public void ToggleMute() => Muted = !Muted;
        public void SetMuted(bool m) => Muted = m;

        public void Play(Sfx sfx)
        {
            if (Muted || _source == null) return;
            AudioClip clip = sfx switch
            {
                Sfx.Jump => _jump,
                Sfx.Collect => _collect,
                Sfx.Hit => _hit,
                Sfx.Milestone => _milestone,
                Sfx.Button => _button,
                _ => null
            };
            if (clip != null) _source.PlayOneShot(clip);
        }

        private static AudioClip MakeTone(float freq, float duration, float volume)
        {
            int sampleRate = 44100;
            int count = Mathf.Max(1, (int)(sampleRate * duration));
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Clamp01(1f - (float)i / count); // simple decay
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * volume;
            }
            var clip = AudioClip.Create("tone", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip MakeChirp(float f0, float f1, float duration, float volume)
        {
            int sampleRate = 44100;
            int count = Mathf.Max(1, (int)(sampleRate * duration));
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float p = (float)i / count;
                float freq = Mathf.Lerp(f0, f1, p);
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * p);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * volume;
            }
            var clip = AudioClip.Create("chirp", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
