# Skill Evaluation Guide

How to evaluate skills in the dotnet/maui repository using the multi-agent evaluation framework.

## Available Evaluation Tools

### 1. Anthropic `skill-creator` (Trigger Evaluation)

Located at `anthropics/skills/skills/skill-creator/` — a meta-skill for creating, evaluating, and optimizing skills.

**Key scripts:**

| Script | Purpose |
|--------|---------|
| `run_eval.py` | Tests whether a skill's description causes Claude to trigger for a set of queries |
| `improve_description.py` | AI-powered iterative description optimization |
| `run_loop.py` | Combines eval + improve loop with train/test split to prevent overfitting |
| `quick_validate.py` | Fast structural validation of SKILL.md |
| `generate_report.py` | HTML report generation for human review |

**When to use:** Testing trigger accuracy — does the skill activate for the right prompts and NOT activate for wrong ones?

**How to discover:** Always enumerate Anthropic's available skills before claiming tools don't exist:
```bash
gh api repos/anthropics/skills/contents/skills --jq '.[].name'
```

### 2. dotnet/skills `skill-validator` (Behavioral Evaluation)

The `skill-validator` tool from `dotnet/skills` runs eval.yaml scenarios with A/B comparison.

**Key commands:**

| Command | Purpose |
|---------|---------|
| `skill-validator check` | Static analysis of SKILL.md + eval.yaml |
| `skill-validator evaluate` | A/B comparison: baseline (no skill) vs skilled (with SKILL.md) |

**When to use:** Testing behavioral compliance — does the agent follow SKILL.md instructions correctly? Does the skill improve task completion?

### 3. Using Both Tools Together

For a complete evaluation, run BOTH:

| Dimension | Tool | What It Tests |
|-----------|------|---------------|
| **Trigger accuracy** | Anthropic `skill-creator` `run_eval.py` | Does the skill activate for the right prompts? |
| **Behavioral compliance** | dotnet/skills `skill-validator evaluate` | Does the agent follow instructions correctly? |
| **Static analysis** | dotnet/skills `skill-validator check` | Are eval.yaml assertions consistent? |
| **Prompt quality** | Manual review (Anthropic evaluator role) | Is the SKILL.md clear, actionable, unambiguous? |

## Multi-Agent Evaluation Protocol

### Worker Roles

| Worker | Role | Focus |
|--------|------|-------|
| **Dotnet Validator** | Empirical testing | Runs `skill-validator evaluate`, measures A/B improvement |
| **Anthropic Evaluator** | Prompt engineering review | Reviews SKILL.md clarity, trigger quality, assertion/rubric conflicts |
| **Eval Generator** | Scenario authoring | Writes eval.yaml scenarios, mines production data |

### Round Protocol (Max 4 rounds)

```
Round 1: All 3 workers evaluate fresh → flag everything
Round 2: Fixes applied → workers review deltas only  
Round 3: Final consensus → KEEP/IMPROVE/REMOVE
Round 4: (only if IMPROVE) → last fixes → KEEP/REMOVE
```

### Consensus Rule

- **KEEP** requires both evaluators to say KEEP, or one KEEP + one IMPROVE
- **IMPROVE** if evaluators disagree or both say IMPROVE
- **REMOVE** if either evaluator says REMOVE with strong evidence

## Mandatory Pre-Evaluation Checklist

Before claiming any evaluation tool or capability does NOT exist, verify:

1. **Check `anthropics/skills/skills/`** — list ALL skills including meta-skills:
   ```bash
   gh api repos/anthropics/skills/contents/skills --jq '.[].name'
   ```
   Key meta-skill: `skill-creator` (creates, evaluates, and optimizes skills)

2. **Check `skills-ref` npm package** — structural validation:
   ```bash
   npm info skills-ref
   ```

3. **Check Claude Code built-in commands** — skill management:
   ```bash
   claude --help 2>&1 | Select-String "skill"
   ```

4. **Check `dotnet/skills` tooling** — behavioral evaluation:
   ```bash
   skill-validator --help
   ```

5. **NEVER claim "tool X doesn't exist" without checking all 4 sources first.**

## Eval Quality Standards

### Assertion/Rubric Conflict Check

Before merging any eval.yaml, verify for EVERY scenario:
- Can the agent satisfy ALL assertions AND ALL rubric items simultaneously?
- Do `output_not_contains` assertions ban terms the rubric requires the agent to mention?
- Are assertions specific enough to avoid false negatives?

### Scenario Coverage Requirements

Every eval.yaml should include at minimum:
- 1+ Happy path scenario
- 1+ Negative trigger scenario (with `expect_activation: false`)
- 2+ Edge case scenarios
- 2+ Regression scenarios (anti-patterns the agent must avoid)

### Eval Design Principles

1. **Rubric-first:** Default to rubric items for behavioral checks. Only use `output_contains`/`output_not_contains` for concrete anti-patterns.
2. **Realistic timeouts:** 900s for scenarios involving builds/tests, 120s for interpretation questions, 60s for negative triggers.
3. **Prompt diversity:** Vary phrasing across scenarios — don't use the same sentence structure for every prompt.
4. **Production-grounded:** Mine real PR sessions for scenario prompts and failure modes.

## Related Resources

- [Issue #34814](https://github.com/dotnet/maui/issues/34814) — Eval lifecycle tracking issue
- [SkillsBench paper](https://arxiv.org/abs/2602.12670) — Academic benchmark for Agent Skills
- [BFCL](https://gorilla.cs.berkeley.edu/leaderboard.html) — Berkeley Function Calling Leaderboard
- [Agent Skills Spec](https://agentskills.io/specification) — Open standard for skill format
- [Anthropic skill-creator](https://github.com/anthropics/skills/tree/main/skills/skill-creator) — Official Anthropic skill evaluation tooling
