using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public class Legato_TempoEvent : Legato_Event
    {
        [SerializeField, Tooltip("The tempo that will be set on triggering the event.")] 
        public Tempo tempo;

        [SerializeField, Tooltip("If true, changes in tempo will apply to all channels at once, regardless of selected channel.")]
        public bool applyToAll;

        public override void Trigger()
        {
            if (applyToAll) 
                emitter.SetTempo(tempo);
            else
                emitter.SetTempo(channel, tempo);
        }
    }
}
