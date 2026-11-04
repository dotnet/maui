# .NET MAUI TODO comment report

**381 TODO-marker lines in 240 files**, from a repository-wide scan of 25,960 tracked files. Controls and Core contain **306 lines (80.3%)**. These are source-comment counts, not counts of unique bugs or confirmed unresolved work.

Snapshot: `43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1`, inspected November 4, 2026. The repository working tree was clean when scanned. This is a fixed source-snapshot audit; the reports and exports are stored alongside this document. Application source was not changed.

## Reports and exports

- [Prioritized triage report](todo-triage.md): context-reviewed implementation gaps, coverage debt, and comments that should not be implemented blindly.
- [Complete inventory](todo-inventory.md): every matching source line, continuation comments, role, and commit-pinned source link, grouped by area.
- [CSV export](todo-inventory.csv): all 381 records, including nearby source context, for filtering and sorting.
- [JSON export](todo-inventory.json): the same inventory plus scope, counters, repeated comments, and excluded matches.

The CSV stores `continuation`, `context_before`, and `context_after` as JSON-encoded strings so every record occupies one physical line without losing original whitespace. Decode these three cells as JSON strings when processing the CSV programmatically.

## Distribution by area

| Area | TODO-marker lines | Files | Share |
| --- | ---: | ---: | ---: |
| Controls | 210 | 129 | 55.1% |
| Core | 96 | 74 | 25.2% |
| Essentials | 44 | 18 | 11.5% |
| TestUtils | 12 | 9 | 3.1% |
| BlazorWebView | 9 | 3 | 2.4% |
| Graphics | 5 | 4 | 1.3% |
| .github | 3 | 1 | 0.8% |
| SingleProject / Resizetizer | 2 | 2 | 0.5% |
| **Total** | **381** | **240** | **100%** |

Percentages are rounded independently.

## Product versus test debt

| File role | TODO-marker lines |
| --- | ---: |
| Product source / build tooling | 269 |
| Tests | 91 |
| Test infrastructure | 12 |
| Samples | 6 |
| Script template placeholders | 3 |
| **Total** | **381** |

Roles are assigned by directory, not by whether a code path is active. For example, an internal implementation referenced only by tests can still reside under a product-source directory.

## Highest-value follow-up candidates

| Candidate | Source evidence | Recommended disposition |
| --- | --- | --- |
| Skia image downsizing | `SkiaImage.cs:48-73`: both `Downsize` overloads return `null`; one carries a TODO marker. | Investigate as a concrete implementation gap, with image-size and disposal-contract coverage. |
| Skia transform concatenation | `SkiaCanvas.cs:831-841`: the incoming transform is unused and concatenation/application code is commented out. | Check the public drawing path and transform composition behavior before choosing a fix. |
| XAML source-generator diagnostics | `KnownMarkups.cs:604-610`: a parse-failure path throws a general exception; other nearby TODOs already have diagnostic reporting. | Separate remaining diagnostic work from stale markers. |
| Screenshot failure handling | `Screenshot.ios.cs:42-45,70-73,104-107`: failed render attempts have empty TODO handling blocks. | Define and cover failed/zero-size rendering behavior rather than assuming success. |
| Disabled or restricted coverage | A fully commented UI-test file, Windows WebView2 conditional exclusions, and non-Windows Resizetizer early returns. | Restore coverage only after checking each guard's original reason and prerequisites. |

See the [triage report](todo-triage.md) for precise source links, caveats, and further candidates. These are investigation priorities, not runtime-confirmed defects or release-blocker classifications.

## Concentrated backlog

| Cluster | TODO-marker lines | Files | Observation |
| --- | ---: | ---: | --- |
| Controls XAML source generation | 29 | 11 | Diagnostics, binding behavior, conversion, and implementation cleanup. |
| Controls XAML build tasks | 6 | 1 | All in `SetPropertiesVisitor.cs`; includes one historical TODO mention. |
| Controls binding source generation | 1 | 1 | Generic-parameter support comment. |
| Controls compatibility implementation | 28 | 16 | Legacy behavior, migration notes, and platform API deprecations. |
| Essentials device tests | 26 | 9 | Includes 20 copies of a test-runner setup comment. |
| Graphics.Skia | 4 | 3 | Includes the two concrete implementation gaps highlighted above. |

Clusters are subsets of the area totals, not additional TODOs.

## Backlog quality

- **20 low-information markers:** 8 bare `TODO` comments and 12 whose substantive text is only `MAUI`. Nearby code may explain them, but the marker itself does not define completion.
- **34 TODOs on `#pragma` lines:** largely platform/deprecation or obsolete-API warning suppressions. A suppression comment is not, by itself, evidence of a current runtime failure.
- **22 marker lines contain a URL** on the marker line or its immediately following comment block (5.8%). Lack of a nearby URL does not establish that no issue exists elsewhere.
- **17 version/dependency-bound comments** mention .NET 8, 9, 10, 11, or `XAMCORE_5_0`. Some are candidates for refresh; public API changes still require compatibility review.
- **9 markers contain explicit calendar dates**, ranging from 2018 to 2021. These are dates written in comments, not Git-blame ages.
- **8 scaffolding markers:** 3 script-template instructions, 3 auto-generated Java constructor comments, and 2 analyzer code-fix scaffold comments. These are included for completeness, not treated as eight product defects.
- **1 historical reference:** `Build.Tasks/SetPropertiesVisitor.cs:1853` says the comment "used to be" a TODO. It remains in the inventory with a historical label.

These dimensions overlap and must not be added together.

## Repeated themes worth consolidating

| Repeated text or theme | Marker lines | Interpretation |
| --- | ---: | --- |
| Essentials test-runner app should perform setup | 20 | A shared harness concern repeated across sensor and battery tests. |
| WebView2 / Windows App SDK request-interception limitation | 8 | Four marker lines each in BlazorWebView and HybridWebView tests; not eight independently established bugs. |
| Shapes should use a remapped mapper | 7 | A cross-shape refactoring theme. |
| SecureStorage "Use Logger here?" | 6 | Logging consistency across platform implementations; not a security finding. |
| Future removal of API linked to issue 30155 | 4 | One linked cleanup theme repeated in implementation and tests. |
| Screenshot "test/handle this case" | 4 | Related rendering-edge-case work, not four reproduced failures. |

## Scope and reproducibility

The broad search was case-insensitive `git grep -n -I -i -E '//.*todo' -- .`, run against the current working tree across all tracked files, including hidden directories. It yielded 384 candidate source lines.

To repeat the broad search without counting these reports or later source changes, use the pinned snapshot:

```bash
git grep -n -I -i -E '//.*todo' 43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1 -- .
```

The inventory retains textual markers matching `//+\s*TODO\b`, case-insensitively. This includes `//TODO`, `// todo`, trailing comments, XML documentation comments, Java comments, and markers in disabled code. One row represents one matching source line.

Three broad-search matches were excluded:

1. Two generated Bootstrap CSS source-map JSON files contain embedded third-party source text, not local line comments.
2. `Issue27797.xaml.cs:125` describes moving a workflow item back to the "TODO" state, rather than marking unfinished implementation.

The result is **381 marker lines / 381 marker occurrences**, comprising **377 C# lines and 4 Java lines**. There were no non-ignored untracked files or tracked submodule entries. Binary files are skipped by the text search.

This is a textual inventory, not a full language-parser or whole-program audit. Other comment styles (`/* TODO */`, `# TODO`, `<!-- TODO -->`), ignored build outputs, and upstream issue/PR status are outside scope. Context review was targeted to the candidates discussed in the triage report; the other markers were inventoried without proving whether their underlying work is still needed. No application behavior was executed or certified.
