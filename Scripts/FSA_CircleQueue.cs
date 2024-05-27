using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public class FSA_CircleQueue : FragmentSortingAlgorithm
    {
        private Queue<Fragment> fragmentQueue = new Queue<Fragment>();
        public FSA_CircleQueue() { }

        public override void Start(Fragment[] fragments)
        {
            foreach (Fragment f in fragments)
            {
                fragmentQueue.Enqueue(f);
            }
        }

        // Returns the next element in the queue, then add that element to the back of the queue
        public override Fragment GetNextFragment()
        {
            fragmentQueue.Enqueue(fragmentQueue.Peek());
            return fragmentQueue.Dequeue();
        }
    }
}
