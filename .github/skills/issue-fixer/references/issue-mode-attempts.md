# Issue-mode attempt isolation

Use this protocol when `issue-fixer` invokes `try-fix` before a PR/fix commit exists. The ordinary
`EstablishBrokenBaseline.ps1` path is intentionally PR-shaped: it detects committed non-test changes between
the merge base and `HEAD`. It cannot represent an issue whose only current change is reproduction scaffolding.

The protocol keeps three states distinct:

1. **Source baseline** — MAUI before the reproduction or fix.
2. **Broken checkpoint** — source baseline plus the repo-owned reproduction, committed only on a temporary
   local branch.
3. **Candidate** — broken checkpoint plus one attempted product fix, committed only on that attempt's
   temporary local branch.

Temporary commits are isolation artifacts. Never push them, never create them on the user's session branch,
and remove their worktrees/branches after the selected result is materialized.

## 1. Create the broken checkpoint

Create a dedicated worktree and local branch from the source baseline. Implement Phase 1's repo-owned
reproduction there, prove the expected failure, then commit **only the explicit reproduction paths**:

```bash
git worktree add -b issue-<N>-repro-checkpoint <temp-root>/repro <source-ref>
cd <temp-root>/repro
git add -- <explicit-reproduction-paths>
git -c user.name="MAUI Issue Fixer" -c user.email="issue-fixer@example.invalid" \
  commit -m "Temporary reproduction checkpoint for issue <N>"
```

Record:

- checkpoint branch and commit SHA
- source baseline SHA
- exact reproduction paths
- test type/filter/command and failing result

Do not use `git add .`. Do not include logs, attempt artifacts, product fixes, or unrelated worktree changes.

## 2. Create one isolated worktree per attempt

Attempts still run **sequentially** because they share the emulator/device, but each gets independent Git
state:

```bash
git worktree add -b issue-<N>-attempt-<K> <temp-root>/attempt-<K> <checkpoint-sha>
```

Create `<attempt-output>` as an absolute directory outside `<temp-root>/attempt-<K>` so artifacts survive
worktree removal.

Invoke `try-fix` with:

- `Mode: Issue`
- `Broken checkpoint: <checkpoint-sha>`
- `Attempt worktree: <absolute-path>`
- `Reproduction paths: <explicit list>`
- `Output directory: <absolute-attempt-output-path>`
- the ordinary problem/platform/target-files/test-command/hints inputs

Issue mode must verify before editing:

- `HEAD` equals the checkpoint SHA
- the attempt worktree is clean
- every reproduction path is committed at the checkpoint

The candidate may not modify reproduction paths. A fix that changes its own reproducer is circular and must
be rejected.

## 3. Capture the complete candidate

After testing and self-review, stage **only explicit candidate product paths**. Include added, modified,
deleted, and renamed files; exclude reproduction paths and `CustomAgentLogsTmp/`.

Create an ephemeral local candidate commit and export its complete binary diff:

```bash
git add -- <explicit-candidate-paths>
git -c user.name="MAUI Issue Fixer" -c user.email="issue-fixer@example.invalid" \
  commit -m "Temporary issue <N> candidate <K>"
git diff --binary <checkpoint-sha> HEAD > <attempt-output>/fix.diff
git rev-parse HEAD > <attempt-output>/candidate-commit.txt
```

Because both sides are commits, `fix.diff` includes staged changes, new files, deletions, renames, and file
modes. A plain `git diff` is not an acceptable issue-mode artifact.

The invoker then removes the attempt worktree. Keep the local attempt branch until selection is complete so
the candidate commit remains reachable.

## 4. Verify the winner from committed state

Create a disposable verification worktree at the winning candidate commit. Run
`verify-tests-fail-without-fix` in full-verification mode against the checkpoint:

```powershell
pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 `
  -BaseBranch issue-<N>-repro-checkpoint `
  -FixFiles @(<explicit-candidate-product-paths>) `
  -TestType <type> `
  -TestProject <unit-project-key-or-csproj-path | device-project-name> `
  -TestFilter "<filter>" `
  -Platform <platform-if-required> `
  -PRNumber <N> `
  -RequireFullVerification
```

Both the reproduction and candidate must be committed in this disposable worktree. `-TestProject` is required
for `UnitTest` and `DeviceTest`; omit it for `UITest`/`XamlUnitTest`. Never omit `-RequireFullVerification`;
failure-only mode cannot establish that the candidate passes.

## 5. Materialize and clean up

After adversarial consensus and full verification, export the complete final change from source baseline to
the winning candidate, then apply it to the user's session feature worktree:

```bash
git diff --binary <source-baseline-sha> <winning-candidate-sha> > <temp-root>/selected.patch
git -C <session-worktree> apply --index <temp-root>/selected.patch
```

Review the staged paths, unstage them for normal local review if appropriate, and confirm the session
worktree contains exactly the reproduction + selected fix. Do not commit or push without user approval.

Remove only the explicitly named temporary worktrees and local branches after materialization. Never remove or
reset the session worktree or the main checkout.
