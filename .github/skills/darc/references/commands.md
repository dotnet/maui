# Darc Command Reference

## Table of Contents

- [Asset and Build Commands](#asset-and-build-commands)
- [Dependency Commands](#dependency-commands)
- [Channel Commands](#channel-commands)
- [Subscription Commands](#subscription-commands)
- [Graph Commands](#graph-commands)
- [VMR Commands](#vmr-commands)

---

## Asset and Build Commands

### get-asset

Get information about an asset by name, version, channel, or age.

```bash
# Basic usage
darc get-asset --name 'Microsoft.Maui.Controls'

# With version
darc get-asset --name 'Microsoft.Maui.Controls' --version 9.0.60

# Filter by channel and recency
darc get-asset --name 'Microsoft.Extensions.Logging.Abstractions' --channel ".NET 9 Dev" --max-age 7
```

Output includes: Version, Repository, Branch, Commit, Build Number, Date, Build Link, BAR Build Id, Channels.

### get-build

Retrieve a specific build by BAR ID.

```bash
darc get-build --id 47814
```

Find BAR build ID in the "Publish Build Assets" task of the "Publish to Build Asset Registry" job logs.

### get-latest-build

Get latest builds matching criteria.

```bash
darc get-latest-build --repo https://github.com/dotnet/maui
darc get-latest-build --repo https://github.com/dotnet/runtime --channel ".NET 9 Dev"
```

### add-build-to-channel

⚠️ **Requires user confirmation** — triggers promotion builds that publish packages.

Assign a build to a channel manually.

```bash
darc add-build-to-channel --id 65256 --channel ".NET 9 Dev"

# Skip publishing (if assets already available)
darc add-build-to-channel --id 65256 --channel ".NET 9 Dev" --skip-assets-publishing
```

---

## Dependency Commands

### get-dependencies

List dependencies in local repo's Version.Details.xml.

```bash
darc get-dependencies
darc get-dependencies --name "Microsoft.NETCore.App"
```

### add-dependency

⚠️ **Requires user confirmation** — mutates Version.Details.xml.

Add new dependency to Version.Details.xml.

```bash
darc add-dependency --name 'Microsoft.NETCore.App' --type 'product' --repo https://github.com/dotnet/runtime
```

Types: `product` (shipped to customers) or `toolset` (build-time only).

### update-dependencies

⚠️ **Requires user confirmation** — mutates Version.Details.xml.

Update local dependencies from a channel or build.

```bash
# From channel
darc update-dependencies --channel ".NET 9 Dev"

# Specific package
darc update-dependencies --channel ".NET 9 Dev" --name "Microsoft.Maui.Controls"

# From specific build
darc update-dependencies --id 123456

# Preview changes (safe, no confirmation needed)
darc update-dependencies --channel ".NET 9 Dev" --dry-run

# Skip coherency updates
darc update-dependencies --id 123456 --no-coherency-updates
```

### verify

Verify dependency information is correct.

```bash
darc verify
```

---

## Channel Commands

### get-channels

List all available channels.

```bash
darc get-channels
```

### add-channel

🚫 **PROHIBITED** — Never run this command. Channel creation is an infrastructure decision.

### delete-channel

🚫 **PROHIBITED** — Never run this command. Channel deletion is an infrastructure decision.

### get-default-channels

List default channel mappings (repo+branch → channel).

```bash
darc get-default-channels
darc get-default-channels --source-repo maui
darc get-default-channels --channel ".NET 9 Dev"
```

### add-default-channel

⚠️ **Requires user confirmation** — changes channel mappings that affect dependency flow.

Map repo+branch to auto-apply builds to a channel.

```bash
darc add-default-channel --channel ".NET 9 Dev" --branch refs/heads/main --repo https://github.com/dotnet/maui
```

### delete-default-channel

⚠️ **Requires user confirmation** — breaks dependency flow mappings.

Remove a default channel mapping.

```bash
darc delete-default-channel --channel ".NET 9 Dev" --branch refs/heads/main --repo https://github.com/dotnet/maui
```

### default-channel-status

⚠️ **Requires user confirmation** — can silently break dependency flow.

Enable/disable a default channel mapping.

```bash
darc default-channel-status --disable --id 192
darc default-channel-status --enable --id 192
```

---

## Subscription Commands

Subscriptions define automatic dependency flow between repositories.

### get-subscriptions

List subscriptions.

```bash
darc get-subscriptions
darc get-subscriptions --source-repo arcade --target-repo runtime
darc get-subscriptions --target-repo https://github.com/dotnet/maui
```

### add-subscription

⚠️ **Requires user confirmation** — creates automated dependency flow with potential auto-merge.

Create subscription (opens interactive editor by default).

```bash
# Interactive mode
darc add-subscription

# Non-interactive
darc add-subscription --channel ".NET 9 Dev" \
  --source-repo https://github.com/dotnet/runtime \
  --target-repo https://github.com/dotnet/sdk \
  --target-branch main \
  --update-frequency everyBuild \
  --standard-automerge -q
```

Update frequencies: `everyBuild`, `everyDay`, `none`

Merge policies:
- `--standard-automerge` - Recommended, includes AllChecksSuccessful + NoRequestedChanges
- `--all-checks-passed` - All PR checks must pass

### update-subscription

⚠️ **Requires user confirmation** — modifies automated dependency flow.

Modify existing subscription.

```bash
darc update-subscription --id <subscription-id>
```

### delete-subscriptions

⚠️ **Requires user confirmation** — removes automated dependency flow.

Delete subscription(s).

```bash
darc delete-subscriptions --id <subscription-id>
```

### subscription-status

⚠️ **Requires user confirmation** — enables/disables automated dependency flow.

Enable/disable a subscription.

```bash
darc subscription-status --id <subscription-id> --disable
darc subscription-status --id <subscription-id> --enable
```

### trigger-subscriptions

⚠️ **Requires user confirmation** — manually triggers a dependency update.

Manually trigger a subscription update.

```bash
darc trigger-subscriptions --id <subscription-id>
```

---

## Graph Commands

### get-dependency-graph

Build full repository dependency graph from Version.Details.xml.

```bash
# From current repo
darc get-dependency-graph

# Flat mode (unique repo+sha only)
darc get-dependency-graph --flat

# Include toolset dependencies
darc get-dependency-graph --include-toolset

# GraphViz format
darc get-dependency-graph --graphviz
```

### get-flow-graph

Get inter-repository dependency flow graph in GraphViz format.

```bash
darc get-flow-graph --channel ".NET 9 Dev"
```

### gather-drop

🚫 **PROHIBITED** — Bulk artifact download is not needed in agent context.

---

## VMR Commands

Virtual Mono Repo (VMR) commands use format `darc vmr <command>`:

### vmr get-version

Get current SHA of a repository in the VMR.

```bash
darc vmr get-version
```

### vmr diff

Diff the VMR and product repositories.

```bash
darc vmr diff
```

### vmr initialize

⚠️ **Requires user confirmation** — modifies VMR content.

Initialize new repo(s) into the VMR.

```bash
darc vmr initialize
```

### vmr backflow

⚠️ **Requires user confirmation** — flows source from VMR into a target repository.

```bash
darc vmr backflow
```

### vmr forwardflow

⚠️ **Requires user confirmation** — flows source from a repository into the VMR.

```bash
darc vmr forwardflow
```

---

## Repository Policies

### get-repository-policies

Get merge policies for a repository branch.

```bash
darc get-repository-policies --repo https://github.com/dotnet/runtime --branch main
```

### set-repository-policies

🚫 **PROHIBITED** — Merge policy changes affect repository security and must be made by infrastructure owners.

---

## Goals

### set-goal

Set goal build time for a definition on a channel.

```bash
darc set-goal --minutes 38 --definition-id 6 --channel ".NET 9 Dev"
```

### get-goal

Get goal build time.

```bash
darc get-goal --definition-id 6 --channel ".NET 9 Dev"
```
