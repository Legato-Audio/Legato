using System;
using System.Collections.Generic;
using UnityEngine;  

namespace Legato
{

    [Serializable]
    public class Channel
    {
        struct QueuedMotifData
        {
            public bool pending;
            public Fragment motif;
            public float priority;
        }

        struct QueuedCrossfadeData
        {
            public bool pending;
            public Instrument instrument;
            public float fadeDuration;
        }

        [SerializeField] private Fragment[] fragments;
        [SerializeField] SortingAlgorithms algorithm;
        [SerializeField, Range(0.0f, 1.0f)] private float volume = 1;
        [SerializeField] private Instrument instrument;
        [SerializeField] private Tempo tempo;
        [SerializeField] private int beatsPerMeasure = 4;
        [SerializeField] private bool playOnStart = true;

        private DynamicPriorityQueue<Fragment> motifQueue = new DynamicPriorityQueue<Fragment>();
        private FragmentSortingAlgorithm sortingAlgorithm;

        private Audio audio;
        private bool playing, inBuffer, debug;
        private Fragment currFragment;
        private float globalVolume;
        private double nextMeasure;
        private QueuedMotifData queuedMotif;
        private QueuedCrossfadeData queuedCrossfade;
        private Instrument nextInstrument;
        private Tempo nextTempo;

        public void Awake(GameObject go, bool d)
        {
            switch (algorithm)
            {
                case SortingAlgorithms.CircleQueue:
                    sortingAlgorithm = new FSA_CircleQueue();
                    break;
                case SortingAlgorithms.Random:
                    sortingAlgorithm = new FSA_Random();
                    break;
                case SortingAlgorithms.Shuffle:
                    sortingAlgorithm = new FSA_ShuffleQueue();
                    break;
                default:
                    Debug.LogError("Unrecognized fragment sorting algorithm. " +
                        "New algorithms can be created by implementing the Legato::FragmentSortingAlgorithm class and added to the enum within the interface file.");
                    break;
            }
            sortingAlgorithm.Start(fragments);

            foreach (Fragment f in fragments)
            {
                if (f == null) Debug.LogError("Null fragment found. At least one fragment in the channel is null.");
            }

            audio = new Audio(go, this, d);

            playing = false;
            inBuffer = false;
            nextMeasure = AudioSettings.dspTime + 2 * Legato_Emitter.loadingBuffer;

            nextInstrument = instrument;
            nextTempo = tempo;

            queuedMotif.pending = false;
            queuedCrossfade.pending = false;

            debug = d;
        }

        public void Start()
        {
            if (playOnStart) Resume();
        }

        public void Update(float dT)
        {
            if (playing && currFragment == null && fragments.Length == 0 && motifQueue.Count == 0 && queuedMotif.pending == false) playing = false;

            if(playing && !inBuffer && AudioSettings.dspTime > nextMeasure - Legato_Emitter.loadingBuffer) // Loading point
            {
                inBuffer = true;

                if (queuedMotif.pending || (audio.IsEnding(nextMeasure, tempo.Get()) && (fragments.Length > 0 || motifQueue.Count > 0))) // New fragment incoming
                {
                    if (queuedMotif.pending)
                    {
                        Debug.Log("Pending queued motif: " + queuedMotif.motif);
                        queuedMotif.pending = false;
                        currFragment = queuedMotif.motif;
                    }
                    else currFragment = PopFragment();

                    tempo = nextTempo;

                    AudioClip clip1 = currFragment.GetClip(instrument, tempo);
                    AudioClip clip2 = currFragment.GetClip(nextInstrument, tempo);
                    audio.Play(clip1, clip2, nextMeasure);
                    if(queuedCrossfade.pending && !audio.IsCrossfading())
                    {
                        audio.StartCrossfadeNextMeasure(nextMeasure, queuedCrossfade.fadeDuration);
                        queuedCrossfade.pending = false;
                    }
                }
                else // Fragment will keep playing
                {
                    bool changeInstrument = false, changeTempo = false;
                    if (queuedCrossfade.pending && !audio.IsCrossfading()) // Start pending crossfade
                    {
                        nextInstrument = queuedCrossfade.instrument;
                        queuedCrossfade.pending = false;
                        changeInstrument = true;
                    }
                    if(nextTempo != tempo) changeTempo = true;

                    if (changeInstrument || changeTempo)
                    {
                        AudioClip clip1 = currFragment.GetClip(instrument, nextTempo);
                        AudioClip clip2 = currFragment.GetClip(nextInstrument, nextTempo);
                        float time = audio.GetTimeNewTempo(clip1, nextMeasure, tempo.Get(), nextTempo.Get());
                        audio.Play(clip1, clip2, nextMeasure, time);
                        if (changeInstrument) audio.StartCrossfadeNextMeasure(nextMeasure, queuedCrossfade.fadeDuration);

                        tempo = nextTempo;
                    }
                }
            }
            else if(AudioSettings.dspTime > nextMeasure + 0.01)
            {
                nextMeasure += 60.0 / tempo.Get() * beatsPerMeasure;
                inBuffer = false;
                audio.ClipChange();
                if (audio.IsEnding(nextMeasure, tempo.Get())) currFragment = null;
            }
            audio.Update(dT);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resume() // Reproduce desde donde estaba si hab�a pausado, desde el fragmento siguiente si hab�a hecho stop
        {
            playing = true;
        }

        public void Stop()
        {
            if (playing)
            {
                playing = false;
                audio.Stop();
            }
        }

        public void SetVolume(float v)
        {
            volume = v;
            audio.SetVolume(volume * globalVolume);
        }

        public void SetGlobalVolume(float v)
        {
            globalVolume = v;
            audio.SetVolume(volume * globalVolume);
        }

        public void SetSpatialBlend(float spatialBlend)
        {
            audio.SetSpatialBlend(spatialBlend);
        }

        public void Set3DRolloffMode(AudioRolloffMode rolloffMode)
        {
            audio.Set3DRolloffMode(rolloffMode);
        }

        public void Set3DMinDistance(float minDistance)
        {
            audio.Set3DMinDistance(minDistance);
        }

        public void Set3DMaxDistance(float maxDistance)
        {
            audio.Set3DMaxDistance(maxDistance);
        }

        public void PlayMotif(Fragment motif, bool interrupt, float priority, float priorityVariance)
        {
            playing = true;

            if (interrupt)
            {
                if (!queuedMotif.pending || priority >= queuedMotif.priority)
                {
                    queuedMotif.pending = true;
                    queuedMotif.motif = motif;
                    queuedMotif.priority = priority;
                }
            }
            else motifQueue.Push(motif, priority, priorityVariance);
        }

        public void CancelMotif(Fragment motif) // TO DO: a�adir par�metros opcionales
        {
            Queue<Fragment> aux = new Queue<Fragment>();
            while(motifQueue.Count > 0)
            {
                Fragment frag = motifQueue.Pop();
                if(frag != motif) aux.Enqueue(frag);
            }
        }

        public void SetInstrument(Instrument i, bool changeCurrent, float fadeDuration) // changeCurrent indica si se cambia el instrumento del motivo actual (false, espera al siguiente)
        {
            if (debug) Debug.Log("Set instrument " + i.name);

            if (nextInstrument != i && !(queuedCrossfade.pending && queuedCrossfade.instrument == i))
            {
                if (changeCurrent && currFragment != null && !inBuffer && !audio.IsCrossfading())
                {
                    nextInstrument = i;
                    AudioClip clip = currFragment.GetClip(i, tempo);
                    audio.StartCrossfadeNow(clip, fadeDuration);
                    queuedCrossfade.pending = false;
                    if (audio.GetCrossfadeEnd() < nextMeasure) instrument = nextInstrument;
                }
                else if (changeCurrent && currFragment != null && (inBuffer || audio.IsCrossfading()))
                {
                    queuedCrossfade.pending = true;
                    queuedCrossfade.instrument = i;
                    queuedCrossfade.fadeDuration = fadeDuration;
                }
                else instrument = nextInstrument = i; // changeCurrent = false
            }
            else if (queuedCrossfade.pending && nextInstrument == i) queuedCrossfade.pending = false;
        }

        public void SetTempo(Tempo t)
        {
            if (debug) Debug.Log("Set tempo " + t.Get());
            nextTempo = t;
        }

        public void CrossfadeEnd()
        {
            instrument = nextInstrument;
        }

        private Fragment PopFragment()
        {
            Fragment frag = null;
            if (motifQueue.Count == 0)
            {
                if (fragments.Length > 0) frag = sortingAlgorithm.GetNextFragment();
                else Debug.LogWarning("No loaded fragments in channel!");
            }
            else frag = motifQueue.Pop();
            return frag;
        }
    }
}
