using UnityEngine;

namespace Legato
{
    public class Tempo : ScriptableObject
    {
        public int Get()
        {
            return int.Parse(this.name);
        }
    }
}