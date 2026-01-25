# Output Structure Reference

All try-fix artifacts are saved to: `CustomAgentLogsTmp/PRState/<PRNumber>/try-fix/attempt-<N>/`

## Required Files Per Attempt

| File | Description |
|------|-------------|
| `approach.md` | Brief description of the fix approach |
| `fix.diff` | Git diff of the changes made |
| `test-output.log` | Full test command output |
| `result.txt` | PASS or FAIL |
| `analysis.md` | Detailed analysis of why it worked/failed |

## Example Structure

```
CustomAgentLogsTmp/PRState/27847/try-fix/
├── attempt-1/
│   ├── approach.md
│   ├── fix.diff
│   ├── test-output.log
│   ├── result.txt
│   └── analysis.md
├── attempt-2/
│   └── ...
└── summary.md  # Overall summary of all attempts
```

## Setup Commands

```bash
# Auto-detect PR number
PR_NUMBER=$(git branch --show-current | sed -n 's/^pr-\([0-9]\+\).*/\1/p')
# Or: PR_NUMBER=$(gh pr view --json number -q .number 2>/dev/null)

# Determine attempt number and create directory
ATTEMPT_NUM=$(($(ls -d CustomAgentLogsTmp/PRState/$PR_NUMBER/try-fix/attempt-* 2>/dev/null | wc -l) + 1))
OUTPUT_DIR="CustomAgentLogsTmp/PRState/$PR_NUMBER/try-fix/attempt-$ATTEMPT_NUM"
mkdir -p "$OUTPUT_DIR"
```
