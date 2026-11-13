# Example Invocation

**Inputs provided:**
```yaml
problem: |
  CollectionView throws ObjectDisposedException when navigating back
  from a page with a CollectionView on Android.

test_command: |
  pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "Issue54321"
  # For device tests use: pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform android -TestFilter "Category=CollectionView"
  # For unit tests use: dotnet test <project.csproj> --filter "TestClassName"

target_files:
  - src/Controls/src/Core/Handlers/Items/ItemsViewHandler.Android.cs
  - src/Controls/src/Core/Handlers/Items/CollectionViewHandler.Android.cs

platform: android

hints: |
  - The issue seems related to disposal timing
  - Similar issue was fixed in ListView by checking IsDisposed before accessing adapter
  - Focus on the Disconnect/Cleanup methods
```

**Skill execution:** Reads context → Analyzes target files → Designs fix (add IsDisposed check) → Applies fix → Performs inline expert self-review against `.github/agents/maui-expert-reviewer.md` rules and writes `reviewer-findings.json` (`[]` if clean) → Runs test (PASS) → If code changed during the test loop, refreshes `reviewer-findings.json` against the final diff → Captures artifacts → Reverts changes → Reports result
