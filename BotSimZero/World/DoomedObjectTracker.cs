using System;
using System.Collections.Generic;

namespace BotSimZero.World
{
    public class DoomedObjectTracker
    {
        // SortedSet with a custom comparer to sort by DeathMillis
        public readonly SortedSet<DoomedComponent> DoomedObjects = new SortedSet<DoomedComponent>(new DoomedComponentComparer());

        public void Update()
        {
            // Use a while loop to avoid creating temporary lists and minimize allocations
            while (DoomedObjects.Count > 0)
            {
                var first = DoomedObjects.Min; // Get the first (smallest) element
                if (first.IsExpired())
                {
                    first.Die();
                    DoomedObjects.Remove(first); // Remove directly
                }
                else
                {
                    // Stop as soon as we find a non-expired component
                    break;
                }
            }
        }

        public void Register(DoomedComponent doomed)
        {
            DoomedObjects.Add(doomed);
        }
    }

    // Custom comparer for sorting DoomedComponent by ExpirationTime
    internal class DoomedComponentComparer : IComparer<DoomedComponent>
    {
        public int Compare(DoomedComponent x, DoomedComponent y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException("DoomedComponent cannot be null");

            // Compare by ExpirationTime
            return x.ExpirationTime.CompareTo(y.ExpirationTime);
        }
    }
}
