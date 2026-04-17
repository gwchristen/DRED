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
        public UndoActionType ActionType { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public RecordData PreviousData { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
