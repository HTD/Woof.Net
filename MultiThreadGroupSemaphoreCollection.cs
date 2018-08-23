using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Woof.Net {

    /// <summary>
    /// Multi-thread semaphore group collection allows multiple groups of threads to block until signal for the group occurs.
    /// </summary>
    public class MultiThreadGroupSemaphoreCollection : IEnumerable<KeyValuePair<string, MultiThreadSemaphore>>, IDisposable {

        /// <summary>
        /// Contains multi-thread semaphore groups.
        /// </summary>
        private readonly ConcurrentDictionary<string, MultiThreadSemaphore> GroupSemaphores = new ConcurrentDictionary<string, MultiThreadSemaphore>();

        /// <summary>
        /// True while disposing the collection.
        /// </summary>
        private bool IsDisposing;

        /// <summary>
        /// Default name for the group.
        /// </summary>
        private const string DefaultGroupName = "default";

        /// <summary>
        /// Gets the <see cref="MultiThreadSemaphore"/> for the specified group name.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        /// <returns>Multi-thread semaphore.</returns>
        public MultiThreadSemaphore this[string groupName] =>
            GroupSemaphores.ContainsKey(groupName) ? GroupSemaphores[groupName] : null;

        /// <summary>
        /// Waits for the release signal for the group.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        public void WaitEvent(string groupName = DefaultGroupName) {
            if (IsDisposing) return;
            if (GroupSemaphores.ContainsKey(groupName))
                GroupSemaphores[groupName].WaitEvent();
            else {
                var newSemaphore = new MultiThreadSemaphore();
                if (GroupSemaphores.TryAdd(groupName, newSemaphore)) newSemaphore.WaitEvent();
                else GroupSemaphores[groupName].WaitEvent();
            }
        }

        /// <summary>
        /// Releases all threads in the group.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        public void ReleaseAll(string groupName = DefaultGroupName) {
            if (IsDisposing) return;
            if (GroupSemaphores.ContainsKey(groupName)) GroupSemaphores[groupName].ReleaseAll();
        }

        /// <summary>
        /// Clears the collection, disposes all semaphores.
        /// </summary>
        public void Clear() => Dispose();

        /// <summary>
        /// Disposes all semaphores, clears the collection.
        /// </summary>
        public void Dispose() {
            IsDisposing = true;
            foreach (var semaphore in GroupSemaphores.Values) semaphore.Dispose();
            GroupSemaphores.Clear();
            IsDisposing = false;
        }

        /// <summary>
        /// Iterates over all items.
        /// </summary>
        /// <returns>Items enumerator.</returns>
        public IEnumerator<KeyValuePair<string, MultiThreadSemaphore>> GetEnumerator() => GroupSemaphores.GetEnumerator();

        /// <summary>
        /// Iterates over all items.
        /// </summary>
        /// <returns>Items enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GroupSemaphores.GetEnumerator();

    }

}