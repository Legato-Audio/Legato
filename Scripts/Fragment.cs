using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Legato
{
    public class Fragment : ScriptableObject
    {
        #region Auxiliary Containers
        // Contains all tempo variations of a single fragment, organized by instrument, then tempo
        [Serializable]
        public class AuxRenderContainer
        {
            public List<AuxTempoContainer> auxLists;
            public AuxRenderContainer(AudioClip[][] clips)
            {
                auxLists = new List<AuxTempoContainer>(clips.Length);
                foreach (AudioClip[] c in clips) 
                {
                    auxLists.Add(new AuxTempoContainer(c));
                }
            }
            public AudioClip Get(int i, int t)
            {
                return auxLists[i].renders[t];
            }
        }

        // Contains all tempo variations of a single fragment with a single instrument
        [Serializable]
        public class AuxTempoContainer
        {
            // SerializeField is unnecessary for public fields
            public AudioClip[] renders;

            public AuxTempoContainer(AudioClip[] renders)
            {
                this.renders = renders;
            }
        }
        #endregion

        public AuxRenderContainer renders; // instruments x tempos
        [SerializeField] private string[] instruments, tempos;

        public void SetInstrumentArray(string[] i)
        {
            instruments = i;
        }
        
        public void SetTempoArray(string[] t)
        {
            tempos = t;
        }

        public void SetRenders(AudioClip[][] tempRenders)
        {
            renders = new AuxRenderContainer(tempRenders);
        }

        public int GetInstrumentIndex(string instrument)
        {
            int i = 0;
            while (i < instruments.Length)
            {
                if (instruments[i] == instrument) return i;
                ++i;
            }
            Debug.LogError("Instrument \"" + instrument + "\" not found.");
            return int.MaxValue;
        }

        public int GetTempoIndex(string tempo)
        {
            int t = 0;
            while (t < tempos.Length)
            {
                if (tempos[t] == tempo) return t;
                ++t;
            }
            Debug.LogError("Tempo \"" + tempo + "\" not found.");
            return int.MaxValue;
        }

        public AudioClip GetClip(Instrument instrument, Tempo tempo)
        {
            AudioClip clip = renders.Get(GetInstrumentIndex(instrument.name), GetTempoIndex(tempo.name));
            if (clip == null) Debug.LogError("The fragment \"" + name + "\" is not rendered in the instrument \"" + instrument.name + "\" and/or the tempo \"" + tempo.name + "\".");
            return clip;
        }
    }
}