using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Legato
{
    public class FSA_ShuffleQueue : FragmentSortingAlgorithm
    {
        private Queue<Fragment> fragmentQueue = new Queue<Fragment>();
        private Fragment[] playedFragments;

        public FSA_ShuffleQueue() { }
        public override void Start(Fragment[] fragments)
        {
            playedFragments = new Fragment[0];
            foreach (Fragment f in fragments)
            {
                fragmentQueue.Enqueue(f);
            }
        }

        // Removes the played fragment from the queue and adds it to the vector. If queue becomes empty, shuffle vector into queue
        public override Fragment GetNextFragment()
        {
            playedFragments.Append(fragmentQueue.Peek());

            if (fragmentQueue.Count == 1)
                shuffle();

            return fragmentQueue.Dequeue();
        }

        private void shuffle()
        {
            while (playedFragments.Length > 0)
            {
                int r = Random.Range(0, playedFragments.Length);
                fragmentQueue.Enqueue(playedFragments[r]);

                // Erase r element from array
                Fragment[] auxFragments = new Fragment[playedFragments.Length - 1];
                for(int i = 0; i < auxFragments.Length; i++)
                {
                    if (i < r) auxFragments[i] = playedFragments[i];
                    else auxFragments[i] = playedFragments[i + 1];
                }
                playedFragments = auxFragments;
            }
        }
    }
}