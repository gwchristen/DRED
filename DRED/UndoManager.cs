using System;
using System.Collections.Generic;

namespace DRED
{
    /// <summary>
    /// Stores and retrieves undo actions.
    /// </summary>
    public static class UndoManager
    {
        private const int MaxActions = 10;
        private static readonly object SyncRoot = new();
        private static readonly List<UndoableAction> Actions = new();

        /// <summary>
        /// Gets whether an undo action is available.
        /// </summary>
        public static bool CanUndo
        {
            get
            {
                lock (SyncRoot)
                    return Actions.Count > 0;
            }
        }

        /// <summary>
        /// Pushes an action onto the undo stack.
        /// </summary>
        public static void Push(UndoableAction action)
        {
            if (action == null) return;

            lock (SyncRoot)
            {
                Actions.Add(action);
                if (Actions.Count > MaxActions)
                    Actions.RemoveAt(0);
            }
        }

        /// <summary>
        /// Returns the most recent undo action without removing it.
        /// </summary>
        public static UndoableAction? Peek()
        {
            lock (SyncRoot)
            {
                if (Actions.Count == 0) return null;
                return Actions[^1];
            }
        }

        /// <summary>
        /// Removes and returns the most recent undo action.
        /// </summary>
        public static UndoableAction? Pop()
        {
            lock (SyncRoot)
            {
                if (Actions.Count == 0) return null;
                int idx = Actions.Count - 1;
                var action = Actions[idx];
                Actions.RemoveAt(idx);
                return action;
            }
        }

        /// <summary>
        /// Clears all undo actions.
        /// </summary>
        public static void Clear()
        {
            lock (SyncRoot)
                Actions.Clear();
        }
    }
}
