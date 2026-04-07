            // Defer SplitterDistance until controls are fully rendered
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