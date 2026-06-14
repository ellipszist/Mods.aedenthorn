using StardewValley;
using System;

namespace SimpleCooking
{
    internal class TrackedItem : ITrackedStack
    {
        public TrackedItem(StardewValley.Object value, Action onEmpty)
        {
            Sample = value;
            OnEmpty = onEmpty;
        }

        public StardewValley.Object Value { get; }
        public Action OnEmpty { get; }

        /*********
        ** Accessors
        *********/
        /// <summary>A sample item for comparison.</summary>
        /// <remarks>This should be equivalent to the underlying item (except in stack size), but *not* a reference to it.</remarks>
        public Item Sample { get; }

        /// <summary>The identifier for the type definition which contains the item, matching one of the <see cref="ItemRegistry"/> <c>type_</c> constants.</summary>
        public string Type { get; }

        /// <summary>The number of items in the stack.</summary>
        public int Count { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Remove the specified number of this item from the stack.</summary>
        /// <param name="count">The number to consume.</param>
        public void Reduce(int count)
        {
            OnEmpty();
        }

        /// <summary>Remove the specified number of this item from the stack and return a new stack matching the count.</summary>
        /// <param name="count">The number to get.</param>
        public Item? Take(int count)
        {
            OnEmpty();
            return null;
        }
    }
}