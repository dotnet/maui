# Common Compile Error Patterns

Reference guide for resolving compile errors during fix attempts.

## Error Pattern Quick Reference

| Error Pattern | Likely Cause | Solution |
|--------------|--------------|----------|
| `CS0234: namespace doesn't exist` | Missing using or namespace collision | Add using statement or type alias |
| `CS0104: ambiguous reference` | Type exists in multiple namespaces | Use type alias (e.g., `using AView = Android.Views.View;`) |
| `CS0103: name doesn't exist` | Missing reference or typo | Check spelling, add using statement |
| `CS0246: type not found` | Missing assembly reference or using | Add using or check project references |
| `CS1061: member doesn't exist` | Wrong type or API signature | Verify type and API documentation |

## Example Iteration

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
