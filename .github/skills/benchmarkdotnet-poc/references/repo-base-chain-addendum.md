# Repo Addendum: Base-chain DS PoC Benchmark Loop

This addendum is specific to this repository's PoC from `issue-draft.md`.

## Benchmark ladder (required)
- **B0**: harness floor (`new object()`).
- **B1**: `new Label()` constructor baseline.
- **B2**: `new Label()` + minimal properties.
- **B3**: one-feature toggles (gesture/effect/dynamic-resource/style-class/navigation touch).
- **B4**: style materialization via app `ResourceDictionary`.
- **B5**: mock visual-tree startup.
- **B6**: app-like startup emulation (`Application` + `Window` + `Page` + styles).

## DS evaluation loop (required)
For each DS todo (`eval-ds01` ... `eval-ds34`):
1. Run pre-change targeted benchmark subset.
2. Implement DS change.
3. Run post-change targeted subset.
4. Run full B0-B6 matrix.
5. Append results to `poc-evaluation-log.md`.
6. Decide Keep/Rework/Revert before proceeding.

## Canonical commands (example)
```bash
# Build
dotnet build src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release

# Fast targeted check
dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -- --filter "*BaseChain*" --memory true

# Full matrix snapshot
dotnet run --project src/Core/tests/Benchmarks/Core.Benchmarks.csproj -c Release -- --filter "*BaseChain*|*Startup*|*Resource*" --memory true
```

## Logging requirements
Each DS entry in `poc-evaluation-log.md` must include:
- code diff summary
- files changed
- pre/post benchmark commands
- `Allocated/op`, `Mean`, `Gen0/Gen1` deltas
- keep/rework/revert decision
