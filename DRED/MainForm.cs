// ... Previous Content ...

            // Defer SplitterDistance until the form is fully visible and all
            // controls have been laid out with real dimensions. Using Shown +
            // BeginInvoke guarantees the SplitContainer has a non-zero Width
            // before we touch SplitterDistance.
            this.Shown += (s, e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    foreach (var sc in _splitContainers)
                    {
                        if (sc == null) continue;
                        try { sc.SplitterDistance = 300; }
                        catch (InvalidOperationException) { /* control not ready yet */ }
                    }
                }));
            };