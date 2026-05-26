# Common Compile Error Patterns

Reference guide for resolving compile errors and test failures during fix attempts.

## Error Pattern Quick Reference

| Error Pattern | Likely Cause | Solution |
|--------------|--------------|----------|
| `CS0234: namespace doesn't exist` | Missing using or namespace collision | Add using statement or type alias |
| `CS0104: ambiguous reference` | Type exists in multiple namespaces | Use type alias (e.g., `using AView = Android.Views.View;`) |
| `CS0103: name doesn't exist` | Missing reference or typo | Check spelling, add using statement |
| `CS0246: type not found` | Missing assembly reference or using | Add using or check project references |
| `CS1061: member doesn't exist` | Wrong type or API signature | Verify type and API documentation |

## Handling Compile Errors

- Read error messages (CS#### codes)
- Fix the issue in your code
- DO NOT manually build - rerun the test command script

## Handling Test Failures

- Read test output carefully - what timeout? what exception?
- `TimeoutException` usually means app crashed or didn't launch properly
- Analyze your code for runtime bugs (null references, invalid calculations, etc.)
- Fix the bug and rerun the test command

## Example Iteration

```
Iteration 1: Test command → Compile error CS1061 → Fix method name → Rerun
Iteration 2: Test command → TimeoutException → Found double-scaling bug → Fix → Rerun  
Iteration 3: Test command → Tests PASS ✅
```

**Another example:**
```
Attempt #1: Implement OnLayoutChangeListener
  → Build: ❌ CS0104: 'View' is ambiguous reference
  → Diagnose: Microsoft.Maui.Controls.View vs Android.Views.View collision
  → Fix: Add `using AView = Android.Views.View;` at top of file
  → Build: ✅ Success
  → Continue to Step 6 (Run Tests)
```

## When to Give Up on Compile Errors

- After 3 iterations of trying to fix compile errors
- When the error indicates a fundamental flaw in the approach (not just a missing using)
- When the fix requires changes outside the target files
