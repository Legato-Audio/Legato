using UnityEngine;
using UnityEngine.Audio;

namespace Legato
{
    public class Audio
    {
        private Channel channel;
        private AudioSource[,] audioSources;

        // None = no crossfade
        // Loading = crossfade requested, but incoming track is still loading
        // InProgress = crossfading
        // Complete = a crossfade has been completed in this measure; auxiliary source is now playing while crossfading back to the main source
        private enum CrossfadeState { None, Loading, InProgress };
        private CrossfadeState crossfadeState;

        private double crossfadeStart;          // Moment that crossfade actually takes effect (delayed relative to initial request)
        private float crossfadeDuration;        // Duration of the main crossfade
        private float volume;
        private bool debug;

        public Audio(GameObject go, Channel ch, bool d)
        {
            channel = ch;
            debug = d;

            volume = 1f;
            crossfadeState = CrossfadeState.None;
            audioSources = new AudioSource[2, 2];
            AudioMixerGroup output = Legato_Emitter.GetInstance().output;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    audioSources[i, j] = go.AddComponent<AudioSource>();
                    audioSources[i, j].playOnAwake = false;
                    audioSources[i, j].outputAudioMixerGroup = output;
                    if (i == 1) audioSources[i, j].volume = 0;
                }
            }
        }

        public void Update(float deltaTime)
        {
            // Crossfade operations
            if (crossfadeState == CrossfadeState.Loading && AudioSettings.dspTime >= crossfadeStart)
            {
                if(debug) Debug.Log("Crossfade start");
                crossfadeState = CrossfadeState.InProgress;
            }

            if (crossfadeState == CrossfadeState.InProgress)
            {
                if (AudioSettings.dspTime < crossfadeStart + crossfadeDuration) // Crossfade
                {
                    // Probar qué pasa si cambiamos el volumen aquí
                    float progress = Mathf.Clamp((float)(AudioSettings.dspTime - crossfadeStart) / crossfadeDuration, 0f, 1f);
                    for (int i = 0; i < 2; i++)
                    {
                        audioSources[0, i].volume = LinearToCurve((1f - progress) * volume);
                        audioSources[1, i].volume = LinearToCurve(progress * volume);
                    }
                }
                else // Crossfade finish
                {
                    if (debug) Debug.Log("Crossfade end");
                    AudioSource aux = audioSources[0, 0];
                    audioSources[0, 0] = audioSources[1, 0];
                    audioSources[1, 0] = aux;

                    for (int i = 0; i < 2; i++)
                    {
                        audioSources[0, i].volume = volume;
                        audioSources[1, i].volume = 0;
                    }
                    crossfadeState = CrossfadeState.None;

                    channel.CrossfadeEnd();
                }
            }
        }

        // Receives an AudioClip and a moment in time. At the time received, it will play the AudioClip in the available AudioSource.
        public void Play(AudioClip clip, AudioClip incomingClip, double nextMeasure, float time = 0)
        {
            audioSources[0, 1].clip = clip;
            audioSources[0, 1].time = time;
            audioSources[0, 1].PlayScheduled(nextMeasure);

            audioSources[1, 1].clip = incomingClip;
            audioSources[1, 1].time = time;
            audioSources[1, 1].PlayScheduled(nextMeasure);
        }

        public void Stop()
        {
            for (int i = 0; i < 2; i++)
            {
                audioSources[0, i].Stop();
                audioSources[1, i].Stop();

                audioSources[0, i].volume = volume;
                audioSources[1, i].volume = 0;
            }
        }

        public void SetVolume(float v)
        {
            volume = v;
            if (crossfadeState != CrossfadeState.InProgress) for (int i = 0; i < 2; i++) audioSources[0, i].volume = v;
        }

        public void SetSpatialBlend(float spatialBlend)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    audioSources[i, j].spatialBlend = spatialBlend;
                }
            }
        }

        public void Set3DRolloffMode(AudioRolloffMode rolloffMode)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    audioSources[i, j].rolloffMode = rolloffMode;
                }
            }
        }

        public void Set3DMinDistance(float minDistance)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    audioSources[i, j].minDistance = minDistance;
                }
            }
        }

        public void Set3DMaxDistance(float maxDistance)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    audioSources[i, j].maxDistance = maxDistance;
                }
            }
        }

        public void ClipChange()
        {
            if (audioSources[0, 1].time > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    AudioSource aux = audioSources[i, 0];
                    audioSources[i, 0] = audioSources[i, 1];
                    audioSources[i, 1] = aux;

                    audioSources[i, 1].Stop();
                }
                if (debug) Debug.Log("Now playing " + audioSources[0, 0].clip.name);
            }
        }

        public float GetTimeNewTempo(AudioClip clip, double nextMeasure, int tempo1, int tempo2)
        {
            return audioSources[0, 0].time > 0 ? (audioSources[0, 0].time + (float)(nextMeasure - AudioSettings.dspTime)) * tempo1 / tempo2 : 0;
        }

        public bool IsCrossfading()
        {
            return crossfadeState != CrossfadeState.None;
        }

        public void StartCrossfadeNow(AudioClip clip, float duration)
        {
            if (!IsCrossfading())
            {
                crossfadeState = CrossfadeState.Loading;
                crossfadeStart = AudioSettings.dspTime + Legato_Emitter.loadingBuffer;
                crossfadeDuration = duration;

                audioSources[1, 0].clip = clip;
                audioSources[1, 0].time = audioSources[0, 0].time + Legato_Emitter.loadingBuffer;
                audioSources[1, 0].PlayScheduled(crossfadeStart);
            }
        }

        public void StartCrossfadeNextMeasure(double nextMeasure, float duration)
        {
            crossfadeState = CrossfadeState.Loading;
            crossfadeStart = nextMeasure;
            crossfadeDuration = duration;
        }

        public double GetCrossfadeEnd()
        {
            return crossfadeStart + crossfadeDuration;
        }

        public bool IsEnding(double nextMeasure, int tempo)
        {
            // We use nextmeasure + 60f / tempo * 2 to account for the extra 2 measures of silence added to the end of each clip. This allows reverbs/audio tails/etc. to finish
            return audioSources[0, 0].clip ? ((AudioSettings.dspTime - audioSources[0, 0].time + audioSources[0, 0].clip.length) < nextMeasure + 60f / tempo * 2f + 0.1 || !audioSources[0, 0].isPlaying) : true;
        }

        private static float LinearToCurve(float linear)
        {
            return 1 - Mathf.Pow(linear - 1, 2);
        }
    }
}
