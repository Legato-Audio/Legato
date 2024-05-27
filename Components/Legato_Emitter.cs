using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Legato
{
    // [HelpURL("http://example.com/docs/MyComponent.html")] // Para que el bot�n ? lleve a nuestra documentaci�n (cambiar url)
    [RequireComponent(typeof(Transform))]
    [Serializable]
    public class Legato_Emitter : MonoBehaviour
    {
        static Legato_Emitter instance = null;
        [ReadOnlyWhenPlaying, SerializeField, Tooltip("Set whether the audio should play through an Audio Mixer first or directly through the Audio Listener.")]
        public AudioMixerGroup output;
        [ReadOnlyWhenPlaying, SerializeField] private bool debug;
        [ReadOnlyWhenPlaying, SerializeField, Range(0.0f, 1.0f)] private float volume = 1;
        
        [ReadOnlyWhenPlaying, SerializeField] private SoundScene[] soundScenes;

        public const float loadingBuffer = 0.4f; // Time (in seconds) that is allowed for audioclips to load between measures.
        private int currentScene = 0;

        private Fragment[] fragments;
        private Instrument[] instruments;
        private Tempo[] tempos;

        public static Legato_Emitter GetInstance()
        {
            return instance;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                foreach (SoundScene s in soundScenes) s.Awake(gameObject, debug);
                SetGlobalVolume(volume);
            }
            else
            {
                Debug.LogError("Only one Legato Emitter may be active at a given time.");
                Destroy(this);
            }

            fragments = Resources.LoadAll<Fragment>("Legato/Fragments");
            instruments = Resources.LoadAll<Instrument>("Legato/Instruments");
            tempos = Resources.LoadAll<Tempo>("Legato/Tempos");
        }

        void Start()
        {
            soundScenes[currentScene].Start();
        }

        private void FixedUpdate()
        {
            soundScenes[currentScene].Update(Time.fixedDeltaTime);
        }

        #region PUBLIC METHODS

        /// <summary>
        /// Set the volume of a specific channel of the current scene.
        /// </summary>
        /// <param name="c">The channel to set the volume to.</param>
        /// <param name="v">The volume of the channel (0.0 to 1.0).</param>
        public void SetVolume(int c, float v)
        {
            if (debug) Debug.Log("Volume of channel " + c + " set to " + v);
            v = Math.Clamp(v, 0, 1);
            soundScenes[currentScene].SetVolume(c, v);
        }

        /// <summary>
        /// Set the volume of the emitter.
        /// </summary>
        /// <param name="v">The volume of the emitter (0.0 to 1.0).</param>
        public void SetGlobalVolume(float v)
        {
            if (debug) Debug.Log("Emitter volume set to " + v);
            volume = Math.Clamp(v, 0, 1);
            foreach(SoundScene s in soundScenes) s.SetGlobalVolume(v);
        }

        /// <summary>
        /// Set current scene to s.
        /// </summary>
        /// <param name="s">The scene to set as current.</param>
        public void SetScene(int s)
        {
            if (s < soundScenes.Length)
            {
                if (debug) Debug.Log("Current scene set to " + s);
                soundScenes[currentScene].StopAll();
                currentScene = s;
                soundScenes[currentScene].Start();
            }
            else Debug.LogError("Can't set scene " + s + " because it doesn't exist. " + s + " is out of range.");
        }

        /// <summary>
        /// Change the spatial blend attribute of the current scene.
        /// </summary>
        /// <param name="spatialBlend">The value of spatial blend, where 0.0 is fully 2D and 1.0 is fully 3D.</param>
        public void SetSpatialBlend(float spatialBlend)
        {
            if (debug) Debug.Log("Current scene's spatial blend set to " + spatialBlend);
            soundScenes[currentScene].SetSpatialBlend(spatialBlend);
        }

        /// <summary>
        /// Change the spatial blend attribute of the scene s.
        /// </summary>
        /// <param name="s">The scene to change the spatial blend to.</param>
        /// <param name="spatialBlend">The value of spatial blend, where 0.0 is fully 2D and 1.0 is fully 3D.</param>
        public void SetSpatialBlend(int s, float spatialBlend)
        {
            if (s < soundScenes.Length)
            {
                if (debug) Debug.Log("Scene " + s + "'s spatial blend set to " + spatialBlend);
                soundScenes[s].SetSpatialBlend(spatialBlend);
            }
            else Debug.LogError("Can't set spatial blend of scene " + s + " because it doesn't exist. " + s + " is out of range.");
        }

        /// <summary>
        /// Set the rolloff mode of current scene's 3D audiosource.
        /// </summary>
        /// <param name="rolloffMode">The new rolloff mode.</param>
        public void Set3DRolloffMode(AudioRolloffMode rolloffMode)
        {
            if (debug) Debug.Log("Current scene's 3D rolloff mode set to " + rolloffMode.ToString());
            soundScenes[currentScene].Set3DRolloffMode(rolloffMode);
        }

        /// <summary>
        /// Set the min distance of current scene's 3D audiosource.
        /// </summary>
        /// <param name="minDistance">The new value of minDistance.</param>
        public void Set3DMinDistance(float minDistance)
        {
            if (debug) Debug.Log("Current scene's 3D min distance set to " + minDistance);
            soundScenes[currentScene].Set3DMinDistance(minDistance);
        }

        /// <summary>
        /// Set the max distance of current scene's 3D audiosource.
        /// </summary>
        /// <param name="maxDistance">The new value of maxDistance.</param>
        public void Set3DMaxDistance(float maxDistance)
        {
            if (debug) Debug.Log("Current scene's 3D max distance set to " + maxDistance);
            soundScenes[currentScene].Set3DMaxDistance(maxDistance);
        }

        /// <summary>
        /// Queue a motif to be played in a specific channel. At the start of the next possible measure, the motif with the highest current priority is chosen.
        /// </summary>
        /// <param name="c">The channel to queue the motif in.</param>
        /// <param name="motif">The name of the motif to add to the queue.</param>
        /// <param name="priority">The initial priority of the motif.</param>
        /// <param name="priorityOverTime">The rate at which the motif's priority increases or decreases over time.</param>
        public void PlayMotif(int c, string motif, bool interrupt = false, float priority = 1f, float priorityOverTime = 0f)
        {
            Fragment m = GetMotif(motif);
            if(m != null) soundScenes[currentScene].PlayMotif(c, m, interrupt, priority, priorityOverTime);

        }

        /// <summary>
        /// Queue a motif to be played in a specific channel. At the start of the next possible measure, the motif with the highest current priority is chosen.
        /// </summary>
        /// <param name="c">The channel to queue the motif in.</param>
        /// <param name="motif">The motif to add to the queue.</param>
        /// <param name="priority">The initial priority of the motif.</param>
        /// <param name="priorityOverTime">The rate at which the motif's priority increases or decreases over time.</param>
        public void PlayMotif(int c, Fragment motif, bool interrupt = false, float priority = 1f, float priorityOverTime = 0f)
        {
            if (motif != null) soundScenes[currentScene].PlayMotif(c, motif, interrupt, priority, priorityOverTime);
        }

        /// <summary>
        /// Remove all instances of a given motif of a specific channel motif queue.
        /// </summary>
        /// <param name="c">The channel to look for the motif.</param>
        /// <param name="motif">The name of the motif to cancel.</param>
        public void CancelMotif(int c, string motif)
        {
            Fragment m = GetMotif(motif);
            if(m != null) soundScenes[currentScene].CancelMotif(c, m);
        }

        /// <summary>
        /// Remove all instances of a given motif of a specific channel motif queue.
        /// </summary>
        /// <param name="c">The channel to look for the motif.</param>
        /// <param name="motif">The motif to cancel.</param>
        public void CancelMotif(int c, Fragment motif)
        {
            if(motif != null) soundScenes[currentScene].CancelMotif(c, motif);
        }

        /// <summary>
        /// Resume a specific channel of the current scene.
        /// </summary>
        /// <param name="c">The channel to resume.</param>
        public void Resume(int c)
        {
            soundScenes[currentScene].Resume(c);
        }

        /// <summary>
        /// Stop a specific channel of the current scene.
        /// </summary>
        /// <param name="c">The channel to stop.</param>
        public void Stop(int c)
        {
            if (debug) Debug.Log("Channel " + c + " stopped");
            soundScenes[currentScene].Stop(c);
        }

        /// <summary>
        /// Stop all the channels of the current scene.
        /// </summary>
        public void StopAll()
        {
            if (debug) Debug.Log("All channels stopped");
            soundScenes[currentScene].StopAll();
        }

        /// <summary>
        /// Resumes play on all the channels of the current scene.
        /// </summary>
        /// <param name="onlyPreviouslyPlaying">If true, only channels that were playing before StopAll was called will be resumed.</param>
        public void ResumeAll(bool onlyPreviouslyPlaying = false)
        {
            if (debug) Debug.Log(onlyPreviouslyPlaying ? "All previously active channels resumed" : "All channels resumed");
            soundScenes[currentScene].ResumeAll(onlyPreviouslyPlaying);
        }

        /// <summary>
        /// Set the instrument of a specific channel.
        /// </summary>
        /// <param name="c">The channel to set the instrument to.</param>
        /// <param name="instrument">The name of the instrument to set.</param>
        /// <param name="changeCurrent">Change the current fragment or not, where true changes the current fragment's instrument and false waits until the next fragment.</param>
        /// <param name="fadeDuration">The length of the crossfade in seconds when changing the instrument.</param>
        public void SetInstrument(int c, string instrument, bool changeCurrent = false, float fadeDuration = 0f)
        {
            Instrument i = GetInstrument(instrument);
            if(i != null) soundScenes[currentScene].SetInstrument(c, i, changeCurrent, fadeDuration);
        }

        /// <summary>
        /// Set the instrument of a specific channel.
        /// </summary>
        /// <param name="c">The channel to set the instrument to.</param>
        /// <param name="instrument">The instrument to set.</param>
        /// <param name="changeCurrent">Change the current fragment or not, where true changes the current fragment's instrument and false waits until the next fragment.</param>
        /// <param name="fadeDuration">The length of the crossfade in seconds when changing the instrument.</param>
        public void SetInstrument(int c, Instrument instrument, bool changeCurrent = false, float fadeDuration = 0f)
        {
            if (instrument != null) soundScenes[currentScene].SetInstrument(c, instrument, changeCurrent, fadeDuration);
        }

        /// <summary>
        /// Set the tempo of a specific channel of the current scene.
        /// </summary>
        /// <param name="c">The channel to set the tempo to.</param>
        /// <param name="tempo">The tempo in bpm to set.</param>
        public void SetTempo(int c, int tempo)
        {
            Tempo t = GetTempo(tempo);
            if (t != null) soundScenes[currentScene].SetTempo(c, t);
        }

        /// <summary>
        /// Set the tempo of a specific channel of the current scene.
        /// </summary>
        /// <param name="c">The channel to set the tempo to.</param>
        /// <param name="tempo">The tempo to set.</param>
        public void SetTempo(int c, Tempo tempo)
        {
            if (tempo != null) soundScenes[currentScene].SetTempo(c, tempo);
        }

        /// <summary>
        /// Set the tempo in all channels of the current scene.
        /// </summary>
        /// <param name="tempo">The tempo in bpm to set.</param>
        public void SetTempo(int tempo)
        {
            Tempo t = GetTempo(tempo);
            if (t != null) soundScenes[currentScene].SetTempo(t);
        }

        /// <summary>
        /// Set the tempo in all channels of the current scene.
        /// </summary>
        /// <param name="tempo">The tempo to set.</param>
        public void SetTempo(Tempo tempo)
        {
            if (tempo != null) soundScenes[currentScene].SetTempo(tempo);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Gets a motif given its name.
        /// </summary>
        /// <param name="motif">The name of the motif to get.</param>
        /// <returns>The motif that matches the given name.</returns>
        private Fragment GetMotif(string motif)
        {
            int i = 0;
            while(i < fragments.Length)
            {
                if (fragments[i].name.ToLower() == motif.ToLower()) return fragments[i];
                ++i;
            }
            Debug.LogError("Fragment \"" + motif + "\" not found. The project assets may need regenerating.");
            return null;
        }

        /// <summary>
        /// Gets an instrument given its name.
        /// </summary>
        /// <param name="instrument">The name of the instrument to get.</param>
        /// <returns>The instrument that matches the given name.</returns>
        private Instrument GetInstrument(string instrument)
        {
            int i = 0;
            while (i < instruments.Length)
            {
                if (instruments[i].name.ToLower() == instrument.ToLower()) return instruments[i];
                ++i;
            }
            Debug.LogError("Instrument \"" + instrument + "\" not found. The project assets may need regenerating.");
            return null;
        }

        /// <summary>
        /// Gets a tempo given its value.
        /// </summary>
        /// <param name="tempo">The value of tempo in bpm.</param>
        /// <returns>The tempo that matches the given value.</returns>
        private Tempo GetTempo(int tempo)
        {
            int i = 0; 
            while(i < tempos.Length)
            {
                if (tempos[i].name == tempo + "") return tempos[i];
                ++i;
            }
            Debug.LogError("Tempo \"" + tempo + "\" not found. The project assets may need regenerating.");
            return null;
        }

        #endregion
    }
}