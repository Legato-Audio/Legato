using System;
using UnityEngine;

namespace Legato
{
    [System.Serializable]
    public struct SoundSettings3D
    {
        [SerializeField]
        private AudioRolloffMode rollofMode;

        [SerializeField]
        private float minDistance;

        [SerializeField]
        private float maxDistance;

        public AudioRolloffMode rMode
        {
            get { return rollofMode; }
            set { rollofMode = value; }
        }

        public float min
        {
            get { return minDistance; }
            set { minDistance = value; }
        }

        public float max
        {
            get { return maxDistance; }
            set { maxDistance = value; }
        }
    }

    [Serializable]
    public class SoundScene
    {
        [SerializeField, Range(0, 1), Tooltip("2D <-> 3D")] private float spatialBlend;
        [SerializeField] private SoundSettings3D soundSettings3D;
        [SerializeField] private Channel[] channels;
        private bool[] activeChannels;  // Allows for only channels that were playing before StopAll to be resumed by ResumeAll.
        private bool debug;

        public void Awake(GameObject go, bool d)
        {
            debug = d;
            foreach (Channel c in channels)
            {
                c.Awake(go, d);

                c.SetSpatialBlend(spatialBlend);
                c.Set3DRolloffMode(soundSettings3D.rMode);
                c.Set3DMinDistance(soundSettings3D.min);
                c.Set3DMaxDistance(soundSettings3D.max);
            }
            activeChannels = new bool[channels.Length];
        }
        
        public void Start()
        {
            foreach (Channel c in channels) c.Start();
            for (int i = 0; i < activeChannels.Length; ++i) activeChannels[i] = true;
        }

        public void Update(float deltaTime)
        {
            foreach(Channel c in channels) c.Update(deltaTime);
        }

        public void SetVolume(int c, float v)
        {
            if(c < channels.Length) channels[c].SetVolume(v);
            else UnityEngine.Debug.LogError("Can't set volume of channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void SetGlobalVolume(float v)
        {
            foreach (Channel c in channels) c.SetGlobalVolume(v);
        }

        public void SetSpatialBlend(float spatialBlend)
        {
            foreach (Channel c in channels) c.SetSpatialBlend(spatialBlend);
        }

        public void Set3DRolloffMode(AudioRolloffMode rolloffMode)
        {
            soundSettings3D.rMode = rolloffMode;
            foreach (Channel c in channels) c.Set3DRolloffMode(rolloffMode);
        }

        public void Set3DMinDistance(float minDistance)
        {
            soundSettings3D.min = minDistance;
            foreach (Channel c in channels) c.Set3DMinDistance(minDistance);
        }

        public void Set3DMaxDistance(float maxDistance)
        {
            soundSettings3D.max = maxDistance;
            foreach (Channel c in channels) c.Set3DMaxDistance(maxDistance);
        }

        public void Resume(int c)
        {
            if (c < channels.Length)
            {
                channels[c].Resume();
                activeChannels[c] = true;
            }
            else UnityEngine.Debug.LogError("Can't resume channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void Stop(int c)
        {
            if (c < channels.Length)
            {
                channels[c].Stop();
                activeChannels[c] = false;
            }
            else UnityEngine.Debug.LogError("Can't stop channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void PlayMotif(int c, Fragment motif, bool interrupt, float priority, float priorityOverTime)
        {
            if (c < channels.Length)
            {
                if (debug) UnityEngine.Debug.Log("Playing motif " + motif.name + " in channel " + c);
                channels[c].PlayMotif(motif, interrupt, priority, priorityOverTime);
            }
            else Debug.LogError("Can't play motif in channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void CancelMotif(int c, Fragment motif)
        {
            if (c < channels.Length) channels[c].CancelMotif(motif);
            else Debug.LogError("Can't cancel motif in channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void StopAll()
        {
            foreach (Channel c in channels) c.Stop();
        }

        public void ResumeAll(bool onlyPreviouslyPlaying)
        {
            if (onlyPreviouslyPlaying)
                for (int i = 0; i < channels.Length; ++i)
                {
                    if (activeChannels[i]) channels[i].Resume();
                }
            else
            {
                for (int i = 0; i < channels.Length; ++i)
                {
                    channels[i].Resume();
                    activeChannels[i] = true;
                }
            }
        }

        public void SetInstrument(int c, Instrument i, bool changeCurrent, float fadeDuration)
        {
            if (c < channels.Length) channels[c].SetInstrument(i ,changeCurrent, fadeDuration);
            else Debug.LogError("Can't set instrument of channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void SetTempo(int c, Tempo t)
        {
            if (c < channels.Length) channels[c].SetTempo(t);
            else Debug.LogError("Can't set tempo of channel " + c + " because it doesn't exist. " + c + " is out of range.");
        }

        public void SetTempo(Tempo t)
        {
            foreach(Channel c in channels)c.SetTempo(t);
        }
    }
}
