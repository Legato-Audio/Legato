using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public class Legato_InstrumentEvent : Legato_Event
    {
        [SerializeField, Tooltip("The instrument that will be set on triggering the event.")]
        public Instrument instrument;

        [SerializeField, Tooltip("If true, any music currently playing be crossfade to the instrument on event trigger. If false, the current fragment will finish playing with its current instrument and the next one in the channel will use the new instrument.")]
        public bool changeCurrent;

        [SerializeField, Tooltip("If greater than 0, triggering the event will cause the channel to crossfade its instrument over fadeDuration seconds."), Range(0f, 30f)]
        public float fadeDuration;

        public override void Trigger()
        {
            emitter.SetInstrument(channel, instrument, changeCurrent, fadeDuration);
        }
    }
}
