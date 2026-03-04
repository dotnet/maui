# Example Invocation

**Inputs provided:**
```yaml
problem: |
  CollectionView throws ObjectDisposedException when navigating back
  from a page with a CollectionView on Android.

test_command: |
  pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue54321"

target_files:
  - src/Controls/src/Core/Handlers/Items/ItemsViewHandler.Android.cs
  - src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Android.cs

platform: android

hints: |
  - The issue seems related to disposal timing
  - Similar issue was fixed in ListView by checking IsDisposed before accessing adapter
  - Focus on the Disconnect/Cleanup methods
```

**Skill execution:** Reads context → Analyzes target files → Designs fix (add IsDisposed check) → Applies fix → Runs test (PASS) → Reports result → Reverts changes
