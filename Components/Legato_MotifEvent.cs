using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public class Legato_MotifEvent : Legato_Event
    {
        [SerializeField, Tooltip("The motif that will play on triggering the event.")]
        public Fragment motif;

        [SerializeField, Tooltip("If true, the motif will be played in the selected channel at the next possible bar. If false, it will be added to the dynamic priority queue.")]
        public bool interrupt;

        [SerializeField, Tooltip("Triggered motifs are added to a queue. When the current fragment ends, the highest priority motif will play next.")]
        public float priority;

        [SerializeField, Tooltip("Rate that the motif's queued priority changes over time (in units/second). Can be positive or negative. If the motif's priority is below 0, the motif is removed from the queue.")]
        public float priorityOverTime;

        public override void Trigger()
        {
            emitter.PlayMotif(channel, motif, interrupt, priority, priorityOverTime);
        }
    }
}
