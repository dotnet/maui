---
name: agentic-workflow-designer
description: Conversational skill that interviews users to design new agentic workflows
disable-model-invocation: true
---

# Workflow Designer

Use this skill to run a structured interview with users who know their goal but not the workflow syntax yet, then generate one complete workflow `.md` file.

## When to Use This Skill

Use this before `.github/aw/create-agentic-workflow.md` when requirements are unclear or incomplete.

- Use `skills/agentic-workflow-designer/SKILL.md` to discover and confirm requirements.
- Use `.github/aw/create-agentic-workflow.md` once requirements are clear and ready for implementation.
- Use `.github/aw/agentic-chat.md` when the user wants a specification/pseudo-code instead of a runnable workflow file.

## Interview Framework

Ask one question at a time. Move to the next phase only after the current phase is clear.

### Phase 1: Goal

Ask: **"What do you want to automate?"**

Capture:
- Workflow name (kebab-case candidate)
- Brief description
- Optional emoji

### Phase 2: Trigger

Ask: **"When should this run?"**

Follow up only if needed:
- Which event type(s)?
- Any filters (labels, branches, commands)?
- Scheduled cadence (daily/weekly/hourly)?

Map to the `on:` block.

### Phase 3: Scope (Read/Write)

Ask:
- **"What should it read?"** (issues, PRs, code, discussions, CI data)
- **"What should it create or update?"** (comments, issues, PRs, labels)

Map to:
- `permissions:` (keep read-only for agent job)
- `tools:`
- `safe-outputs:`

### Phase 4: Data Strategy

Ask:
- **"What data does the agent need to make decisions?"**
- Follow up: **"Can we pre-fetch and aggregate that data with shell commands so the agent only reads compact JSON?"**

Capture:
- Whether `steps:` should pre-fetch GitHub data with `gh` + `jq`
- Output paths under `/tmp/gh-aw/data/`
- Whether batch work should use sub-agents

Map to:
- `steps:`
- Prompt references to pre-computed file paths

### Phase 5: Guardrails

Ask: **"Should it block merging, just advise, or silently log?"**

Capture:
- Visibility expectations (comment, issue, no visible output)
- No-op behavior expectation

Guide toward safe output behavior and explicit `noop` instructions.

### Phase 6: Context & Network

Ask: **"Does it need external APIs, web access, package installs, or MCP servers?"**

Follow up:
- **"Any third-party services or MCP servers to include (for example Slack, Jira, Datadog, custom internal MCP)?"**
- **"Are you deploying on GitHub.com, GHEC with custom endpoints, or GHES?"**
- For each integration, identify required auth from source docs and map it to GitHub Actions secrets + workflow env variables.
- Ask for exact external domains (FQDN/wildcard).

Map to:
- `network.allowed`
- Optional MCP/GitHub tool usage in `tools:`
- `secrets:` / `env:` wiring for integration tokens
- GHES/GHEC settings such as `engine.api-target` and `aw.json` `ghes: true` (when applicable)

### Phase 7: Engine (optional)

Ask only if ambiguous: **"Any AI engine preference?"**

If no preference, suggest default:
- "I'd suggest Copilot since you haven't mentioned a preference. Sound good?"

Map to `engine:` only when not default.

### Phase 8: Confirmation

Present a structured summary and ask for approval before generation.

## Decision Heuristics

### Trigger Mapping

| User says... | Maps to |
|---|---|
| "when someone opens a PR" | `on: pull_request:` with `types: [opened]` |
| "when a PR is updated" | `on: pull_request:` with `types: [opened, synchronize]` |
| "every morning", "daily" | fuzzy schedule shorthand `on: schedule: daily on weekdays` (compiler expands to cron) |
| "every Monday", "weekly" | fuzzy schedule shorthand `on: schedule: weekly` (compiler expands to cron) |
| "when I say /review" | `on: slash_command:` with `name: review` (or requested command) |
| "when an issue is labeled bug" | `on: issues:` with `types: [labeled]` and label filter guidance |
| "run when label ai-review is added" | `on: label_command:` with `name`/`names`, optional event scoping, and label-as-command semantics |
| "run on PRs from forks" | `on: pull_request:` plus explicit `forks:` allowlist and fork security guardrails |
| "sometimes automatic, sometimes manual" | semi-active pattern: combine `schedule`/event triggers with `workflow_dispatch` |
| "manually", "on demand" | `on: workflow_dispatch:` |
| "when a deployment fails" | `on: deployment_status:` |
| "when another workflow finishes" | `on: workflow_run:` |

### Safe Output Mapping

| User says... | Maps to |
|---|---|
| "post a comment" | `add-comment` |
| "create an issue" | `create-issue` |
| "update issue title/body" | `update-issue` |
| "close the issue" | `close-issue` |
| "assign someone", "remove assignment" | `assign-to-user`, `unassign-from-user` |
| "set issue type/field/milestone" | `set-issue-type`, `set-issue-field`, `assign-milestone` |
| "open a PR", "submit changes" | `create-pull-request` |
| "update PR description/title" | `update-pull-request` |
| "close the PR", "merge the PR" | `close-pull-request`, `merge-pull-request` |
| "mark PR ready", "sync PR branch" | `mark-pull-request-as-ready-for-review`, `update-branch` |
| "commit a fix to the PR branch" | `push-to-pull-request-branch` |
| "approve / request changes" | `submit-pull-request-review` |
| "inline review comment", "reply to review thread" | `create-pull-request-review-comment`, `reply-to-pull-request-review-comment`, `resolve-pull-request-review-thread` |
| "start or edit discussion", "close discussion" | `create-discussion`, `update-discussion`, `close-discussion` |
| "request reviewer", "hide comment" | `add-reviewer`, `hide-comment` |
| "create/update project", "project status update" | `create-project`, `update-project`, `create-project-status-update` |
| "update release", "upload release asset" | `update-release`, `upload-asset` |
| "trigger another workflow", "dispatch to workflow", "run another workflow" | `dispatch-workflow` |
| "create/auto-fix code scan alert" | `create-code-scanning-alert`, `autofix-code-scanning-alert` |
| "start an agent session", "assign to an agent" | `create-agent-session`, `assign-to-agent` |
| "store persistent memory comment" | `comment-memory` |
| "link a sub-issue" | `link-sub-issue` |
| "add labels", "remove labels" | `add-labels`, `remove-labels` |
| "replace a label with another" | `replace-label` |
| "nothing visible", "just analyze" | no safe outputs required |

### Network Mapping

| User says... | Maps to |
|---|---|
| "calls an external API" | ask for exact FQDN/wildcard, then add to `network.allowed` |
| "reads GitHub data / clones repos" | include `github` in `network.allowed` |
| "uses GitHub Actions artifacts or cache" | include `github-actions` in `network.allowed` |
| "installs npm packages" | include `node` in `network.allowed` |
| "runs pip install" | include `python` in `network.allowed` |
| "builds Go code" | include `go` in `network.allowed` |
| "installs gems / uses Bundler" | include `ruby` in `network.allowed` |
| "runs cargo build" | include `rust` in `network.allowed` |
| "uses NuGet / .NET restore" | include `dotnet` in `network.allowed` |
| "builds with Maven / Gradle" | include `java` in `network.allowed` |
| "uses Docker / pulls container images / pushes to GHCR" | include `containers` in `network.allowed` |
| "runs Playwright browser tests" | include `playwright` in `network.allowed` |
| "runs apt install / yum / apk" | include `linux-distros` in `network.allowed` |
| "uses Terraform / HashiCorp registry" | include `terraform` in `network.allowed` |
| "connects to localhost / loopback / local services" | include `local` in `network.allowed` |
| "uses Swift Package Manager" | include `swift` in `network.allowed` |
| "uses Composer / PHP packages" | include `php` in `network.allowed` |
| "uses pub.dev / Dart packages" | include `dart` in `network.allowed` |
| "uses Hackage / Haskell packages" | include `haskell` in `network.allowed` |
| "uses CPAN / Perl packages" | include `perl` in `network.allowed` |
| "serves or loads web fonts" | include `fonts` in `network.allowed` |
| "uses Deno or JSR packages" | include `deno` in `network.allowed` |
| "uses Elixir / Hex packages" | include `elixir` in `network.allowed` |
| "uses Bazel build" | include `bazel` in `network.allowed` |
| "uses Clojure / Clojars packages" | include `clojure` in `network.allowed` |
| "uses Julia packages" | include `julia` in `network.allowed` |
| "uses Kotlin / JetBrains packages" | include `kotlin` in `network.allowed` |
| "uses LuaRocks / Lua packages" | include `lua` in `network.allowed` |
| "uses node CDNs (jsdelivr, unpkg)" | include `node-cdns` in `network.allowed` |
| "uses OPAM / OCaml packages" | include `ocaml` in `network.allowed` |
| "uses PowerShell Gallery" | include `powershell` in `network.allowed` |
| "uses R / CRAN packages" | include `r` in `network.allowed` |
| "uses sbt / Scala packages" | include `scala` in `network.allowed` |
| "uses Zig packages" | include `zig` in `network.allowed` |
| "uses Renovate, Codecov, or other CI tools" | include `dev-tools` in `network.allowed` |
| "uses Chrome / Chromium downloads" | include `chrome` in `network.allowed` |
| "uses LaTeX / TeX / MiKTeX" | include `latex` in `network.allowed` |
| "uses Lean theorem prover" | include `lean` in `network.allowed` |
| "builds Python packages from source" | include `python-native` in `network.allowed` |
| "no external access" | `network.allowed: [defaults]` (or `[]` if explicitly zero network) |

### Tool Mapping

| User says... | Maps to |
|---|---|
| "read GitHub issues/PRs/workflows" | `tools.github` with `mode: gh-proxy` and minimal `toolsets` |
| "use full MCP server/tool definitions" | `tools.github` with `mode: local` |
| "use other MCP servers but keep token cost down" | `tools.cli-proxy: true` (hybrid CLI-proxy mode) |
| "edit files" | `edit` tool (default unless restricted) |
| "run commands/tests" | `bash` tool (default unless restricted) |
| "browse web pages/docs" | `web-fetch` and/or `web-search` |
| "test UI flows" | `playwright` |

### Pattern Heuristics

| User says... | Recommended named pattern |
|---|---|
| "triage issues automatically" | `IssueOps` |
| "run on /commands with human approval loops" | `ChatOps` |
| "run every weekday and keep improving" | `DailyOps` |
| "monitor workflow failures and trends" | `MonitorOps` |
| "process a big backlog in chunks" | `BatchOps` |
| "run manually with input parameters" | `DispatchOps` |
| "apply a label-based workflow" | `LabelOps` |
| "operate across multiple repositories" | `MultiRepoOps` |
| "coordinate multiple sub-agents" | `Orchestration` |
| "manage project board items" | `ProjectOps` |
| "research, plan, and assign issues" | `ResearchPlanAssignOps` |
| "self-correcting / retry on failure" | `CorrectionOps` |
| "run in a side/fork repo" | `SideRepoOps` |
| "write a spec before implementing" | `SpecOps` |
| "A/B test workflow variants" | `TrialOps` |
| "process items from a queue" | `WorkQueueOps` |
| "deterministic, no LLM needed" | `DeterministicOps` |
| "manage from a central repo" | `CentralRepoOps` |
| "track work via GitHub Projects" | `Monitoring with Projects` |

### Integration Auth Mapping

When the user names a third-party service or MCP server:

1. Confirm whether native tool, MCP server, or safe-output job is the right integration path.
2. Look up the integration's auth requirements and required scopes before finalizing the design.
3. Provide a concrete setup checklist with:
   - required GitHub Actions secrets (names to create)
   - workflow env variables that consume those secrets
   - minimum token scopes/permissions needed

Output format to use:

```text
Integration auth setup:
- <service-or-mcp>: <purpose>
  - Secrets to create: <SECRET_NAME>, <SECRET_NAME>
  - Workflow env vars: <ENV_VAR>=${{ secrets.<SECRET_NAME> }}
  - Required scopes/permissions: <least-privilege scopes>
```

Never suggest committing plaintext tokens.

### Data Strategy Mapping

| User says... | Maps to |
|---|---|
| "analyze PRs", "review issues", "check status" | add `steps:` that pre-fetch with `gh` + `jq` |
| "read the diff", "look at changed files" | add `steps:` using `gh pr diff` or `gh pr view --json files` |
| "search for patterns across repos" | add `steps:` using `gh search` + `jq` filters |
| "just respond to a comment" | no pre-fetch needed (event payload is enough) |
| "process each item individually" | suggest sub-agent pattern with `model: small` |

## Token Optimization Defaults

Apply these defaults unless the user explicitly asks otherwise:

1. Use DataOps by default for GitHub reads: pre-fetch/aggregate with `gh` + `jq` in `steps:`, store compact JSON in `/tmp/gh-aw/data/`, and point the prompt to those files (see `.github/aw/token-optimization.md` for details).
2. Keep tool surface minimal: default to `tools.github.mode: gh-proxy`, include only required toolsets, and prefer `bash` + `gh` for simple reads.
3. For batch workloads, split items into compact data and suggest sub-agent processing with `model: small`.
4. Keep prompts compact: concise imperative instructions, explicit file paths, single-line `noop` guidance, and stable instructions before dynamic content.

## Progressive Disclosure Rules

1. Never dump all options at once; ask one targeted question at a time.
2. Skip questions when answers are inferable from prior user statements.
3. Offer smart defaults and request confirmation instead of over-questioning.
4. Ask at most 5 questions before presenting a summary; then ask "anything else?" if needed.
5. Detect done signals (`that's it`, `looks good`, `generate it`) and proceed to generation.

## Confirmation Format

Use this exact structure:

```text
📋 Proposed workflow:
- Name: <workflow-id>
- Trigger: <event + key options>
- Engine: <engine or default>
- Tools: <tool summary>
- Safe outputs: <list or none>
- Network: <allowed summary>
- Integrations/Auth: <service/mcp + required secrets/env vars>
- Deployment: <GitHub.com or GHEC/GHES details>
- Intent: <one-sentence task>
```

Then ask: **"Ready to generate, or want to adjust anything?"**

## Generation Template

After confirmation, generate one workflow file using the same skeleton style as `.github/aw/create-agentic-workflow.md`.

```markdown
---
emoji: <emoji>
description: <brief description>
on:
  <trigger config>
permissions:
  contents: read
  issues: read
  pull-requests: read
tools:
  github:
    mode: gh-proxy
    toolsets: [default]
steps:
  - name: <optional data prefetch>
    run: |
      mkdir -p /tmp/gh-aw/data
      <gh + jq commands that produce compact JSON>
safe-outputs:
  <safe-output-types-if-needed>
network:
  allowed:
    - defaults
    - <additional entries if needed>
---

# <Workflow Name>

## Task

<clear instructions tied to trigger context>
If `steps:` includes pre-fetch commands, read the resulting `/tmp/gh-aw/data/*.json` files instead of broad live re-fetches.

## Safe Outputs

- Use configured safe outputs for all visible write actions.
- Call `noop` with a short reason when no action is needed.
```

## Validation Checklist

Before final output, run this internal self-check:

- [ ] Agent job permissions remain read-only (writes only via safe outputs)
- [ ] `safe-outputs:` covers every write action mentioned in prompt/instructions
- [ ] Network access is scoped; avoid blanket wildcard entries
- [ ] Trigger matches the user's intended activation event
- [ ] Prompt instructs agent to call `noop` when no action is needed
- [ ] Unnecessary defaults are omitted (for example `engine: copilot`)
- [ ] If reading GitHub data, `steps:` pre-fetches compact JSON (DataOps)
- [ ] `tools.github.mode` is `gh-proxy` unless broader MCP toolsets are explicitly needed
- [ ] Only required toolsets are listed (avoid blanket toolset lists)
- [ ] Prompt references specific pre-computed file paths
- [ ] For batch processing (>5 items), sub-agent pattern is suggested
- [ ] For each third-party service/MCP integration, required secrets/env vars are listed
- [ ] Auth guidance includes least-privilege token scope recommendations
- [ ] For GHEC/GHES deployments, `engine.api-target` and GHES compatibility guidance are included when needed

## References (load only when needed)

In-repo references:
- `.github/aw/syntax.md` (index → `.github/aw/syntax-core.md`, `.github/aw/syntax-agentic.md`, `.github/aw/syntax-tools-imports.md`)
- `.github/aw/safe-outputs.md` (index → `.github/aw/safe-outputs-content.md`, `.github/aw/safe-outputs-management.md`, `.github/aw/safe-outputs-automation.md`, `.github/aw/safe-outputs-runtime.md`)
- `.github/aw/network.md`
- `.github/aw/patterns.md`
- `.github/aw/subagents.md`
- `.github/aw/token-optimization.md`
- `.github/aw/triggers.md`
- `.github/aw/create-agentic-workflow.md`

Portable HTTPS references:
- `https://github.com/github/gh-aw/blob/main/.github/aw/syntax.md` (index → `.../syntax-core.md`, `.../syntax-agentic.md`, `.../syntax-tools-imports.md`)
- `https://github.com/github/gh-aw/blob/main/.github/aw/safe-outputs.md` (index → `.../safe-outputs-content.md`, `.../safe-outputs-management.md`, `.../safe-outputs-automation.md`, `.../safe-outputs-runtime.md`)
- `https://github.com/github/gh-aw/blob/main/.github/aw/network.md`
- `https://github.com/github/gh-aw/blob/main/.github/aw/patterns.md`
- `https://github.com/github/gh-aw/blob/main/.github/aw/subagents.md`
- `https://github.com/github/gh-aw/blob/main/.github/aw/token-optimization.md`
- `https://github.com/github/gh-aw/blob/main/.github/aw/triggers.md`
- `https://github.com/github/gh-aw/blob/main/.github/aw/create-agentic-workflow.md`
