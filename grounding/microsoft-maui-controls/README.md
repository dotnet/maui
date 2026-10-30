# Controls upgrade evaluation consumers

This directory is the **measurement instrument**, not a shipping skill. The
Controls package includes only its `skills/` Markdown. Never pack this directory,
copy evaluator code into an agent's inputs, or use a successful control run as a
model success.

## What is implemented

Three pinned, public `dotnet/maui-samples` consumers span a simple binding
workflow, independent Shell tab state, and a larger multi-project employee
directory with local data and favorites persistence. `suite.json` records the
exact upstream commit, archive checksum, project paths, prompts and executable
UI checkpoints. These are **three readiness cases, not CT-24**, a representative
cross-platform sample or an established skills benefit.
The binding checks cover two gallery workflows, not every page. Its pinned
menu contains two identical "Enable Buttons" buttons linking to the same page;
the evaluator declares their count and the selected index instead of silently
choosing an ambiguous match. This suite does not replace the historical
customized-Shell pilot's separate evaluator or pool its results.

The source controls are deliberately adapted to SDK `10.0.301` and Controls
`10.0.110`. Target controls use SDK `11.0.100-rc.1.26425.128` and Controls
`11.0.0-rc.1.26451.6`. These installed, historically verified coordinates are not
a claim about the latest or final MAUI 11 release. Both stages are Android-only,
minimum API 24, standalone APKs; builds target `android-arm64` for the evaluator's
emulator. Logging remains explicitly pinned to `10.0.0` in both stages so this
does not silently become an unrelated dependency-upgrade experiment.

`stage.py` preserves original file hashes and records its setup adaptations.
The source control is therefore **not** an untouched upstream build. It removes
framework/version/minimum declarations from the UI project and writes the pinned
Android settings; the employee Core project also gets an explicit managed TFM.
The prompts' existing setup means this staged configuration. Source and target
reference controls are created mechanically; neither is an agent delivery.
Upstream sample code, notices and license files are downloaded to the chosen
external volume, not vendored into MAUI. EmployeeDirectory contains Apache-2.0
file notices in addition to the repository's MIT license; retain both. Existing
remote photos are not an offline-image or network-independence guarantee.

## Run controls on a separate volume

Use a fresh absolute directory under `/Volumes/NieuwVolume` for `ROOT`. No command
below starts a coding model or provisions an SDK/workload. Installed SDKs,
Android SDK and JDK are prerequisites. The runner is verified on macOS with
Python 3.9+ and an Android arm64 emulator, not other build-host/device combinations.

```sh
ROOT=/Volumes/NieuwVolume/my-maui-skills-eval
python3 grounding/microsoft-maui-controls/stage.py --root "$ROOT" --case binding --mode source
python3 grounding/microsoft-maui-controls/stage.py --root "$ROOT" --case binding --mode target
python3 grounding/microsoft-maui-controls/controls.py --root "$ROOT" --case binding --mode source --serial emulator-5584
python3 grounding/microsoft-maui-controls/controls.py --root "$ROOT" --case binding --mode target --serial emulator-5584
```

Repeat for `shell-tabs` and `employee-directory`. Supply only a serial belonging
to an emulator you own: each run removes an existing installation of that exact
sample application ID, installs the APK and clears the sample's data. This
avoids signature conflicts between isolated debug-signing caches. Application
IDs must match the source case; unrelated packages are not removed. The script
does not select, start, reset or dismiss warnings on a device.
Omitting `--serial` is explicitly **build-only**. Existing stages and result
directories are never overwritten; `--run-id` selects a fresh evidence suffix.
After all six full controls complete with the same harness and run ID, recheck
the saved APK, input hashes, command outputs, UI assertions and native evidence:

```sh
python3 grounding/microsoft-maui-controls/report.py --root "$ROOT" --run-id 01
```

This writes `results/readiness-01.json`; it refuses incomplete, stale-harness or
build-only results and never treats them as model attempts.

Downloads, consumer projects, `bin/obj`, NuGet caches, temporary files, private
.NET/XDG state, debug-signing material and logs stay under `ROOT`. Shared SDKs
are not made writable. This controls runner is **not an agent sandbox**; real
model trials need a separately verified app-only filesystem/tool boundary.

Each control records actual SDK selection, configuration-specific restore graph,
evaluated properties, Debug/Release commands, signed APK hash/identity/minimum,
launch result, named raw UI XMLs and native activity dump. Every UI checkpoint
requires positive app-owned observations; converter, counter and persistence
checks inspect actual behavior rather than the agent's explanation.
`build-pass-runtime-pending` is not `control-pass`. The exact native class check
in `shell-tabs` identifies the **unmodified reference control** only.
`candidate.py` instead checks live framework ancestry and accepts subclasses;
the unmodified reference's class name is not a universal solution template.

The runner is intentionally limited to evaluator-owned controls: input drift
is rejected, and `agentAttempt` is always false. It cannot grade a model's
edited project or produce a skill-effectiveness result.

### Local readiness result

The pinned Android arm64 source and target controls completed Debug and Release
builds, APK identity/minimum checks, and actual emulator navigation on
2026-10-16. Saved evidence was regraded with the same suite and controls hashes.

| Consumer | Controls 10 source | Controls 11 RC1 target | UI checkpoints per control |
| --- | --- | --- | --- |
| DataBindingDemos | Pass | Pass | 8 |
| ShellTabBarSample | Pass | Pass | 11 |
| EmployeeDirectory + Core | Pass | Pass | 7 |

These are six **reference controls**, not six model upgrades. A diagnostic copy
of real passing binding evidence with a required UI label changed was correctly
rejected despite its copied pass verdict and successful command rows. It is a
negative evidence replay, not another app execution or model attempt.
Earlier failed selector/protocol preflights are preserved separately.

The local summary is `results/readiness-runtime-02.json` under the external run
root. Those six controls predate the candidate adapter and later harness
changes. Their exact four-file harness is preserved at
`reference-harness-v1/` under that root; recheck them with its `report.py`, not
the current checkout's changed harness. Fresh controls use the current commands
above. No raw APKs, downloaded samples, caches or result logs are committed.
Stored preflight stages may predate prompt edits (the EmployeeDirectory
source prompt does); **freshly stage and freeze matched inputs before any agent
trial**. They must not be recycled into an efficacy score.

```sh
TMPDIR="$ROOT/tmp" PYTHONDONTWRITEBYTECODE=1 \
  python3 -m unittest discover -s grounding/microsoft-maui-controls -p 'test_*.py' -v
```

## Local macOS build preflight

`filesystem_sandbox.py` runs a command through macOS `sandbox-exec` with file
reads and writes denied by default. Only the staged `app/` and separate private
state are writable. The three exact sibling configuration files, explicitly
supplied public toolchains and narrow OS runtime paths are readable. Directory
metadata grants are not recursive content grants. Rosetta gets its exact public
runtime file, not all of `/Library`; .NET IPC uses the launcher's owned session
directories and socket-only numeric MSBuild endpoints, not writable shared temp.

Prepare private .NET, NuGet, XDG, signing and temporary directories on the
external volume first. Set `DOTNET_CLI_HOME`, `NUGET_PACKAGES`,
`NUGET_HTTP_CACHE_PATH`, `NUGET_PLUGINS_CACHE_PATH`, `XDG_DATA_HOME`, `TMPDIR`
and `AppSettingsDirectory` inside that private root. Use an explicitly prepared
**public-package-only cache**, not a user credential/configuration directory.
Disable shared compilation and MSBuild node reuse. Set `JavaSdkDirectory` and
`AndroidSdkDirectory` to the same public roots passed below: implicit JDK
discovery otherwise enumerates host directories outside the boundary.

For an existing staged consumer and prepared environment:

```sh
python3 grounding/microsoft-maui-controls/filesystem_sandbox.py \
  --app "$APP" --private "$PRIVATE" \
  --read-root /usr/local/share/dotnet \
  --read-root "$ANDROID_HOME" --read-root "$JAVA_HOME" \
  -- dotnet restore DataBindingDemos.csproj --configfile ../NuGet.config \
    -p:RuntimeIdentifier=android-arm64 -p:Configuration=Debug
```

Run the matching Debug build with `--no-restore`, then configuration-specific
Release restore/build through the same launcher. Neither model inputs nor
project code need evaluator imports. Use fresh result names and retain the
exact commands, filesystem-policy source hash and stdout/stderr of failures.
The regression suite exercises real processes: teacher/evaluator and APFS-alias
reads, symlink escape, protected writes and descendants starting a new session
are rejected; a permitted Unix socket does not permit an ordinary file with
the same name pattern.

The local binding source and target preflights passed SDK selection, NuGet cache
inspection and configuration-specific restore plus nonincremental Debug/Release
builds through frozen copies of this launcher: 12 successful commands and four
signed APKs. Original staged input hashes remained unchanged. The administrative
`stage.json` and additional target-SDK DeveloperTools state inside the permitted
app directory are recorded separately, not mistaken for original source or
silently omitted from the inventory. The external run root preserves
`fs-preflight/final-summary.json`, command records, launcher snapshots and raw
logs. These runs did **not** execute UI/runtime checks or a coding model.

### Offline, allowlisted-environment mode

Use `--offline` to deny networking, credential-service lookup and unrelated
process access in addition to the filesystem restrictions. This mode constructs
the child's environment from an allowlist rather than inheriting provider tokens,
credential-plugin settings, agent sockets or arbitrary host variables. It
redirects HOME and build/cache/signing state inside `PRIVATE`, disables shared
compilation and diagnostic listeners, and omits the shared MSBuild socket grant.
The package cache must already contain the pinned dependencies for both build
configurations; populate it through a trusted reference setup, not by granting
an offline build access to personal caches or credentials.

For example, after the trusted controls runner has populated `ROOT/cache/packages`,
make an independent APFS copy-on-write clone into a **new** private directory
on the same volume (not shared writable hard links):

```sh
mkdir "$PRIVATE"
cp -cR "$ROOT/cache/packages" "$PRIVATE/packages"
```

```sh
python3 grounding/microsoft-maui-controls/filesystem_sandbox.py \
  --offline --app "$APP" --private "$PRIVATE" \
  --dotnet-root /usr/local/share/dotnet \
  --android-sdk "$ANDROID_HOME" --java-home "$JAVA_HOME" \
  -- dotnet restore DataBindingDemos.csproj --configfile ../NuGet.config \
    -p:RuntimeIdentifier=android-arm64 -p:Configuration=Debug

python3 grounding/microsoft-maui-controls/filesystem_sandbox.py \
  --offline --app "$APP" --private "$PRIVATE" \
  --dotnet-root /usr/local/share/dotnet \
  --android-sdk "$ANDROID_HOME" --java-home "$JAVA_HOME" \
  -- dotnet build DataBindingDemos.csproj -c Debug --no-restore \
    -p:RuntimeIdentifier=android-arm64
```

Use an existing staged `APP` and private directory. The launcher creates private state
subdirectories but does not install SDKs, download packages or start an emulator.
Offline vulnerability lookup can report `NU1900`; that is an unavailable audit,
not evidence of a clean dependency audit. It is not disabled or suppressed.

Two narrowly allowed OS services support public tool execution: Rosetta and Unix
user/group lookup. Denying the hostname query used by native `uname` made Android
tools misidentify macOS; denying group lookup made .NET's `FileInfo` group probe
overflow inside MSBuild Copy/CreateAppHost. The offline mode allows those exact
queries/services, not SecurityServer or all Mach services. Actual positive-control
tests verify the credential-service, loopback, outside-process signal and
process-argument denials; filesystem tests run against both modes. CLI tests
also check environment stripping and nonzero child exit propagation.

The pinned binding source completed a nonincremental Debug build through a frozen
offline launcher, and the target completed the documented restore/Debug commands.
The earlier Copy-task overflow was independently reproduced and removed by changing
only the group-lookup service permission. These are build controls, not runtime
or model-efficacy results.

**Offline Release is still blocked:** the .NET 10 trimmer requires an out-of-process
MSBuild task host that uses a hardcoded `/tmp/MSBuild<PID>` Unix socket. With shared
socket access denied, the actual Release control failed `MSB4216`/`MSB4027`. Do not
disable trimming or weaken the task's runtime checks to hide this. Full trusted
source/target Debug/Release and runtime controls remain available through
`controls.py` and the original filesystem-only preflight. The stricter offline mode
is not a replacement for those controls or a ready executor for scored migrations.
The external `boundary-preflight/verified-offline-01/` preserves the completed
source commands and Release failure; target CLI logs are separately preserved.

This is a **local build preflight**, not the Vally actor executor or a complete
agent sandbox. Trusted reference builds and targeted OS probes do not establish
general safety for untrusted MSBuild, exhaustive broker coverage, or
coding-model/provider integration. Historical failures remain under the external
`boundary-preflight/`; they are infrastructure diagnostics, not model failures.
Nothing here enables generated Vally agent startup or authorizes model spending.

CLI control-plane isolation also needs separate verification. Restrict model
tool visibility and validate the actual instruction/skill/plugin/hook inventory;
disabling instruction files or embeddings-based skill retrieval alone does not
prove zero personal-artifact loading. Tool denials take precedence over allows:
do not design a deny-all rule with supposedly overriding allow exceptions.
See the [CLI tool-permission documentation](https://docs.github.com/en/copilot/how-tos/copilot-cli/use-copilot-cli/allowing-tools)
and [configuration-directory reference](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-config-dir-reference).
No generated spec enables model execution on the strength of these probes.

## Vally and the broader evaluation pattern

Read [Vally Get Started](https://microsoft.github.io/vally/get-started/),
[Scoring Functions](https://microsoft.github.io/vally/reference/scoring-functions/),
[the eval-spec schema](https://microsoft.github.io/vally/reference/eval-spec/),
and [the deterministic run-command grader](https://microsoft.github.io/vally/reference/graders/run-command/).
The actual shipping skill can be checked without inference using Vally 0.19.0:

```sh
vally lint src/Controls/src/NuGet/skills --verbose
```

The example to learn from is
[richlander/markout#148](https://github.com/richlander/markout/pull/148), not #14,
and its [measurement directory](https://github.com/richlander/markout/tree/main/grounding/markout).
Its `scenarios`/`assertions` YAML belongs to a different harness; it is not a
Vally spec. Likewise, fixture directories absent from an eval manifest are not
measured cases. Follow the current
[package-skills workflow](https://github.com/richlander/dotnet-package-skills/blob/main/docs/getting-started.md)
and [examples](https://github.com/richlander/dotnet-package-skills/tree/main/examples)
without copying a serializer-specific API-conformance test into a MAUI migration.

Before claiming a benefit, extend the instrument into an approved, frozen paired
study: fresh source-only app inputs, the same tools/cache/network policy in both
arms, fixed repeats per task/model, explicit model identities, and interleaved
baseline/skill-available ordering. First establish runnable source and target
controls. Keep unseen tasks separate from authoring tasks. Do not teach to a
failing assertion by changing the task, require a preferred class name, or count
skill invocation itself as successful migration.

## Candidate grading through Vally

`vally.py` generates actual Vally 0.19.0 specs, fresh matched source inputs,
mechanical target references, a copied shipping skill and protected grader
snapshots. Baseline and skill-available variants have identical tasks and
configuration apart from skill availability. The generator uses the documented `agent_environment`,
grader-only `grading_environment` and deterministic `run-command` grader,
not Markout's schema. Subsequent checkout edits do not change the copied skill
or grader.

With a fresh `ROOT` and an owned, booted Android arm64 emulator:

```sh
python3 grounding/microsoft-maui-controls/vally.py --root "$ROOT" \
  --skill "$PWD/src/Controls/src/NuGet/skills/microsoft-maui-controls-upgrade-to-11"
vally lint -e "$ROOT/vally" --param SERIAL=emulator-5584 --strict
mkdir -p "$ROOT/results"
vally grade -e "$ROOT/vally/baseline/eval.yaml" --stimulus binding \
  --workspace "$ROOT/vally/references/binding" --param SERIAL=emulator-5584 \
  --output jsonl --require-pass \
  < "$ROOT/vally/references/binding/trajectory.json" \
  > "$ROOT/results/vally-binding.jsonl" 2> "$ROOT/results/vally-binding.stderr"
python3 grounding/microsoft-maui-controls/summarize_vally.py \
  "$ROOT/results/vally-binding.jsonl" --output "$ROOT/results/vally-summary.json"
```

Repeat offline grading for the other two stimuli using separate output files.
The synthetic ATIF receipts explicitly say `reference-replay-no-agent`; they
are required replay inputs, not fabricated model trajectories. `vally grade`
does not execute an agent. Generated specs intentionally **block agent startup**:
an approved trial budget and verified app-only OS/tool boundary are still
required. The reference specs' `reference-replay` result kind must not be used
to label future model deliveries.

The grader seals only the delivered `app/` sources into a separate consumer;
build outputs and agent instruction directories are excluded. It verifies the
three read-only sibling configuration files against a protected source-stage
record, then uses the protected copies. Source hash equality is **not** required
for edited app files. SDK selection, restored package identity, Debug/Release,
APK identity/minimum and every declared UI behavior are all mandatory.
Missing runtime cannot be offset by a weighted score.

For Shell, an evaluator-owned observer is injected only after delivery sealing.
It records the attached live handler and platform object's full CLR base chains
and assembly identities, tagged with a fresh run token. The required target
ancestors are `ShellHandler` and `MauiDrawerLayout` from the actual MAUI
assemblies. A valid custom subclass is accepted; a same-named unrelated class
or a compatibility renderer is not. The app-visible setup contains neither this
observer nor a target solution.

Each grade gets a fresh evidence directory with copied source hashes, harness
identity, command stdout/stderr/exit status, evaluated build properties,
Debug/Release outputs, APK hash, raw runtime UI/native evidence and a verdict.
The candidate's own tests, explanations or claimed verdicts are not evidence.
`candidate.py` returns exit 0 for `target-pass`, 1 for `candidate-failed`, and 2
for identified `infrastructure` failures. Vally's `run-command` treats either
nonzero exit as an unsuccessful check; use `summarize_vally.py` to retain the
explicit infrastructure classification rather than scoring it as model failure.
Unknown or inconsistent records fail summary generation.

Grader-only file staging and a sealed copy are **not an OS sandbox**. The
current local executor must not run untrusted model-modified MSBuild against the
host merely because its grader files were hidden during the agent phase.
Only trusted public reference projects have been executed with this adapter.
Before paid trials, enforce and verify the source-only app write surface,
three exact read-only sibling files, toolchain reads and private build caches;
keep evaluator files, credentials, other projects and emulator access outside
the actor boundary.

### Local adapter result

Actual offline Vally grading passed all three target references and an additional
Shell reference using a custom `ExtensionHandler` subclass. Each completed
Debug/Release, APK checks and its full runtime contract: 8 binding, 11 tab,
7 directory and 11 subclass checkpoints. Saved raw XML assertions, native CLR
ancestry, APK hashes and completed command rows were rechecked. The custom
handler passed without a special-case class-name allowance. The unchanged
SDK 10 source was intentionally rejected by the target grader.

The positive and negative summaries are separately preserved as
`results/vally-final-positive-summary.json` and
`results/vally-final-negative-summary.json` under the external run root.
The generated baseline and skill-available specs both pass strict Vally lint;
the later copied-skill snapshot has a byte-identical grading instrument.
All 27 standard-library Python regressions pass on macOS, including both
preflight profiles' real-process boundary checks and offline environment handling.

Preflights remain diagnostics, not model outcomes. An initial private-key
signature conflict required replacing the previous owned sample installation.
An experimental native-factory override compiled but produced an empty Shell:
it skipped the base handler's private navigation-root initialization and was
correctly rejected by UI checks. The passing subclass retains base
initialization; no runtime assertion was weakened to accept that experiment.
These reference replays prove the adapter executes meaningful checks, not that
skills improve model migrations or that a native-view factory override is safe.

Compare complete deliveries, report failures and infrastructure separately,
record actual skill/reference use, and measure efficiency only alongside
productivity. Small raw time/call differences are not certified savings. The
previous four-versus-four Shell pilot remains separate historical evidence:
both arms passed, and only two skill-available runs actually loaded the skill.
It does not establish broader efficacy, guaranteed activation, or a release
ship gate.

## Prototype conclusion

The package payload, install/discovery workflow, sample readiness checks and
Vally reference grading are demonstrated; a skills correctness advantage is
**not**. The approved historical comparison is closed with both arms passing,
not extended until a favorable result appears. The public sample suite broadens
the executable measurement instrument, not the model-efficacy denominator.
Independent code/claim review found no blocking defects, but reviewer model
identities were not verified and are not presented as a paired multi-model study.

Further paired-model trials and a production-quality isolated actor executor
are follow-up research, requiring their own verified execution boundary and
approved budget. They are not prerequisites for reviewing this text-only draft
package prototype, and their absence must not be hidden by a completion marker,
reference replay, disabled audit or claimed model saving. Generated agent
startup remains blocked. Promotion to MAUI 11/final release coordinates and
cross-platform validation remain release-owner decisions, not results of this
Android pilot.
