using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class that stores a vector of objects based on priority. These objects can have a priority that increases or decreases over time.
/// Each time the top item is popped, all objects in the vector have their priority updated before the one with highest priority is returned and erased.
/// Highest priority value is returned first. If an item's priority falls below zero, it is removed from the queue.
/// </summary>
/// <typeparam name="T"></typeparam>
namespace Legato
{

    public class DynamicPriorityQueue<T>
    {
        private class DynamicPrioItem
        {
            public DynamicPrioItem(T item_, float initialPrio, float prioVariation_)
            {
                item = item_;
                priority = initialPrio;
                prioVariation = prioVariation_;
                lastUpdate = Time.time;
            }

            public T item;
            public float priority;
            private float prioVariation;    // The speed at which the priority changes over time (in units/second)
            private float lastUpdate;      // The moment (in seconds) that the items priority was last updated

            public void UpdatePrio(float time)
            {
                priority += (prioVariation * (time - lastUpdate));
                lastUpdate = time;
            }
        }

        private List<DynamicPrioItem> l;

        public DynamicPriorityQueue()
        {
            l = new List<DynamicPrioItem>();
        }

        public int Count
        {
            get => l.Count;
        }

        public void Push(T item, float initialPriority = 0f, float priorityVariation = 0f)
        {
            l.Add(new DynamicPrioItem(item, initialPriority, priorityVariation));
        }

        public T Pop() 
        {
            int i = 0, highestPos = -1;
            float time = Time.time, highestPrio = float.MinValue;
            while (i < l.Count)
            {
                l[i].UpdatePrio(time);
                
                if (l[i].priority < 0f)
                {
                    l.RemoveAt(i);
                }
                else
                {
                    if (l[i].priority > highestPrio)
                    {
                        highestPrio = l[i].priority;
                        highestPos = i;
                    }
                    ++i;
                }
            }
            if (highestPos > -1)
            {
                T item = l[highestPos].item;
                l.RemoveAt(highestPos);
                return item;
            }
            else
            {
                return default;
            }
        }
    }

}