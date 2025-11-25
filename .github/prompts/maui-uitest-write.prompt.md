---
description: "Write new UI tests for .NET MAUI with proper conventions and best practices"
name: maui-uitest-write
agent: uitest-coding-agent
---

# UI Test Writer

Writes high-quality UI tests for .NET MAUI following established conventions.

## When to Use

✅ **Use this prompt when:**
- "Write a UI test for issue #12345"
- "Add test coverage for..."
- "Create automated test for..."
- Need to write NEW test files

❌ **DON'T use this prompt when:**
- "Test this PR" → Use `@workspace /maui-sandbox-test` instead
- "Review this PR" → Use `@workspace /maui-pr-reviewer` instead
- "Investigate issue #12345" → Use `@issue-resolver` instead
- Only need manual verification → Use `@workspace /maui-sandbox-test` instead

## Usage Examples

```
@workspace /maui-uitest-write Create UI test for issue #12345
@workspace /maui-uitest-write Add test for Button click updating Label text
@workspace /maui-uitest-write Write Android SafeArea keyboard test
@workspace /maui-uitest-write Write test for issue #32479 using the reproduction steps
```

## What This Does

1. ✅ Creates TWO required files (HostApp XAML + NUnit test)
2. ✅ Follows all naming conventions and best practices
3. ✅ Adds proper `AutomationId` attributes for test automation
4. ✅ Selects correct test category from `UITestCategories`
5. ✅ Runs test locally to verify it works
6. ✅ Ready to commit

**Timeline:** 3-5 minutes for complete, verified test

## What You'll Get

- 📄 **HostApp XAML page** - UI that demonstrates/reproduces the behavior
- 🧪 **Appium-based NUnit test** - Validates the expected behavior
- ✅ **Verified test** - Passes on at least one platform (iOS or Android)
- 📋 **Summary** - What the test validates and how to run it

## Test Requirements

**Two-Project Structure:**
- HostApp XAML page: `TestCases.HostApp/Issues/IssueXXXXX.xaml[.cs]`
- NUnit test: `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

**Essential Elements:**
- All interactive elements need `AutomationId` attributes
- Test class inherits from `_IssuesUITest`
- ONE `[Category]` attribute per test
- `[Issue()]` attribute required on HostApp page
- Descriptive test method names

**Platform Coverage:**
- Tests run on ALL platforms by default
- Only use platform-specific directives when technically necessary

## How Tests Are Validated

The agent automatically:
- 🔧 Builds and deploys TestCases.HostApp
- 🧪 Runs the test with BuildAndRunHostApp.ps1 script
- 📊 Captures all logs and results
- ✅ Reports pass/fail with evidence

You get verification that files compile, deploy, and tests pass before committing.

## Documentation

**Complete guides:**
- [UI Testing Guide](../../docs/UITesting-Guide.md) - Comprehensive UI test documentation
- [uitest-coding-agent](../agents/uitest-coding-agent.md) - Full agent instructions with templates and patterns
- [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs) - All available test categories

**Quick references:**
- [Common Testing Patterns](../instructions/common-testing-patterns.md) - Build and deployment commands
- [Appium Control Scripts](../instructions/appium-control.md) - Manual debugging helpers

## Notes

- Script handles all infrastructure (never run manual `dotnet test` or `adb` commands)
- Tests remain in HostApp for CI pipeline execution
- All logs captured automatically to organized directory
- Tests should be independent and not rely on other test state
