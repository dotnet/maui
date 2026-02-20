# Base-chain Data-Structure PoC Evaluation Log

This file is append-only and records each cumulative DS change evaluation.

## Canonical runbook reminder (before every benchmark run)
- Primary skill: `.github/skills/benchmarkdotnet-poc/SKILL.md`
- Supplemental shortcut: `.github/skills/base-chain-benchmark-loop/SKILL.md`

Before running benchmarks, confirm:
1. Benchmark methods are one operation and contain no manual inner loops.
2. Run configuration is comparable to the baseline profile.
3. You will record command + `Allocated/op` + `Mean` + `Gen0/Gen1` + decision.

## Baseline snapshot (before DS01)
- Date/time: 2026-02-19T17:13:03Z
- Commit/branch: `f0393875b5` on `dev/simonrozsival/label-investigation`
- Benchmark command(s): `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"`
- Artifacts path(s):
  - Baseline run log: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771521339142-18y033.txt`
- B0-B6 summary:
  - `B1_NewLabel`: `2928 B/op` (`~1.40-1.42 us`)
  - `B2_NewLabelWithBasicProperties`: `3648 B/op` (`~1.93-1.96 us`)
  - `B3_NewLabelWithFeatureToggle`: `2928-5024 B/op` (`~1.44-2.76 us`)
  - `B4_ResourceDictionaryStyleMaterialization`: `16571 B/op` (`~12.74-13.13 us`)
  - `B5_MockVisualTreeStartup`: `2.5 MB` (500) / `4.99 MB` (1000)
  - `B6_AppLikeStartupEmulation`: `3.91 MB` (500) / `7.79 MB` (1000)

## Iterative DS evaluations

### DS01 — `Dictionary<BindableProperty, BindablePropertyContext>`
- SQL todo id: `eval-ds01`
- Opportunity: Evaluate lower initial capacity / compact storage strategy
- Expected effect: Lower baseline dictionary + bucket footprint
- Risk: High
- Code diff summary: Changed `_properties` initialization from `new Dictionary<...>(4)` to default `new Dictionary<...>()`.
- Files changed: `src/Controls/src/Core/BindableObject.cs`
- Pre-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"`
- Post-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"`
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2928 B -> 2816 B` (all scenarios)
  - `B2`: `3648 B -> 3784 B` (all scenarios, regression)
  - `B3`: `-112 B` to `-360 B` depending on scenario
  - `B4`: `16571 B -> 16347 B`
  - Startup allocations: `B5/B6` increased (`+0.07 MB` to `+0.13 MB`)
- Decision: Keep
- Notes: Mixed result; retained provisionally in cumulative chain because it improves bare `new Label()` and style/feature paths, but revisit at final rollup due `B2` and startup allocation regressions.

### DS02 — `Dictionary<TriggerBase, SetterSpecificity>`
- SQL todo id: `eval-ds02`
- Opportunity: Lazy-init on first trigger usage
- Expected effect: Avoid dictionary alloc for trigger-free objects
- Risk: Low
- Code diff summary: Removed eager `_triggerSpecificity` initialization and created dictionary on first trigger attach.
- Files changed:
  - `src/Controls/src/Core/BindableObject.cs`
  - `src/Controls/src/Core/Interactivity/TriggerBase.cs`
- Pre-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771521893009-d32fws.txt`)
- Post-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771522634579-gxh60q.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2816 B -> 2736 B`
  - `B2`: `3784 B -> 3704 B`
  - `B3`: `-80 B` (`Effect/DynamicResource/StyleClass/Nav`) and `-160 B` (`GestureRecognizer`)
  - `B4`: `16347 B -> 16107 B`
  - Startup allocations improved vs DS01 (`B5: 2.57/5.12 MB -> 2.53/5.05 MB`, `B6: 3.97/7.92 MB -> 3.93/7.84 MB`)
- Decision: Keep
- Notes: Clear allocation win with low risk and straightforward lazy-init semantics.

### DS03 — `Dictionary<Size, SizeRequest>`
- SQL todo id: `eval-ds03`
- Opportunity: Lazy-init on first measure cache write
- Expected effect: Avoid cache alloc for simple/one-shot elements
- Risk: Medium
- Code diff summary: Switched `_measureCache` to lazy allocation and guarded reads/clears for null cache.
- Files changed: `src/Controls/src/Core/VisualElement/VisualElement.cs`
- Pre-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771522634579-gxh60q.txt`)
- Post-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771523317612-iunko4.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2736 B -> 2656 B`
  - `B2`: `3704 B -> 3624 B`
  - `B3`: `-80 B` across scenarios
  - `B4`: `16107 B -> 15947 B`
  - Startup allocations: `B5 2.53/5.05 MB -> 2.49/4.96 MB`, `B6 3.93/7.84 MB -> 3.90/7.77 MB`
- Decision: Keep
- Notes: Allocation wins are consistent; mean time shifts are noisy and should be interpreted with startup benchmark variance warnings.

### DS04 — `Dictionary<BindableProperty,(string,SetterSpecificity)>`
- SQL todo id: `eval-ds04`
- Opportunity: Ensure true on-demand path everywhere
- Expected effect: Reduce early dictionary creation
- Risk: Low
- Code diff summary: Tried to eliminate potential accidental `DynamicResources` dictionary materialization in remove/resources-changed paths.
- Files changed: `src/Controls/src/Core/Element/Element.cs` (reverted)
- Pre-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771523317612-iunko4.txt`)
- Post-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771523974274-d5uh7u.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - No meaningful allocation change in B1-B6 (`Allocated` unchanged across tracked rows).
  - Mean shifts were noise-level and mixed.
- Decision: Revert
- Notes: Kept baseline behavior because attempted change did not move memory metrics.

### DS05 — `Dictionary<BindableProperty,(string,SetterSpecificity)>`
- SQL todo id: `eval-ds05`
- Opportunity: Delay creation until first implicit/class style mutation
- Expected effect: Reduce style dictionary overhead in unstyled cases
- Risk: Medium
- Code diff summary: Reduced merged-style bookkeeping allocation pressure by pre-sizing backing lists (`_implicitStyles` and `_classStyleProperties`) instead of default growth.
- Files changed: `src/Controls/src/Core/MergedStyle.cs`
- Pre-change benchmark command: `dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre snapshot from DS03 kept state)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771530209142-zhqg8z.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2656 B -> 2632 B`
  - `B2`: `3624 B -> 3600 B`
  - `B3`: `-24 B` (`Gesture/Effect/DynamicResource/NavigationTouch`) and `-48 B` (`StyleClass`)
  - `B4`: `15947 B -> 15915 B`
  - Startup allocations improved slightly (`B5 2.49/4.96 MB -> 2.48/4.94 MB`, `B6 3.90/7.77 MB -> 3.88/7.75 MB`)
- Decision: Keep
- Notes: Small but consistent allocation reductions across all benchmark tiers with no behavioral changes.

### DS06 — `HashSet<string>`
- SQL todo id: `eval-ds06`
- Opportunity: Strict lazy-init path only when handler update batching starts
- Expected effect: Avoid set alloc for elements without pending updates
- Risk: Low
- Code diff summary: Made `_pendingHandlerUpdatesFromBPSet` lazy (`null` until needed) and added null-safe access/removal in property-change pipeline.
- Files changed: `src/Controls/src/Core/Element/Element.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771530209142-zhqg8z.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771530903149-mkhf43.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: unchanged (`2632 B`)
  - `B2`: unchanged (`3600 B`)
  - `B3`: `GestureRecognizer 4400 B -> 4336 B`; other scenarios unchanged
  - `B4`: `15915 B -> 15851 B`
  - Startup allocations unchanged (`B5 2.48/4.94 MB`, `B6 3.88/7.75 MB`)
- Decision: Keep
- Notes: Net win is small and concentrated in style-heavy/gesture paths, but there were no allocation regressions.

### DS07 — `Int32[]` bucket arrays
- SQL todo id: `eval-ds07`
- Opportunity: Reduce initial capacities where safe
- Expected effect: Smaller initial bucket arrays
- Risk: Medium
- Code diff summary: Evaluated lowering explicit dictionary initial capacity in merged-resource path (`ResourcesExtensions`) from `8` to default initialization.
- Files changed: `src/Controls/src/Core/ResourcesExtensions.cs` (reverted)
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771530903149-mkhf43.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771531543961-o6njev.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - No allocation changes in B1-B6 (`Allocated` unchanged across data-structure and startup scenarios).
- Decision: Revert
- Notes: Candidate did not move memory metrics in target scenarios, so original capacity hint was restored.

### DS08 — `Entry<TKey,TValue>[]` arrays
- SQL todo id: `eval-ds08`
- Opportunity: Delay growth; tune first-resize thresholds
- Expected effect: Fewer entry-array allocations
- Risk: Medium
- Code diff summary: No safe MAUI-layer change identified; dictionary entry-array growth behavior is primarily runtime-controlled unless we replace dictionaries with custom storage.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Rework
- Notes: Revisit only if we introduce custom small-map structures for hot paths (covered by later DS09/DS12/DS13/DS14 opportunities).

### DS09 — `SetterSpecificityList<object>`
- SQL todo id: `eval-ds09`
- Opportunity: Lazy-init when non-default value path is used
- Expected effect: Avoid list alloc on untouched/default properties
- Risk: Low
- Code diff summary: Tested reducing `BindablePropertyContext.Values` preallocated capacity from `3` to `1`.
- Files changed: `src/Controls/src/Core/BindableObject.cs` (reverted)
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771531543961-o6njev.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771532224781-crq6fy.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2632 B -> 2712 B` (regression)
  - `B2`: `3600 B -> 3920 B` (regression)
  - `B4`: `15851 B -> 16731 B` (regression)
  - Startup allocations regressed: `B5 2.48/4.94 MB -> 2.71/5.40 MB`, `B6 3.88/7.75 MB -> 4.34/8.66 MB`
- Decision: Revert
- Notes: Lowering initial capacity forced extra growth/copy behavior and caused broad allocation regressions.

### DS10 — `SetterSpecificityList<BindingBase>`
- SQL todo id: `eval-ds10`
- Opportunity: Lazy-init only when bindings are attached
- Expected effect: Avoid bindings list alloc for non-bound properties
- Risk: Low
- Code diff summary: Made `BindablePropertyContext.Bindings` nullable/lazy and materialized it only when a binding is actually set; updated all binding access sites to be null-safe.
- Files changed:
  - `src/Controls/src/Core/BindableObject.cs`
  - `src/Controls/src/Core/BindableObjectExtensions.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771531543961-o6njev.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771533523332-4qxd84.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2632 B -> 2592 B`
  - `B2`: `3600 B -> 3440 B`
  - `B3`: `-80 B` (`Gesture/Effect/DynamicResource/StyleClass`) and `-40 B` (`NavigationTouch`)
  - `B4`: `15851 B -> 15411 B`
  - Startup allocations improved: `B5 2.48/4.94 MB -> 2.36/4.71 MB`, `B6 3.88/7.75 MB -> 3.65/7.29 MB`
- Decision: Keep
- Notes: This is the strongest win so far and aligns directly with DS10 intent.

### DS11 — `Queue<SetValueArgs>`-style delayed setters
- SQL todo id: `eval-ds11`
- Opportunity: Lazy-init only during batched set scenarios
- Expected effect: Avoid queue alloc for non-batched paths
- Risk: Low
- Code diff summary: Audited existing `SetValueCore` implementation and confirmed delayed-setter queue is already allocated lazily only when `IsBeingSet` is re-entered.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: DS11 intent is already met in current code; no additional safe reduction found.

### DS12 — `Object[]` backing arrays
- SQL todo id: `eval-ds12`
- Opportunity: Tighten initial capacity / delayed allocate backing
- Expected effect: Fewer backing-array allocations
- Risk: Low
- Code diff summary: Evaluated `SetterSpecificityList<object>` backing strategy and deferred additional changes because DS09 capacity reduction experiment showed broad allocation regressions.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Rework
- Notes: Requires a larger redesign (single-entry inline storage or custom small-buffer list) rather than simple capacity tweaks.

### DS13 — `BindableProperty[]` backing arrays
- SQL todo id: `eval-ds13`
- Opportunity: Delay keys array creation until second insertion
- Expected effect: Avoid arrays for singleton/default cases
- Risk: Low
- Code diff summary: No isolated low-risk patch applied; DS13 depends on introducing inline/single-entry storage in `SetterSpecificityList<T>`.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Rework
- Notes: Bundle with DS12/DS14 in a dedicated small-collection redesign to avoid repeating DS09-style regressions.

### DS14 — `SetterSpecificity[]` backing arrays
- SQL todo id: `eval-ds14`
- Opportunity: Delay/init with smaller footprint
- Expected effect: Reduce small-array overhead
- Risk: Low
- Code diff summary: No isolated change applied; specificity-key backing shape is coupled with DS12/DS13 object/value/key storage strategy.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Rework
- Notes: Treat DS12-DS14 as one refactor unit (inline first slot + deferred arrays) to get deterministic wins.

### DS15 — `List<BindableProperty>`
- SQL todo id: `eval-ds15`
- Opportunity: Delay list creation until first implicit style registration
- Expected effect: Avoid list alloc in style-light trees
- Risk: Medium
- Code diff summary: No change applied; implicit style registration is currently required at `MergedStyle` construction to preserve dynamic resource behavior and implicit-style updates.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Rework
- Notes: Requires redesign of implicit-style propagation hooks before deferring `_implicitStyles` allocation safely.

### DS16 — `IList<BindableProperty>` / `IList<Style>`
- SQL todo id: `eval-ds16`
- Opportunity: Allocate only when `StyleClass` is actually set
- Expected effect: Avoid class-style list allocations
- Risk: Low
- Code diff summary: Confirmed style-class tracking lists are already on-demand; DS05 also reduced style-class list overhead via pre-sizing when style classes are present.
- Files changed: none (incremental behavior already captured in DS05)
- Pre-change benchmark command: n/a (no additional code change)
- Post-change benchmark command: n/a (no additional code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: Current implementation already allocates class-style structures only when `StyleClass` is assigned.

### DS17 — `IList<Element>` / `List<Element>`
- SQL todo id: `eval-ds17`
- Opportunity: Keep null or shared empty until first child add
- Expected effect: Save child-list alloc for leaf elements
- Risk: Low
- Code diff summary: Verified `_internalChildren` is already nullable and created on-demand (`??=`) on first logical-child mutation.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: DS17 optimization is already present in the current implementation.

### DS18 — `List<Action<object,ResourcesChangedEventArgs>>`
- SQL todo id: `eval-ds18`
- Opportunity: Single-subscriber fast path + lazy list promotion
- Expected effect: Avoid list alloc in 0/1-handler cases
- Risk: Low
- Code diff summary: Switched resources-changed listener storage to a single-field fast path (`object _changeHandlers`) with promotion to `List<Action<...>>` only when a second subscriber is added.
- Files changed: `src/Controls/src/Core/Element/Element.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771533523332-4qxd84.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771534850772-7anyui.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: unchanged (`2592 B`)
  - `B2`: unchanged (`3440 B`)
  - `B3`: `GestureRecognizer 4256 B -> 4184 B`; other scenarios unchanged
  - `B4`: `15411 B -> 15203 B`
  - Startup allocations effectively unchanged (`B5: 2.36/4.71 MB`, `B6: 3.65/~7.29 MB`)
- Decision: Keep
- Notes: Preserves baseline constructor footprint while reducing style/resource-heavy allocation paths.

### DS19 — `IList<BindableObject>`
- SQL todo id: `eval-ds19`
- Opportunity: Strict lazy-init and avoid pre-creation
- Expected effect: Save list alloc when no bindable resources
- Risk: Low
- Code diff summary: Deferred `_bindableResources` list allocation in `Element.OnResourcesChanged` until a bindable resource is actually discovered.
- Files changed: `src/Controls/src/Core/Element/Element.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771534850772-7anyui.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771535469684-9h8z9u.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1/B2/B3`: allocation-neutral in tracked scenarios.
  - `B4`: `15203 B -> 15139 B`
  - Startup allocations: essentially unchanged to slightly lower (`B6 500/1000: 3.65/7.30 MB -> 3.64/7.26 MB`)
- Decision: Keep
- Notes: Small but consistent allocation improvement with no observed regressions.

### DS20 — `TrackableCollection<Effect>`
- SQL todo id: `eval-ds20`
- Opportunity: Lazy-create only on first effects access
- Expected effect: Save collection alloc for effect-free elements
- Risk: Low
- Code diff summary: Confirmed `Element.Effects` already lazily creates `TrackableCollection<Effect>` on first access.
- Files changed: none
- Pre-change benchmark command: n/a (no code change)
- Post-change benchmark command: n/a (no code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: DS20 optimization is already implemented in current code.

### DS21 — `ObservableCollection<IGestureRecognizer>`
- SQL todo id: `eval-ds21`
- Opportunity: Lazy-init on first gesture registration
- Expected effect: Remove always-paid collection cost on gesture-free views
- Risk: Medium
- Code diff summary: Made `View._gestureRecognizers` lazy and moved collection-change wiring to first-access creation, while avoiding accidental eager creation in `OnBindingContextChanged`.
- Files changed: `src/Controls/src/Core/View/View.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771535469684-9h8z9u.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771536166489-1o5jvm.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2592 B -> 2440 B`
  - `B2`: `3440 B -> 3288 B`
  - `B3`: `GestureRecognizer` unchanged; all other scenarios `-152 B`
  - `B4`: `15139 B -> 14987 B`
  - Startup allocations improved: `B5 2.36/4.71 MB -> 2.29/4.56 MB`, `B6 3.64/7.26 MB -> 3.57/7.12 MB`
- Decision: Keep
- Notes: Large, broad allocation win with preserved behavior in benchmark scenarios.

### DS22 — `List<IGestureRecognizer>`
- SQL todo id: `eval-ds22`
- Opportunity: Delay internal list creation via lazy outer collection
- Expected effect: Avoid nested list alloc when unused
- Risk: Medium
- Code diff summary: Covered by DS21; lazy `ObservableCollection` creation also defers the internal list allocation.
- Files changed: none (covered by DS21 `View.cs` change)
- Pre-change benchmark command: n/a (same run pair as DS21)
- Post-change benchmark command: n/a (same run pair as DS21)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): included in DS21 deltas above
- Decision: Keep
- Notes: No additional patch required after DS21.

### DS23 — `ObservableCollection<IGestureRecognizer>`
- SQL todo id: `eval-ds23`
- Opportunity: Maintain strict on-demand path
- Expected effect: Prevent accidental eager creation
- Risk: Low
- Code diff summary: Verified strict on-demand behavior after DS21; `CompositeGestureRecognizers` remains lazy and no additional eager touch points were found.
- Files changed: none (validated existing + DS21 behavior)
- Pre-change benchmark command: n/a (no new code change)
- Post-change benchmark command: n/a (no new code change)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: DS23 intent is satisfied by current implementation plus DS21 updates.

### DS24 — `Lazy<List<Page>>`
- SQL todo id: `eval-ds24`
- Opportunity: Replace wrapper with nullable list + manual lazy init
- Expected effect: Remove `Lazy` + `LazyHelper` overhead
- Risk: Medium
- Code diff summary: Replaced `NavigationProxy._pushStack` from `Lazy<List<Page>>` to nullable `List<Page>` with `GetOrCreatePushStack()` and null-safe call sites.
- Files changed: `src/Controls/src/Core/NavigationProxy.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771536166489-1o5jvm.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771536934434-1s11bs.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2440 B -> 2368 B` (`-72 B`)
  - `B2`: `3288 B -> 3216 B` (`-72 B`)
  - `B3`: all scenarios `-72 B`
  - `B4`: `14987 B -> 14843 B` (`-144 B`)
  - Startup allocations improved: `B5 2.29/4.56 MB -> 2.26/4.50 MB`, `B6 3.57/7.12 MB -> 3.53/7.04 MB`
- Decision: Keep
- Notes: Strong, consistent reduction from removing `Lazy<T>` wrapper overhead on navigation stack path.

### DS25 — `Lazy<NavigatingStepRequestList>`
- SQL todo id: `eval-ds25`
- Opportunity: Replace wrapper with nullable field + manual lazy init
- Expected effect: Remove `Lazy` + `LazyHelper` overhead
- Risk: Medium
- Code diff summary: Replaced `NavigationProxy._modalStack` from `Lazy<NavigatingStepRequestList>` to nullable `NavigatingStepRequestList` with `GetOrCreateModalStack()` and null-safe paths.
- Files changed: `src/Controls/src/Core/NavigationProxy.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771536934434-1s11bs.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771537614205-8bpfqj.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2368 B -> 2296 B` (`-72 B`)
  - `B2`: `3216 B -> 3144 B` (`-72 B`)
  - `B3`: all scenarios `-72 B`
  - `B4`: `14843 B -> 14699 B` (`-144 B`)
  - Startup allocations improved: `B5 2.26/4.50 MB -> 2.22/4.43 MB`, `B6 3.53/7.04 MB -> 3.50/6.97 MB`
- Decision: Keep
- Notes: Mirrors DS24 gains; confirms `Lazy<T>` wrappers in this path had measurable per-instance overhead.

### DS26 — `Lazy<T>` wrappers (general)
- SQL todo id: `eval-ds26`
- Opportunity: Convert to nullable field + `??=` in single-threaded UI paths
- Expected effect: Remove wrapper/helper/delegate objects
- Risk: Medium
- Code diff summary: Audited base-chain/shared path and confirmed `NavigationProxy` wrappers were the remaining `Lazy<T>` cases; addressed by DS24 + DS25.
- Files changed: none (covered by DS24/DS25)
- Pre-change benchmark command: n/a (no additional patch)
- Post-change benchmark command: n/a (no additional patch)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): n/a
- Decision: Keep
- Notes: DS26 objective is satisfied in current base-chain scope.

### DS27 — `LazyHelper` internal objects
- SQL todo id: `eval-ds27`
- Opportunity: Eliminated indirectly by DS24-DS26
- Expected effect: Fewer helper allocations per instance
- Risk: Medium
- Code diff summary: `LazyHelper` allocations in `NavigationProxy` were removed indirectly by DS24 + DS25 (no `Lazy<T>` wrappers remain there).
- Files changed: none (covered by DS24/DS25)
- Pre-change benchmark command: n/a (covered by DS24/DS25 run pair)
- Post-change benchmark command: n/a (covered by DS24/DS25 run pair)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): included in DS24/DS25 deltas
- Decision: Keep
- Notes: No additional code required after DS24/DS25.

### DS28 — `Func<T>` factory delegates
- SQL todo id: `eval-ds28`
- Opportunity: Replace with direct initializer methods and null checks
- Expected effect: Remove delegate allocations
- Risk: Low
- Code diff summary: Removed `Lazy<T>` factory delegates (`() => new List<Page>()`, `() => new NavigatingStepRequestList()`) in `NavigationProxy` via DS24 + DS25.
- Files changed: none (covered by DS24/DS25)
- Pre-change benchmark command: n/a (covered by DS24/DS25 run pair)
- Post-change benchmark command: n/a (covered by DS24/DS25 run pair)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1): included in DS24/DS25 deltas
- Decision: Keep
- Notes: Delegate-removal objective already achieved in evaluated path.

### DS29 — Delegate objects (`EventHandler*`)
- SQL todo id: `eval-ds29`
- Opportunity: Defer subscriptions where safe; static delegate caching
- Expected effect: Fewer per-instance delegate allocations
- Risk: Medium
- Code diff summary: Tried caching `MergedStyle` property-changed delegates in instance fields and reusing them for `BindableProperty.Create` calls; then reverted after regression.
- Files changed: `src/Controls/src/Core/MergedStyle.cs` (attempt + revert)
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771537614205-8bpfqj.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (attempt: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771538315911-ostjid.txt`; revert validation: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771539041942-3byw1e.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - Attempt regressed allocations: `B1 2296 B -> 2376 B`, `B2 3144 B -> 3224 B`, `B3 +80 B`, `B4 14699 B -> 14731 B`
  - Revert restored DS25-level allocations (`B1=2296 B`, `B2=3144 B`, `B4=14699 B`)
- Decision: Revert
- Notes: Added per-instance delegate fields increased object baggage more than callback reuse helped.

### DS30 — `WeakReference` / `WeakReference<Element>`
- SQL todo id: `eval-ds30`
- Opportunity: Audit for nullable direct refs + explicit lifecycle points
- Expected effect: Reduce weak-ref object churn
- Risk: Medium
- Code diff summary: Replaced `Element._realParent` weak reference storage with a direct nullable `Element` reference in the parent tracking path.
- Files changed: `src/Controls/src/Core/Element/Element.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771539041942-3byw1e.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771539736823-juf467.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1/B2`: unchanged (`2296 B` / `3144 B`)
  - `B3`: `GestureRecognizer 4040 B -> 4016 B` (`-24 B`), other scenarios unchanged
  - `B4`: `14699 B -> 14675 B` (`-24 B`) across scenarios
  - Startup allocations improved: `B5 2.22/4.43 MB -> 2.21/4.40 MB`, `B6 3.50/6.97 MB -> 3.49/6.95 MB`
- Decision: Keep
- Notes: Small but consistent win in parented scenarios; requires follow-up lifecycle validation for retention semantics.

### DS31 — Weak proxy wrappers
- SQL todo id: `eval-ds31`
- Opportunity: Ensure strict on-demand creation and reuse patterns
- Expected effect: Avoid proxy alloc on feature-unused elements
- Risk: Low
- Code diff summary: Tried converting `Element._parentOverride` from weak reference storage to direct reference; reverted after no measurable allocation gain in benchmark matrix.
- Files changed: `src/Controls/src/Core/Element/Element.cs` (attempt + revert)
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771539736823-juf467.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (attempt: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771540394729-e53t5g.txt`; revert validation: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771540975071-p62yr3.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - Attempt: no allocation change vs pre (`B1/B2/B3/B4/B5/B6 allocations unchanged`)
  - Revert: preserved DS30-level allocations (`B3 Gesture=4016 B`, `B4=14675 B`, `B5 2.21/4.40 MB`, `B6 3.49/6.95 MB`)
- Decision: Revert
- Notes: No measured upside; avoid extra lifecycle-retention risk for no gain.

### DS32 — Boolean flag fields
- SQL todo id: `eval-ds32`
- Opportunity: Bit-pack to a compact flags field
- Expected effect: Reduce object size + padding waste
- Risk: Medium
- Code diff summary: Replaced multiple private `VisualElement` boolean fields with a single bit-packed flags enum plus property-backed accessors.
- Files changed: `src/Controls/src/Core/VisualElement/VisualElement.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771540975071-p62yr3.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771541653550-czbgq6.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2296 B -> 2288 B` (`-8 B`)
  - `B2`: `3144 B -> 3136 B` (`-8 B`)
  - `B3`: all scenarios `-8 B`
  - `B4`: `14675 B -> 14659 B` (`-16 B`)
  - Startup allocations improved: `B6 3.49/6.95 MB -> 3.48/6.94 MB` (B5 unchanged to two decimals)
- Decision: Keep
- Notes: Clean, broad per-instance reduction consistent with object-size shrink.

### DS33 — Small scalar state (`ushort`, nullable ids)
- SQL todo id: `eval-ds33`
- Opportunity: Pack/co-locate infrequent state with flags or deferred generation
- Expected effect: Minor but broad per-instance savings
- Risk: Low
- Code diff summary: Replaced `Element._id` from `Guid?` with direct `Guid` plus `Guid.Empty` sentinel lazy initialization.
- Files changed: `src/Controls/src/Core/Element/Element.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771541653550-czbgq6.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771542258034-57mzbc.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2288 B -> 2280 B` (`-8 B`)
  - `B2`: `3136 B -> 3128 B` (`-8 B`)
  - `B3`: all scenarios `-8 B`
  - `B4`: `14659 B -> 14635 B` (`-24 B`)
  - Startup allocations improved: `B5 2.21/4.40 MB -> 2.20/4.39 MB`, `B6 3.48/6.94 MB -> 3.48/6.94 MB` (small but positive)
- Decision: Keep
- Notes: Good low-risk scalar packing win; sentinel approach avoids nullable overhead on hot object graph.

### DS34 — Struct-heavy state slots
- SQL todo id: `eval-ds34`
- Opportunity: Verify necessity of always-live slots vs deferred state blocks
- Expected effect: Potential object-size reduction
- Risk: High
- Code diff summary: Replaced always-live mock-bounds scalar fields in `VisualElement` (`_mockX/_mockY/_mockWidth/_mockHeight`) with deferred `MockBoundsState` reference allocated only when mock bounds are used.
- Files changed: `src/Controls/src/Core/VisualElement/VisualElement.cs`
- Pre-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (pre: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771542258034-57mzbc.txt`)
- Post-change benchmark command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (post: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771542916373-5uki9g.txt`)
- Metrics delta (Allocated/op, Mean, Gen0/Gen1):
  - `B1`: `2280 B -> 2256 B` (`-24 B`)
  - `B2`: `3128 B -> 3104 B` (`-24 B`)
  - `B3`: all scenarios `-24 B`
  - `B4`: `14635 B -> 14587 B` (`-48 B`)
  - Startup allocations improved: `B5 2.20/4.39 MB -> 2.19/4.37 MB`, `B6 3.48/6.94 MB -> 3.47/6.91 MB`
- Decision: Keep
- Notes: Biggest late-stage object-size win; deferred state block appears behavior-safe in current benchmark coverage.

## Final cumulative rollup
- Final benchmark command(s): `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -f net11.0 -p:TreatWarningsAsErrors=false -- --filter "*BaseChain*"` (baseline: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771526120713-6vqv7q.txt`, final: `/var/folders/rs/_8rmhb_n3l1f_xx66kpjf42r0000gn/T/copilot-tool-output-1771542916373-5uki9g.txt`)
- Baseline vs final summary (B0-B6):
  - `B1_NewLabel`: `2928 B/op -> 2256 B/op`
  - `B2_NewLabelWithBasicProperties`: `3648 B/op -> 3104 B/op`
  - `B3_NewLabelWithFeatureToggle`: `2928-5024 B/op -> 2256-3968 B/op` (scenario-dependent)
  - `B4_ResourceDictionaryStyleMaterialization`: `16571 B/op -> 14587 B/op`
  - `B5_MockVisualTreeStartup`: `2.50/4.99 MB -> 2.19/4.37 MB` (500/1000 controls)
  - `B6_AppLikeStartupEmulation`: `3.91/7.79 MB -> 3.47/6.91 MB` (500/1000 controls)
- Net allocation delta (% and bytes/op):
  - `B1`: `-672 B/op` (`-22.95%`)
  - `B2`: `-544 B/op` (`-14.91%`)
  - `B4`: `-1984 B/op` (`-11.97%`)
  - `B3` range: `-672 B/op` to `-1056 B/op` (`~21-23%`)
- Net startup emulation delta (B6):
  - 500 controls: `-0.44 MB` (`-11.25%`)
  - 1000 controls: `-0.88 MB` (`-11.30%`)
- DS changes kept: `DS01`, `DS02`, `DS03`, `DS05`, `DS06`, `DS10`, `DS18`, `DS19`, `DS21`, `DS22`, `DS23`, `DS24`, `DS25`, `DS26`, `DS27`, `DS28`, `DS30`, `DS32`, `DS33`, `DS34`.
- DS changes reverted: `DS04`, `DS07`, `DS09`, `DS29`, `DS31`.
- Follow-up recommendations:
  1. Add targeted lifecycle/leak tests around parent/reference changes (`DS30`) before production merge.
  2. Consider extending the `Lazy<T> -> nullable + manual init` pattern to other high-frequency shared paths after targeted benchmark gating.
  3. Keep the benchmark matrix unchanged for any follow-up tweak to preserve comparability.

## Post-rollup validation

### XAML SG inflation benchmark (AFTER vs BEFORE)
- AFTER command: `cd /Users/simonrozsival/Projects/dotnet/maui/label-investigation && dotnet run --project src/Controls/tests/Xaml.Benchmarks/Microsoft.Maui.Controls.Xaml.Benchmarks.csproj -c Release -p:TreatWarningsAsErrors=false -- --filter "*LayoutBenchmark*"`
- BEFORE command: `cd /Users/simonrozsival/Projects/dotnet/maui/net11.0 && dotnet run --project src/Controls/tests/Xaml.Benchmarks/Microsoft.Maui.Controls.Xaml.Benchmarks.csproj -c Release -p:TreatWarningsAsErrors=false -p:NoWarn=CA2252 -- --filter "*LayoutBenchmark*"`
- Artifact inputs:
  - AFTER: `label-investigation/BenchmarkDotNet.Artifacts/results/Microsoft.Maui.Controls.Xaml.Benchmarks.LayoutBenchmark-report.csv`
  - BEFORE: `net11.0/BenchmarkDotNet.Artifacts/results/Microsoft.Maui.Controls.Xaml.Benchmarks.LayoutBenchmark-report.csv`
- Comparison summary:
  - `XamlC`: `298.5 us, 345.1 KB` -> `306.9 us, 338.17 KB` (`+2.81%` mean, `-2.01%` allocated)
  - `SourceGen`: `157.2 us, 167.57 KB` -> `157.9 us, 161.37 KB` (`+0.45%` mean, `-3.70%` allocated)
  - `Runtime`: `26,514.5 us, 24085.14 KB` -> `25,945.9 us, 24191.82 KB` (`-2.14%` mean, `+0.44%` allocated)
- Notes:
  - Relative ranking is unchanged (`SourceGen` remains substantially faster/lower-alloc than `XamlC`; `Runtime` remains far slower and much higher allocation).
  - The base-chain control optimizations do not materially shift XAML SG throughput; observed SG changes are small and mixed.
