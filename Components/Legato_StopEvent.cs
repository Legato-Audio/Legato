using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Legato
{
    public class Legato_StopEvent : Legato_Event
    {
        [SerializeField, Tooltip("If true, triggering this event will stop all channels at once.")]
        public bool applyToAll;

        public override void Trigger()
        {
            if (applyToAll) 
                emitter.StopAll();
            else
                emitter.Stop(channel);
        }
    }
}
