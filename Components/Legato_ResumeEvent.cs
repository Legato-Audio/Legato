using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Legato
{
    public class Legato_ResumeEvent : Legato_Event
    {
        [SerializeField, Tooltip("If true, triggering this event will resume all channels at once.")]
        public bool applyToAll;

        [SerializeField, Tooltip("If true, only channels that were playing before StopAll was called will be resumed. Does nothing if ApplyToAll is not checked.")]
        public bool onlyResumePreviouslyPlaying;

        public override void Trigger()
        {
            if (applyToAll) 
                emitter.ResumeAll(onlyResumePreviouslyPlaying);
            else
                emitter.Resume(channel);
        }
    }
}
