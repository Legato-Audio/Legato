using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legato
{
    public class FSA_Random : FragmentSortingAlgorithm
    {
        private Fragment[] fragmentList;

        public FSA_Random() { }
        public override void Start(Fragment[] fragments)
        {
            fragmentList = fragments;
        }

        // Return a random element from the queue
        public override Fragment GetNextFragment()
        {
            return fragmentList[Random.Range(0, fragmentList.Length)];
        }
    }
}