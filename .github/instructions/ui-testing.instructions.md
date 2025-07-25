---
applyTo: 
  - "src/Controls/tests/TestCases.HostApp/**/*.cs"
  - "src/Controls/tests/TestCases.iOS.Tests/**/*.cs"
  - "src/Controls/tests/TestCases.Android.Tests/**/*.cs"
  - "src/Controls/tests/TestCases.Shared.Tests/**/*.cs"
  - "src/Controls/tests/TestCases.Mac.Tests/**/*.cs"
  - "src/Controls/tests/TestCases.WinUI.Tests/**/*.cs"
---

# UI Testing Guidelines for .NET MAUI

When adding UI tests to validate visual behavior and user interactions, follow this two-part pattern:

**CRITICAL: UITests require code in TWO separate projects that must BOTH be implemented:**

1. **HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`)
   - Create the actual UI page that demonstrates the feature or reproduces the issue
   - Use XAML with proper `AutomationId` attributes on interactive controls for test automation
   - Follow naming convention: `IssueXXXXX.xaml` and `IssueXXXXX.xaml.cs`
   - Ensure the UI provides clear visual feedback for the behavior being tested

2. **NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Create corresponding Appium-based NUnit tests that inherit from `_IssuesUITest`
   - Use the `AutomationId` values to locate and interact with UI elements
   - Follow naming convention: `IssueXXXXX.cs` (matches the HostApp file)
   - Include appropriate `[Category(UITestCategories.XYZ)]` attributes
   - Test should validate expected behavior through UI interactions and assertions

## UI Test Pattern Example

```csharp
// In TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs
public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Description of the issue being tested";
    
    public IssueXXXXX(TestDevice device) : base(device) { }
    
    [Test]
    [Category(UITestCategories.Layout)] // or appropriate category
    public void TestMethodName()
    {
        App.WaitForElement("AutomationId");
        App.Tap("AutomationId");
        // Add assertions to verify expected behavior
    }
}
```

## Before committing UI tests

- Compile both the HostApp project and TestCases.Shared.Tests project to ensure no build errors
- Verify AutomationId references match between XAML and test code
- Ensure tests follow the established naming and inheritance patterns

## UI Test API Reference

For comprehensive API documentation and examples, see the detailed guide at:
`src/Controls/tests/TestCases.Shared.Tests/AppiumTestGuide.md`

This guide includes:
- Complete IApp and IUIElement API reference
- All test base classes (_IssuesUITest, UITest, etc.)
- Visual regression testing patterns
- Platform-specific testing strategies
- Step-by-step instructions for creating new tests
- Best practices and common pitfalls