using System;
using System.Windows.Forms;

namespace DRED
{
    /// <summary>
    /// Handles keyboard shortcuts for the main form.
    /// </summary>
    internal sealed class KeyboardShortcutHandler
    {
        private readonly KeyboardActions _actions;

        public KeyboardShortcutHandler(KeyboardActions actions)
        {
            _actions = actions;
        }

        public bool HandleKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.F:
                        e.Handled = true;
                        _actions.OpenAdvancedSearch?.Invoke();
                        return true;
                    case Keys.S:
                        e.Handled = true;
                        _actions.ExportAllTabs?.Invoke();
                        return true;
                }
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N:
                        e.Handled = true;
                        if (_actions.IsUnlocked?.Invoke() == true)
                            _actions.AddRecord?.Invoke();
                        return true;
                    case Keys.E:
                        e.Handled = true;
                        if (_actions.IsUnlocked?.Invoke() == true)
                            _actions.EditRecord?.Invoke();
                        return true;
                    case Keys.Z:
                        e.Handled = true;
                        if (_actions.IsUndoEnabled?.Invoke() == true)
                            _actions.Undo?.Invoke();
                        return true;
                    case Keys.R:
                        e.Handled = true;
                        _actions.Refresh?.Invoke();
                        return true;
                    case Keys.F:
                        e.Handled = true;
                        _actions.FocusSearch?.Invoke();
                        return true;
                    case Keys.S:
                        e.Handled = true;
                        _actions.ExportCurrentTab?.Invoke();
                        return true;
                    case Keys.I:
                        e.Handled = true;
                        if (_actions.IsUnlocked?.Invoke() == true)
                            _actions.ImportFromExcel?.Invoke();
                        return true;
                    case Keys.Oemcomma:
                        e.Handled = true;
                        if (_actions.IsUnlocked?.Invoke() == true)
                            _actions.OpenSettings?.Invoke();
                        return true;
                    case Keys.D1:
                        e.Handled = true;
                        _actions.SelectTab?.Invoke(0);
                        return true;
                    case Keys.D2:
                        e.Handled = true;
                        _actions.SelectTab?.Invoke(1);
                        return true;
                    case Keys.D3:
                        e.Handled = true;
                        _actions.SelectTab?.Invoke(2);
                        return true;
                    case Keys.D4:
                        e.Handled = true;
                        _actions.SelectTab?.Invoke(3);
                        return true;
                    case Keys.D5:
                        e.Handled = true;
                        _actions.SelectTab?.Invoke(4);
                        return true;
                }
            }

            switch (e.KeyCode)
            {
                case Keys.F5:
                    e.Handled = true;
                    _actions.Refresh?.Invoke();
                    return true;
                case Keys.Delete:
                    if (_actions.IsSearchFocused?.Invoke() == true)
                        return false;
                    e.Handled = true;
                    if (_actions.IsUnlocked?.Invoke() == true)
                        _actions.DeleteRecord?.Invoke();
                    return true;
                case Keys.Escape:
                    e.Handled = true;
                    _actions.ClearSearchAndFilters?.Invoke();
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Bundles the action callbacks that keyboard shortcuts can trigger.
    /// </summary>
    internal sealed class KeyboardActions
    {
        public Action? AddRecord { get; init; }
        public Action? EditRecord { get; init; }
        public Action? DeleteRecord { get; init; }
        public Action? Undo { get; init; }
        public Action? Refresh { get; init; }
        public Action? FocusSearch { get; init; }
        public Action? ExportCurrentTab { get; init; }
        public Action? ExportAllTabs { get; init; }
        public Action? ImportFromExcel { get; init; }
        public Action? OpenSettings { get; init; }
        public Action? OpenAdvancedSearch { get; init; }
        public Action<int>? SelectTab { get; init; }
        public Action? ClearSearchAndFilters { get; init; }
        public Func<bool>? IsUnlocked { get; init; }
        public Func<bool>? IsUndoEnabled { get; init; }
        public Func<bool>? IsSearchFocused { get; init; }
    }
}
