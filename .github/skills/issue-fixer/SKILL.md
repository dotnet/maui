---
name: issue-fixer
description: "Reproduce-first, root-cause-first workflow for fixing a .NET MAUI GitHub ISSUE (often with no PR yet). Front-loads a faithful empirical reproduction and true root-cause analysis BEFORE any fix, then composes try-fix / verify-tests-fail-without-fix / run-device-tests / run-helix-tests / write-ui-tests / write-xaml-tests / azdo-build-investigator for the fix and verification. Use whenever asked to 'fix issue #XXXXX', 'reproduce issue #XXXXX', 'root-cause issue #XXXXX', 'why does #XXXXX happen', 'investigate this bug', or handed a repro repo/branch and a broken behavior — even if the user doesn't say the word 'reproduce'. Do NOT use to review an existing PR (use pr-review) or to run one isolated fix attempt (use try-fix)."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires git (incl. worktrees), PowerShell 7+ (pwsh), and the .NET MAUI build environment (BuildTasks + platform workloads). Device/Helix verification needs Android SDK/emulator, Xcode, or the run-helix-tests prerequisites depending on platform.
---

# Issue Fixer — Reproduce First, Root-Cause First

A discipline for fixing a MAUI bug **from the issue up**. The whole point of this skill is to refuse to
guess: never assume the bug is real, never assume a fix is correct, and never assume a "version" label maps
to a git tag — **prove everything empirically** before and after you touch code.

This skill exists because the expensive failures in real MAUI bug work are almost never "I couldn't write the
fix." They're "I fixed the wrong thing," "I calibrated against the wrong source tree," or "my test was
circular and proved nothing." Front-loading reproduction and root-cause analysis is what prevents those.

**Trigger phrases:** "fix issue #XXXXX", "reproduce issue #XXXXX", "root-cause issue #XXXXX",
"why does #XXXXX happen", "investigate this bug", "I have a repro repo for this crash/leak".

## When NOT to use this skill (differentiation)

This skill deliberately overlaps in vocabulary with two neighbours. Pick the right one:

| If the situation is… | Use | Why |
|----------------------|-----|-----|
| A **PR already exists** and needs review / alternative-fix exploration / a merge recommendation | **`pr-review`** | pr-review is PR-centric: it runs a gate, 4-model try-fix, and writes a review report. It assumes the fix and (usually) a test already exist. |
| You just need to **run ONE alternative fix idea**, test it, and get a pass/fail back | **`try-fix`** | try-fix is a single-shot building block. This skill *calls* it during Phase 3 — it is not a replacement for it. |
| You only need to **prove a test catches the bug** (fails without fix / passes with fix) | **`verify-tests-fail-without-fix`** | That skill owns the inverted-semantics verification. This skill *calls* it during Phase 4. |
| You're asked **"why is CI red / is this PR ready to merge"** | **`azdo-build-investigator`** | CI triage, not issue reproduction. This skill *calls* it only for Phase 4 CI escalation. |

**issue-fixer is different:** it starts from a GitHub **issue** (frequently with **no PR at all**), and it spends
its first two phases establishing a faithful reproduction and the true root cause *before* any code changes.
It **composes** the skills above rather than re-implementing them. If a PR already exists for the issue, prefer
`pr-review`; only fall back here when the issue still needs original reproduction and root-cause work.

## Guardrails (read before doing anything)

- 🚫 **Never push or commit to GitHub without explicit user approval.** When you reach a point where pushing
  would be the next step, **stop and ask.** The user reviews locally first.
- 🚫 **Never touch the main checkout.** Do all work in this session's worktree, and create *throwaway*
  `git worktree` checkouts for reproduction builds (see Phase 1). Never build or mutate the user's primary tree.
- 🌿 **Follow the repo git rules:** work on the session feature branch; no rebase / squash / force-push unless
  the user explicitly asks.
- 🧾 This skill produces **analysis + reproduction evidence + a fix + verification**. It does **not** post PR
  review approvals and never runs `gh pr review --approve` / `--request-changes`.

## The eight disciplines (the heart of this skill)

These are hard-won lessons. They're written as reasoning, not rote rules, because you'll need to apply them to
situations they don't literally describe. Internalize the *why*.

1. **Reproduce first, always.** A fix for a bug you haven't reproduced is a hypothesis, not a fix. Treat
   *"I can't reproduce it"* not as a conclusion but as a signal that a variable is still missing — go hunt it
   (see discipline 4). Only after you can make the bug happen on demand do you get to reason about causes.

2. **Use the reporter's *exact bits* and resolve source provenance — don't equate a version label with a git
   tag.** A reporter's *"Version with bug: 10.0.60"* is a **NuGet package label, not necessarily a git tag.**
   If a repro branch is in the MAUI source commit graph, find its **actual base commit** with `git merge-base`
   and build MAUI at that commit. If it is an external application repo, pin the app revision and its exact
   package reference instead — unrelated histories cannot establish a MAUI source base. Reproduce behavior
   against that exact package even when the version-to-commit mapping remains unresolved; label any source
   analysis baseline as approximate. (In the case that motivated this skill, the MAUI-source repro branch was
   based on `main` ~10 days newer than the `10.0.60` tag; calibrating against the tag produced false negatives.)
   When building MAUI from source, build `Microsoft.Maui.BuildTasks.slnf` first, then the app.

3. **Use the reporter's exact app, supplied build command, and steps.** Fast-deployment debug builds
   (`-t:Install`, no `EmbedAssembliesIntoApk`) can behave *differently* from embedded-assembly APKs. When the
   reporter supplies a `dotnet build` invocation, reproduce it exactly; always preserve their exact interaction
   sequence. If the command is omitted, use only the bounded canonical fallback in Phase 1 and label it as not
   reporter-exact. Small deviations ("I used the Sandbox instead," "I skipped a tap") routinely hide the bug.

4. **Enumerate and eliminate environmental variables — and record each as tested.** When the bug won't
   reproduce, systematically vary the things you control and log the result of each:
   API level; deploy mode (fast-deploy vs embedded-assembly APK); emulator ABI (**arm64-v8a** on Apple Silicon
   vs **x86_64** on CI/Helix); emulator vs physical device; source commit. Keep a table (template below). When
   you've exhausted every variable you control and it *still* won't repro, **name the remaining delta you
   cannot test locally** (e.g. x86_64 Helix) explicitly — don't pretend it's not there and don't declare "not
   a bug."

5. **Separate symptom from root cause.** A fix that merely severs the last reference or flushes pending state
   — e.g. `CurrentView = null`, or calling something like `ExecutePendingTransactionsEx()` to force a pending
   fragment transaction — may be a **symptom fix** if it doesn't remove **what actually roots the object** or
   **what actually causes the crash.** Always force the question: *what is the true root cause, and does this
   change address it or just paper over the observable symptom?* Prefer root fixes. If only a symptom fix is
   feasible right now, **say so** and document what the real root is and why the root fix was deferred.

6. **Reject circular / tautological tests.** A test that asserts the exact field the fix mutates proves
   nothing about real behavior (fix sets `CurrentView == null`; test asserts `CurrentView == null` → circular).
   - A **leak** test must use `WeakReference` + `GC.Collect()` + `GC.WaitForPendingFinalizers()` and assert the
     object graph is **actually collected** — not that some convenience field was nulled.
   - A **crash** test must actually **exercise the crashing code path** (the real navigation/detail-swap/etc.),
     not a stand-in.
   Run the circular-test checklist (below) before you trust any test.

7. **Escalate to CI for ground truth when local repro is genuinely impossible.** When the only remaining
   variable is one you can't control locally (classically: x86_64 Helix ABI), escalate to a real device test in
   CI (`maui-pr-devicetests`) and read the **actual** result. Use this decision matrix:

   | Unfixed test result | Fixed test result | Conclusion |
   |--------------------|-------------------|------------|
   | **PASSES** unfixed | — | Bug does **not** reproduce on this train → the fix is **redundant here**; don't ship it as a "fix" without saying so. |
   | **FAILS** unfixed | **PASSES** with fix | Bug is **real** and the fix is **needed** — the clean, shippable outcome. |
   | **FAILS** unfixed | **FAILS** with fix | Fix is **wrong or incomplete** → back to Phase 2/3. |

   See `.github/skills/azdo-build-investigator/SKILL.md` and `.github/docs/maui-ci-facts.md` for correct
   pipeline names/IDs and how to read Helix results (including the XHarness exit-0 blind spot).

8. **No shortcuts, no assumptions.** Assume infinite budget: thoroughness beats speed. Every claim in your
   final report must be backed by an **observed** result — a build that ran, a tap sequence that crashed, a
   test that went red then green. "It should work" is not evidence.

## Workflow

Run the phases in order. Each phase has an exit condition; don't advance until it's met. If a phase is
genuinely blocked by the environment, record *why* and what you'd need — don't paper over it.

### Phase 0 — Triage & context

Understand the bug on paper before touching a keyboard.

- Read the issue body **and every comment in chronological order**: symptom, expected vs actual,
  **stated version(s)**, platform(s), and linked discussion. Human corrections in later comments supersede
  stale details in the body or earlier agent analysis. `gh issue view <n> --comments`.
- Collect the reporter's **reproduction assets**: repro repo/branch URL, exact `dotnet build`/run command,
  exact interaction steps, screenshots/stack traces/logs.
- **Classify the setup evidence — reporter-exact vs canonical fallback.** The reporter's *exact* launch command
  is the gold standard and is **required whenever they give it**, because build flags and deploy mode change
  behavior (discipline 3). But reporters routinely hand you a complete, runnable project + platform/TFM +
  interaction steps and simply *omit the terminal command*. That omission alone is **not** a reason to block:
  the app is still runnable. When the command is missing, record the absence explicitly, and note that Phase 1
  will fall back to a **canonical** launch (see Phase 1) — clearly labeled as *not reporter-exact*. Only treat
  Phase 0 as truly blocked when there is **no** viable declared target at all (no project, no platform/TFM, or
  no interaction steps).
- Search for **existing PRs/issues** that already touch this. If a PR already fixes it → this is probably a
  `pr-review` job; note it and confirm with the user before continuing here.
- Write the **falsifiable claim** you're going to prove, e.g. *"On Android, tapping Detail→Swap twice on the
  reporter's app crashes with `IllegalStateException` in fragment commit."*

**Exit:** you can state the app, platform, interaction steps, and the claim to reproduce, **and** you have
classified the launch command as either `reporter-exact` (captured verbatim) or `canonical fallback` (command
absent — absence recorded, fallback flagged for Phase 1). Block only when no runnable target exists.

### Phase 1 — Faithful reproduction

Make the bug happen on demand, against the reporter's real tree. Two independent evidence tracks run here —
keep them separate so you never over-claim:

- **Source provenance** (what MAUI *bits* the behavior runs on).
- **Application behavior** (does the app misbehave when driven through the reporter's steps).

1. **Pin the source commit (provenance track).** If a repro branch is in the MAUI source repository (or the
   same commit graph), `git merge-base` it against `main`/candidate to find its *actual* base — do **not**
   trust the version label (discipline 2). This `git merge-base` rule is **not** relaxed when a repro branch
   gives you a MAUI source base. An external application-repo branch instead establishes the app revision, not
   the MAUI source revision; use the app's exact package reference as its MAUI provenance evidence. If the
   package version cannot be mapped to a commit, that must **not** block reproducing behavior on the reporter's
   **exact package version**. Unmapped provenance only limits your *exact-source / root-cause* claims later.
   If you must build MAUI from source for analysis and can't resolve the commit, use the nearest analysis
   baseline **only with an explicit "approximate, not the reporter's exact source" label** and report the
   unresolved mapping.
2. **Create a throwaway MAUI worktree only when source analysis/building needs a resolved source commit**
   (never the main checkout):
   ```bash
   git worktree add ../maui-repro-<sha> <sha>
   ```
   Build `Microsoft.Maui.BuildTasks.slnf` first when building MAUI from source. Build an external reporter app
   from its own clone/project against its declared package version; do not assume it belongs in the MAUI
   worktree.
3. **Launch the app — reporter-exact if given, otherwise a bounded canonical fallback.**
   - **Reporter-exact (preferred, required when supplied):** use their exact `dotnet build`/run command and
     deploy mode (discipline 3).
   - **Canonical fallback (command absent):** derive a **documented, canonical** MAUI launch command from the
     project's *declared* TFM/platform and the repository's own tooling (e.g. the standard
     `dotnet build -t:Run -f <declared-tfm>` path, or the repo's device-test/run scripts). **Bounds — do not
     cross:** don't invent undeclared configuration flags, device identifiers, deployment modes, or extra
     interaction steps. Record the **exact fallback command**, the **source** that makes it canonical (which
     declared TFM/tooling it came from), and **every remaining unknown** in the variable table. If there is no
     viable declared target or tooling to derive a command from, stay **blocked** and say why.
     > The fallback reproduces **application behavior only** — it is *not* evidence of the reporter's original
     > build environment. Label any behavior you observe as "reproduced via canonical fallback (not
     > reporter-exact build env)."
4. **Run the reporter's exact steps** on the app. Reproduce the crash/leak/misbehavior. A behavior reproduced
   under the canonical fallback **is sufficient** to continue to root-cause investigation (Phase 2) — just
   carry the label forward.
5. **If it won't reproduce, enumerate variables** (discipline 4) and record each attempt:

   | Variable | Value tried | Result | Notes |
   |----------|-------------|--------|-------|
   | Application revision | `<sha>` / branch / supplied snapshot | ❌/✅ | external repro app identity |
   | MAUI source commit | `<sha>` via merge-base / `unresolved` / `N/A exact-package repro` | ❌/✅ | never substitute a package label for a commit |
   | Launch command | `reporter-exact` / `canonical fallback: <cmd>` | | source of canonical + unknowns |
   | Package provenance | version→commit `<mapped sha>` / `unmapped` | | approximate-baseline label if unmapped |
   | API level | e.g. 34 | | |
   | Deploy mode | fast-deploy `-t:Install` / embedded APK | | unknown if fallback |
   | Emulator ABI | arm64-v8a (Apple Silicon) / x86_64 (Helix) | | |
   | Device | emulator / physical | | |

   A canonical fallback that does **not** reproduce is **not** a false negative — enumerate the remaining
   variables/delta exactly as today (a missing reporter-exact command is itself one such variable). When every
   controllable variable is exhausted and it still won't repro, **name the uncontrollable delta**
   (e.g. x86_64 Helix) and carry it to Phase 4 escalation — do **not** conclude "not a bug."

**Exit:** either a reliable local reproduction — labeled `reporter-exact` or `canonical fallback` — **or** an
explicit, evidence-backed statement of the remaining variable(s) you can't test locally, with provenance
(mapped/unmapped) recorded.

### Phase 2 — Root-cause analysis

Find *what actually causes* the observed behavior — not the last thing that touches it.

- **Minimize only after faithful reproduction.** Reduce the reproduced case one variable at a time and verify
  the smaller case still fails. This is a diagnostic accelerator, not a replacement acceptance scenario: the
  reporter's exact app/steps (or labeled canonical fallback) remains the final behavior check. If minimization
  loses the failure, keep the faithful repro and record which removed variable mattered.
- **Form at least 3 meaningfully competing root-cause hypotheses.** For each, record: theory, a discriminating
  command/observation that could reject it, result (`CONFIRMED`, `DENIED`, or `PARTIAL`), and implications.
  Three variants of the same symptom are not competing hypotheses. Prefer experiments that distinguish several
  hypotheses at once; record this in session evidence/report artifacts, not a new planning file in the repo.
- Trace the failing path in the reproduced app/bits: what roots the leaked object, or what state makes the crash
  legal? Use the debugger/logs/`lsp` call hierarchy. When package provenance is unresolved, keep runtime
  observations separate from hypotheses based on an approximate source baseline — don't call approximate
  source the reproduced tree.
- Apply **symptom-vs-root** (discipline 5): for every candidate change, ask "does this remove the cause, or
  just hide the effect?" Write down the root cause hypothesis explicitly.
- Prefer a **root** fix. If only a symptom fix is viable now, document the real root and why the root fix is
  deferred, so the follow-up is not lost.

**Exit:** a minimized diagnostic repro (or evidence explaining why it cannot be reduced), a result for each
competing hypothesis, and an evidence-selected root-cause hypothesis + location where the true fix should live.

### Phase 3 — Fix (multi-model exploration + adversarial consensus)

Don't produce the fix single-track. Borrow `pr-review`'s proven structure — *come up with a fix, then
challenge that fix* — because a fix that survives being attacked by other models is far likelier to be a
**root** fix than a symptom fix. This phase mirrors `pr-review` Phase 2 (multi-model `try-fix` +
cross-pollination + comparison-table selection), then adds an adversarial challenge of the winner.

Don't hand-roll the fix loop — every attempt goes through **`try-fix`**
(`.github/skills/try-fix/SKILL.md`), which establishes a baseline, applies **one** approach, tests it, records
artifacts, and reverts.

> 🚨 **Sequential only — never parallel/background.** Run each `try-fix` with `mode: "sync"`, one fully
> completing before the next starts. try-fix mutates the same target files, shares the emulator/device, and
> reverts to baseline between runs; running them concurrently corrupts each other's file state and results.
> Restore baseline between attempts exactly as try-fix/pr-review prescribe.

#### Round 1 — independent exploration

Invoke `try-fix` via `general-purpose` task agents across these 4 models, **SEQUENTIALLY**:

| Order | Model |
|-------|-------|
| 1 | `claude-opus-4.6` |
| 2 | `claude-opus-4.7` |
| 3 | `gpt-5.3-codex` |
| 4 | `gpt-5.5` |

Feed each `try-fix` invocation:
- `problem` — the faithful + minimized reproductions and the Phase 2 evidence-selected root-cause hypothesis
- `hints` — decisive evidence for the selected hypothesis plus `DENIED` / `PARTIAL` alternatives and why, so
  each model aims at the root without blindly rediscovering rejected causes (it may challenge them with new
  evidence)
- `platform` — the reproduced platform from Phase 1
- `target_files` — the files identified in Phase 2
- `test_command` — the strongest empirical command currently available: the planned Phase 4 regression command
  when a test already exists; otherwise the faithful Phase 1 behavior reproducer through the appropriate repo
  runner, explicitly labeled provisional. A provisional reproducer does not replace Phase 4's non-circular
  regression test.

Each model must generate its approach **independently first**, then produce a **DIFFERENT** approach from any
existing changes (review the diff first). "Different" means a different root-cause hypothesis, not just a
different line — see discipline 5.

#### Round 2 — cross-pollination (mandatory)

After Round 1, show **each** model the list of attempts + results and ask for NEW ideas
("NEW IDEA: …" or "NO NEW IDEAS"). Run any new ideas as further **sequential** `try-fix` attempts. Repeat
until every model says NO NEW IDEAS (max 3 rounds).

#### Selection

Build a comparison table of all ✅-passing candidates and rank them, in priority order:
1. **Addresses the ROOT cause, not a symptom** (discipline 5) — a fix that merely nulls a reference or flushes
   pending state is symptom-level and ranks below one that removes what actually roots the object / causes the
   crash.
2. **Simplest** — fewest files/lines.
3. **Most robust** — handles edge cases.
4. **Matches MAUI codebase style** — consistent with existing patterns.

Pick the winner.

#### Materialize the selected fix

`try-fix` **always restores the working directory to baseline**, so the winner exists only as its recorded
`fix.diff` after exploration. Apply that exact candidate diff to the session feature worktree, verify the
materialized diff matches the reviewed candidate and contains no attempt artifacts, then rerun the same
empirical command. Do not commit or push. This materialized diff — not a prose summary or a reverted attempt —
is what the adversarial reviewers challenge and what Phase 4 verifies.

#### Adversarial challenge of the selected fix (the "challenge that fix" step)

> **Adaptation vs pr-review:** `pr-review` challenges an **existing PR's** fix. issue-fixer has **no PR yet**,
> so the artifact under attack here is the winning fix the multi-model exploration *just produced* — not a
> PR. Everything else about the challenge mirrors the adversarial agree/dispute/discard pattern.

Dispatch the **other** models (not the one that authored the winner) to attack the winning fix. Each returns a
structured verdict — **AGREE / DISPUTE / DISCARD** — with reasoning, probing specifically:
- **Root vs symptom** — does it remove the actual cause, or paper over the observable symptom?
- **Regression risk** — could it reintroduce the leak, or introduce a new crash / lifecycle bug?
- **Test integrity** — is the Phase 4 verification test circular/tautological (does it fail the circular-test
  checklist in Phase 4)? If only a provisional behavior reproducer exists, mark this dimension **DEFERRED**
  rather than treating the absence of a regression test as agreement.

Synthesize consensus:
- Consensus **AGREE** on the fix → the fix survives; advance to Phase 4. A deferred test-integrity dimension
  remains a Phase 4 gate.
- Consensus **DISPUTE / DISCARD** → loop back: to **Phase 2** if the root cause itself was wrong, or to
  **Phase 3 Round 1** if only the approach was wrong. Only a fix that survives the challenge advances.

**Exit:** the multi-model winner is materialized in the session worktree, builds, and **survived adversarial
challenge** — believed to remove the root cause. Any deferred test-integrity challenge is recorded for Phase 4.

### Phase 4 — Verification (non-circular, escalate if needed)

Prove the fix works with a test that would actually catch the bug.

1. **Pick / write a non-circular test** (discipline 6). If the issue lacks coverage, compose the right author:
   - UI interaction bug → **`write-ui-tests`** (`.github/skills/write-ui-tests/SKILL.md`)
   - XAML parse/XamlC/source-gen bug → **`write-xaml-tests`** (`.github/skills/write-xaml-tests/SKILL.md`)
   - Then run the **circular-test checklist** below and fix the test if it fails any item.
   - If Phase 3 deferred the test-integrity challenge, send the final test to the other models for that focused
     probe now. A dispute loops back to test authoring; don't let the earlier fix consensus waive this gate.
2. **Verify fail-without-fix / pass-with-fix** via **`verify-tests-fail-without-fix`**
   (`.github/skills/verify-tests-fail-without-fix/SKILL.md`) — remember its **inverted** semantics
   (tests *failing* without the fix is the *good* outcome).
   - Local device/unit runs: **`run-device-tests`** (`.github/skills/run-device-tests/SKILL.md`).
   - Helix unit projects (XAML, Core, Essentials, Resizetizer, …): **`run-helix-tests`**
     (`.github/skills/run-helix-tests/SKILL.md`).
3. **Run the affected area's existing regression sweep.** After targeted red/green proof, run the containing
   test class/category or smallest established area suite. If it fails, reproduce that failure on the unfixed
   baseline before calling it pre-existing, unrelated, or flaky. For intermittent failures, record repetitions
   and observed failure rate; one pass is not evidence of flakiness or safety.
4. **Escalate to CI when local repro was impossible** (discipline 7). Run the real device test on
   `maui-pr-devicetests` and read the **actual** Helix/test result (not code attributes, not exit-0). Apply the
   decision matrix in discipline 7 to decide whether the bug is real and the fix is needed. Use
   **`azdo-build-investigator`** for reading CI.

**Circular-test checklist** — reject the test if any is true:
- [ ] Its **setup** does not reproduce the issue's exact relevant scenario (including corrections from comments).
- [ ] Its **assertions** do not verify the exact user-visible behavior reported broken/fixed.
- [ ] It asserts the exact field/state the fix mutates (e.g. fix sets `X=null`, test asserts `X==null`).
- [ ] A leak test lacks `WeakReference` + `GC.Collect()` + `GC.WaitForPendingFinalizers()` and a real
      collection assertion.
- [ ] A crash test doesn't actually drive the crashing path (uses a stand-in / mocks the very thing under test).
- [ ] It passes *without* the fix (then it doesn't catch the bug — see verify-tests-fail-without-fix).

**Exit:** evidence that the test goes **red without the fix** and **green with it** — locally or in CI — plus
a clean area regression sweep or baseline evidence classifying every additional failure.

### Phase 5 — Report

Summarize with evidence, then **stop and ask before pushing** (guardrails).

Use this structure:

```markdown
## Issue #XXXXX — <one-line summary>

### Reproduction
- Application revision: <reporter repo commit / branch / supplied project snapshot>
- MAUI runtime bits: <source commit | exact NuGet package version>
- MAUI source provenance: <resolved via merge-base or version→sha | unresolved — analysis baseline <sha> is approximate>
- Launch command: <reporter-exact: <cmd> | canonical fallback: <cmd> — source: <declared TFM/tooling>; not reporter-exact build env>
- App / steps: <exact interaction steps>
- Result: <reproduced ✅ (reporter-exact | canonical fallback) | not reproducible locally — remaining delta: <x86_64 Helix/…>>
- Variable enumeration: <table or link to it>
- Remaining unknowns: <flags/device/deploy-mode not derivable from declared config>

### Root cause
- <the true root cause>
- Competing hypotheses: <CONFIRMED / DENIED / PARTIAL table with decisive evidence>
- Symptom-vs-root: <why this is the root, not just the last reference>

### Fix
- Approach (via try-fix): <what changed, which files, why it removes the root>
- Alternatives tried and why rejected: <…>
- Adversarial consensus: <AGREE / DISPUTE / DISCARD — reviewers and decisive reasoning>

### Verification
- Test: <name> — non-circular because <reason>
- Fail-without-fix / pass-with-fix: <local result | CI decision-matrix outcome>
- Regression sweep: <command + result; baseline comparison for every additional failure>

### Status
- Local checks: <…>
- ⏸️ Awaiting user approval before pushing.
```

## Skill composition map

This skill orchestrates; it does not re-implement. Quick reference for what to call and when:

| Phase | Compose | For |
|-------|---------|-----|
| 3 | `try-fix` ×4 models (sequential) + cross-pollination + adversarial challenge | Multi-model fix exploration, then attack the winner (AGREE/DISPUTE/DISCARD) before it advances |
| 4 | `write-ui-tests` / `write-xaml-tests` | Author a non-circular reproducing test |
| 4 | `verify-tests-fail-without-fix` | Prove fail-without-fix / pass-with-fix (inverted semantics) |
| 4 | `run-device-tests` / `run-helix-tests` | Actually run the verification locally |
| 4 | `azdo-build-investigator` | CI escalation + reading Helix/device-test ground truth |

## Origin

The disciplines here were distilled from an intense empirical investigation of a MAUI Android bug — the
issue #35371 stale-`ContainerView` leak (PRs #35372 / #36169) and the FlyoutPage detail-swap fragment crash —
where reproduction hinged on the reporter's *actual* base commit (not the `10.0.60` label), on embedded-assembly
vs fast-deploy, and ultimately on the x86_64-Helix ABI that couldn't be reproduced on Apple-Silicon locally.
The recurring failure mode was calibrating against the wrong tree and writing circular tests. This skill encodes
the fix: reproduce faithfully, find the real root, and verify with a test that can actually go red.
