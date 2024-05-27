using Legato;
using UnityEngine;
using UnityEngine.UI;

public class Legato_PadController : MonoBehaviour
{
    [SerializeField] private Toggle changeCurrent;
    [SerializeField] private Slider globalVol, channelVol;

    [SerializeField] private Legato_MotifEvent motif;
    [SerializeField] private Legato_InstrumentEvent[] instruments;

    private void Start()
    {
        ChangeCurrent();
        GlobalVol();
        ChannelVol();
    }

    public void ChangeCurrent()
    {
        motif.interrupt = changeCurrent.isOn;
        foreach(Legato_InstrumentEvent i in instruments) i.changeCurrent = changeCurrent.isOn;
    }

    public void GlobalVol()
    {
        Legato_Emitter.GetInstance().SetGlobalVolume(globalVol.value);
    }

    public void ChannelVol()
    {
        Legato_Emitter.GetInstance().SetVolume(0, channelVol.value);
    }
}
