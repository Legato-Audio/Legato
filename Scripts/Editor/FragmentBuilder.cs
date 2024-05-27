using Legato;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class FragmentBuilder
{
    public static void CreateFragment(string fragmentName)
    {
        string[] instruments, tempos;


        Fragment fragment = ScriptableObject.CreateInstance<Fragment>();

        // Get names of all instruments
        Instrument[] inst = Resources.LoadAll<Instrument>("Legato/Instruments");
        instruments = new string[inst.Length];
        for (int i = 0; i < instruments.Length; ++i)
        {
            instruments[i] = inst[i].name;
        }

        // Get names of all tempos
        Tempo[] tem = Resources.LoadAll<Tempo>("Legato/Tempos");
        tempos = new string[tem.Length];
        for (int i = 0; i < tempos.Length; ++i)
        {
            tempos[i] = tem[i].name;
        }

        fragment.SetInstrumentArray(instruments);
        fragment.SetTempoArray(tempos);

        // Find and store all rendered samples
        AudioClip[][] tempRenders = new AudioClip[instruments.Length][];
        for (int i = 0; i < instruments.Length; ++i) { tempRenders[i] = new AudioClip[tempos.Length]; }

        string[] clipID = AssetDatabase.FindAssets('.' + fragmentName + '.', new string[] { "Assets/Resources/Legato/RenderedSamples" });
        for (int f = 0; f < clipID.Length; f++)
        {
            string[] clipName = AssetDatabase.GUIDToAssetPath(clipID[f]).Split('.');
            string instrument = clipName[2], tempo = clipName[3];

            tempRenders[fragment.GetInstrumentIndex(instrument)][fragment.GetTempoIndex(tempo)] = Resources.Load<AudioClip>("Legato/RenderedSamples/motif." + fragmentName + '.' + instrument + '.' + tempo);
            //Debug.Log("Temp")
        }

        
        fragment.SetRenders(tempRenders);
        AssetDatabase.CreateAsset(fragment, "Assets/Resources/Legato/Fragments/" + fragmentName + ".asset");
    }
}
