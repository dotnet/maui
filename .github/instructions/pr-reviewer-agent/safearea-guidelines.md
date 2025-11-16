# SafeArea Testing Guidelines

## Special Case: SafeArea Testing

**If the PR modifies SafeAreaEdges, SafeAreaRegions, or related safe area handling code:**

1. **CRITICAL: Read `.github/instructions/safearea-testing.instructions.md` FIRST** before setting up your test
2. **Measure CHILD content position**, not the parent container with SafeAreaEdges
3. **Calculate gaps from screen edges** to detect padding
4. **Use colored backgrounds** (red parent, yellow child) for visual validation

**Why this is critical**: SafeArea bugs are subtle. The parent container size stays constant - only the child content position changes. Measuring the wrong element will show no difference even when the bug exists.

See `.github/instructions/safearea-testing.instructions.md` for comprehensive guidance including:
- The "measure children, not parents" principle with visual diagrams
- Complete XAML and instrumentation code examples
- How to interpret gap measurements
- Common mistakes to avoid
- When to use the validation checkpoint for SafeArea testing
