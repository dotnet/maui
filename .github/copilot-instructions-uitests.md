# UI Testing Guidelines for .NET MAUI

**appliesTo:** Controls/tests

## Overview

This document provides specific guidance for GitHub Copilot when writing UI tests for the .NET MAUI repository.

## UI Test Platform Coverage

### Running Tests on All Applicable Platforms

**IMPORTANT:** UI tests should ALWAYS be run on all platforms that they apply to, unless there is a specific technical limitation that prevents it.

### Default Behavior

- **DO NOT** use platform-specific conditional compilation directives (`#if ANDROID`, `#if IOS`, etc.) unless there is a specific reason
- Tests in the `TestCases.Shared.Tests` project should run on both iOS and Android by default
- The test infrastructure automatically handles platform detection

### When to Use Platform-Specific Directives

Only use `#if ANDROID`, `#if IOS`, or other platform directives when:

1. **Platform-specific API is being tested** - The test validates behavior that only exists on one platform
2. **Known platform limitation** - There is a documented bug or limitation that prevents the test from running on a specific platform
3. **Different expected behavior** - The platforms are expected to behave differently for valid reasons

### Example: Correct Approach

```csharp
[Test]
[Category(UITestCategories.SafeAreaEdges)]
public void SoftInputBehaviorTest()
{
    // This test runs on both iOS and Android
    App.WaitForElement("ContentGrid");
    // Test code...
}
```

### Example: Incorrect Approach (Don't Do This)

```csharp
#if ANDROID
[Test]
[Category(UITestCategories.SafeAreaEdges)]
public void SoftInputBehaviorTest()
{
    // This unnecessarily limits the test to Android only
    App.WaitForElement("ContentGrid");
    // Test code...
}
#endif
```

## UI Test Structure

### Test Organization

- Tests are located in `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`
- Corresponding UI pages are in `src/Controls/tests/TestCases.HostApp/Issues/`
- Each test should inherit from `_IssuesUITest`

### Test Naming

- Follow the pattern: `IssueXXXXX.cs` for test files
- Match the corresponding XAML file in TestCases.HostApp

### Required Components

**CRITICAL: UITests require code in TWO separate projects:**

1. **HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`)
   - Create the actual UI page (XAML and code-behind)
   - Use proper `AutomationId` attributes on interactive controls
   - Follow naming: `IssueXXXXX.xaml` and `IssueXXXXX.xaml.cs`

2. **NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Create Appium-based NUnit tests inheriting from `_IssuesUITest`
   - Use `AutomationId` values to locate and interact with UI elements
   - Follow naming: `IssueXXXXX.cs`
   - Include appropriate `[Category(UITestCategories.XYZ)]` attributes

### Test Categories

- Use appropriate categories from `UITestCategories`
- Only one `[Category]` attribute per test
- Pick the most specific category that applies

### AutomationId Best Practices

- Always use unique, descriptive `AutomationId` values
- Reference the same `AutomationId` in both XAML and test code
- Use PascalCase for AutomationId values

## Verification and Building

Before committing UI tests:
- Compile both the HostApp project and TestCases.Shared.Tests project
- Verify AutomationId references match between XAML and test code
- Ensure tests follow established naming and inheritance patterns
- Verify tests run on all applicable platforms (iOS and Android)

## Common Patterns

### Waiting for Elements

```csharp
App.WaitForElement("AutomationId");
```

### Interacting with Elements

```csharp
App.Tap("AutomationId");
App.FindElement("AutomationId").GetText();
var rect = App.WaitForElement("AutomationId").GetRect();
```

### Assertions

```csharp
Assert.That(actualValue, Is.EqualTo(expectedValue).Within(tolerance));
Assert.That(rect.Height, Is.LessThanOrEqualTo(maxHeight));
```

## Remember

- Tests should be platform-agnostic by default
- Only use platform-specific directives when absolutely necessary
- Always test on all applicable platforms to ensure consistent behavior
- Document any platform-specific limitations with clear comments
