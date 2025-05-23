using System;
using UnityEngine;

namespace Treep.SFX {
    public class SoundFXManager : MonoBehaviour {
        public static SoundFXManager Instance;

        [SerializeField] private AudioSource soundFxObject;

        private void Awake() {
            if (SoundFXManager.Instance == null) {
                SoundFXManager.Instance = this;
            }
        }

        public void PlaySoundFXClip(AudioClip audioClip, Transform spawn, float volume) {
            AudioSource audioSource = Instantiate(soundFxObject, spawn.position, Quaternion.identity);
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            float clipLength = audioSource.clip.length;
            DontDestroyOnLoad(this.soundFxObject);
            Destroy(audioSource.gameObject, clipLength);
        }
        public AudioSource PlayLoopingSound(AudioClip clip, Transform parent, float volume)
        {
            AudioSource loopingSource = Instantiate(soundFxObject, parent.position, Quaternion.identity, parent);
            loopingSource.clip = clip;
            loopingSource.volume = volume;
            loopingSource.loop = true;
            loopingSource.Play();
            return loopingSource;
        }

        public void StopLoopingSound(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
            }
        }

    }
}
