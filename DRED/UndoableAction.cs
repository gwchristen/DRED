using System;

namespace DRED
{
    /// <summary>
    /// Identifies supported undo action types.
    /// </summary>
    public enum UndoActionType
    {
        Delete,
        Edit,
    }

    /// <summary>
    /// Represents a single undoable action.
    /// </summary>
    public sealed class UndoableAction
    {
        /// <summary>
        /// Gets or sets the type of action that can be undone.
        /// </summary>
        public UndoActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the table containing the affected record.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Id of the affected record.
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Gets or sets the previous record values used to restore state.
        /// </summary>
        public RecordData PreviousData { get; set; } = new();

        /// <summary>
        /// Gets or sets when the undoable action was captured.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the action.
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
