# macOS Tahoe visual expectations

These are reviewed, OS-specific overrides for native Mac Catalyst UI on macOS 26.
`UITest.VerifyScreenshot` selects an override only when that exact image exists
in this directory. All other images continue to use `snapshots/mac`, which also
remains the environment for older macOS versions.

The fallback is based only on a missing override, never on a failed comparison.
Comparison tolerances, density normalization, and logical screenshot crop values
are unchanged. Do not copy the full legacy corpus into this directory.

Before adding an override, compare the actual CI attachment with the legacy
image and the test's expected control semantics. Native system chrome, control
metrics, and Retina rendering can legitimately differ on Tahoe. Missing content,
lost explicitly configured colors, clipping, incorrect sizing or state, and
unexplained gesture/event changes must remain failures, not new expectations.
Keep the existing `snapshots/mac` images unchanged.
