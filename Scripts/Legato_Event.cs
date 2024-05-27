using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public abstract class Legato_Event : MonoBehaviour
    {
        protected Legato_Emitter emitter;
        [SerializeField, Tooltip("The audio channel that the event will trigger on.")] public int channel;

        private void Start()
        {
            emitter = Legato_Emitter.GetInstance();
        }

        public abstract void Trigger();
    }
}
