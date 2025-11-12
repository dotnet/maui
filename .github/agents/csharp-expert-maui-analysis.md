# C# Expert Agent Analysis for .NET MAUI Repository

## Executive Summary

This document analyzes how the generic C# Expert agent instructions apply to the .NET MAUI repository and identifies areas where MAUI-specific adaptations are needed. The analysis is based on examination of the MAUI codebase structure, conventions, and development patterns.

## Key Findings

### 1. Testing Framework Usage - REQUIRES MODIFICATION ⚠️

**Generic C# Expert Guidance:**
- States to "use the framework already in the solution (xUnit/NUnit/MSTest)"
- Treats all test frameworks as interchangeable options

**MAUI-Specific Reality:**
- **xUnit** is used for unit tests (e.g., `src/Controls/tests/Core.UnitTests/`)
- **NUnit** is used for UI tests with Appium (e.g., `src/Controls/tests/TestCases.Shared.Tests/`)
- The choice is NOT interchangeable - it's determined by test type

**Recommended Modification:**
```markdown
### Test Framework Selection for MAUI

MAUI uses different test frameworks for different test types:

1. **Unit Tests** - Use xUnit
   - Location: `*.UnitTests` projects
   - Examples: `Controls.Core.UnitTests`, `Controls.BindingSourceGen.UnitTests`
   - Standard xUnit patterns apply: `[Fact]`, `[Theory]` with `[InlineData]`

2. **UI Tests** - Use NUnit with Appium
   - Location: `TestCases.Shared.Tests` and platform-specific test projects
   - Inherit from `_IssuesUITest` base class
   - Use `[Test]` attribute and `[Category(UITestCategories.XXX)]`
   - Must create corresponding UI test page in `TestCases.HostApp/Issues/`
   - Naming convention: `IssueXXXXX.cs` where XXXXX is GitHub issue number

3. **Device Tests** - Use xUnit
   - Location: `*.DeviceTests` projects
   - Run on actual devices/simulators
```

### 2. Platform-Specific Code Patterns - REQUIRES ADDITION ➕

**Generic C# Expert Guidance:**
- Brief mention of "guard OS-specific APIs" for cross-platform code
- No specific patterns for multi-targeted code

**MAUI-Specific Reality:**
- Extensive use of platform-specific file extensions (`.Android.cs`, `.iOS.cs`, `.Windows.cs`, `.MacCatalyst.cs`)
- Platform-specific conditional compilation is pervasive
- Handler pattern with platform-specific implementations

**Recommended Addition:**
```markdown
### Platform-Specific Code Organization in MAUI

MAUI uses multiple strategies for platform-specific code:

1. **Platform-Specific File Extensions**
   - `ClassName.Android.cs` - Android-only implementation
   - `ClassName.iOS.cs` - iOS and MacCatalyst implementation
   - `ClassName.Windows.cs` - Windows-only implementation
   - `ClassName.MacCatalyst.cs` - MacCatalyst-specific implementation
   
   These files automatically compile only for their target platform.

2. **Conditional Compilation Directives**
   - Use `#if ANDROID`, `#if IOS`, `#if MACCATALYST`, `#if WINDOWS`, `#if TIZEN`
   - These are defined in platform-specific builds
   - Example: Platform type aliases at file top

3. **Handler Pattern**
   - Handlers bridge MAUI controls to platform native views
   - Use `IPropertyMapper` for property mapping
   - Use `CommandMapper` for command handling
   - Platform views defined via type aliases:
     ```csharp
     #if __IOS__ || MACCATALYST
     using PlatformView = UIKit.UIButton;
     #elif MONOANDROID
     using PlatformView = Google.Android.Material.ImageView.ShapeableImageView;
     #elif WINDOWS
     using PlatformView = Microsoft.UI.Xaml.Controls.Button;
     #endif
     ```

4. **Platform-Specific Folders**
   - `Android/`, `iOS/`, `MacCatalyst/`, `Windows/`, `Tizen/` folders
   - Organize platform implementations within these directories
```

### 3. Project Structure Conventions - REQUIRES ADDITION ➕

**Generic C# Expert Guidance:**
- Generic advice about following project conventions
- No specific guidance on complex multi-targeted projects

**MAUI-Specific Reality:**
- Complex solution with multiple TFMs (net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows)
- Custom build tasks that MUST be built first
- Specific directory structure for Controls, Core, Essentials

**Recommended Addition:**
```markdown
### MAUI-Specific Build Requirements

Before making any code changes:

1. **Check SDK Version**
   ```bash
   cat global.json | grep -A 1 '"dotnet"'
   dotnet --version  # Must match global.json
   ```

2. **Build Tasks First** (CRITICAL)
   ```bash
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   ```
   
   Failure to build tasks first will result in cryptic build errors.

3. **Project Structure**
   - `src/Core/` - Core MAUI framework (handlers, platform services)
   - `src/Controls/` - UI controls (Button, Label, ListView, etc.)
   - `src/Essentials/` - Device APIs (Battery, Compass, etc.)
   - `src/Controls/samples/` - Sample applications for testing
   
4. **Multi-Targeting Considerations**
   - Most projects target multiple platforms: `$(UseMauiTargets)`
   - Code must work across all targeted platforms or use platform guards
   - Properties like `$(_MauiTargetPlatformIsAndroid)` control platform-specific compilation
```

### 4. Async Programming - MOSTLY APPLIES ✅

**Generic C# Expert Guidance:**
- Comprehensive async/await guidance
- Cancellation token patterns
- ConfigureAwait guidance

**MAUI-Specific Reality:**
- UI-centric framework where async is critical (image loading, navigation, etc.)
- Main thread considerations for UI updates
- Platform-specific async patterns (e.g., iOS main thread requirements)

**Minor Addition Needed:**
```markdown
### Async in MAUI UI Code

- **UI Thread Requirements**: Platform UI updates must occur on main thread
  - Use `MainThread.BeginInvokeOnMainThread()` or `MainThread.InvokeOnMainThreadAsync()`
  - Applies especially to iOS and MacCatalyst
  
- **Async Lifecycle Methods**: Many MAUI lifecycle methods are synchronous
  - Use patterns like fire-and-forget with error handling
  - Or defer to async helper methods
  
- **Image Loading**: Always async
  - Handlers use `ImageSourcePartLoader` with async operations
  - Never block on image loading

- **Navigation**: Can be async or sync
  - Prefer `await Shell.Current.GoToAsync()` over sync `Navigation.PushAsync()`
```

### 5. Error Handling - MOSTLY APPLIES ✅

**Generic C# Expert Guidance:**
- Use `ArgumentNullException.ThrowIfNull`
- Precise exception types
- No silent catches

**MAUI-Specific Reality:**
- Follows .NET 10 conventions (modern C#)
- Nullable reference types enabled in most projects
- Platform exceptions need special handling

**Minor Addition Needed:**
```markdown
### Platform Exception Handling in MAUI

- **Platform-Specific Exceptions**: Catch and wrap platform exceptions
  ```csharp
  #if ANDROID
  try {
      // Android-specific code
  }
  catch (Java.Lang.Exception ex) {
      throw new InvalidOperationException("Android operation failed", ex);
  }
  #endif
  ```

- **Handler Exceptions**: Handlers should not throw
  - Log errors appropriately
  - Set sensible defaults
  - Platform views should remain in valid state
  
- **Nullable Reference Types**: Enabled in Core project
  - Use nullable annotations consistently
  - Avoid `!` operator unless absolutely certain
```

### 6. Code Design Rules - NEEDS CLARIFICATION 📝

**Generic C# Expert Guidance:**
- "DON'T add interfaces/abstractions unless used for external dependencies or testing"
- "Don't wrap existing abstractions"

**MAUI-Specific Reality:**
- Extensive use of interfaces for abstraction (IButton, ILabel, IView, etc.)
- Interfaces are the public API surface
- This is the CORRECT pattern for MAUI

**Recommended Clarification:**
```markdown
### Interface Usage in MAUI

MAUI's architecture REQUIRES interfaces:

1. **Control Interfaces** (e.g., `IButton`, `ILabel`, `IView`)
   - Define the contract between MAUI controls and platform handlers
   - These are the public API surface
   - Always create an interface for new controls

2. **Handler Interfaces** (e.g., `IButtonHandler`, `ILabelHandler`)
   - Allow platform-specific handler implementations
   - Required for the handler pattern

3. **When NOT to Create Interfaces**
   - Internal helper classes
   - Platform-specific implementations
   - One-off utilities
   
The generic C# Expert guidance about avoiding unnecessary interfaces applies to
helper/utility code, NOT to the core MAUI control and handler abstractions.
```

### 7. Testing Patterns - REQUIRES ADDITION ➕

**Generic C# Expert Guidance:**
- Generic AAA pattern
- One behavior per test
- Avoid disk I/O

**MAUI-Specific Reality:**
- Two-project UI test pattern (HostApp + Test project)
- Appium-based UI automation
- Platform-specific test execution

**Recommended Addition:**
```markdown
### MAUI UI Testing Pattern

UI tests in MAUI require TWO files:

1. **Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`)
   ```xaml
   <ContentPage xmlns="..." 
                x:Class="Maui.Controls.Sample.Issues.IssueXXXXX">
       <Button x:Name="TestButton" 
               AutomationId="TestButton"
               Text="Click Me" />
   </ContentPage>
   ```
   
   - Must include `AutomationId` on interactive elements
   - Inherit from `TestContentPage` or `ContentPage`
   - Include `[Issue(IssueTracker.Github, XXXXX, "Description", PlatformAffected.All)]`

2. **Test Class** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`)
   ```csharp
   public class IssueXXXXX : _IssuesUITest
   {
       public IssueXXXXX(TestDevice testDevice) : base(testDevice) { }
       
       public override string Issue => "Description";
       
       [Test]
       [Category(UITestCategories.Button)]
       public void TestMethodName()
       {
           App.WaitForElement("TestButton");
           App.Tap("TestButton");
           // Assertions
       }
   }
   ```
   
   - Inherit from `_IssuesUITest`
   - Use ONE `[Category]` attribute per test
   - Test runs on ALL platforms by default (no platform guards unless needed)

### Running UI Tests

**Android:**
```bash
dotnet build TestCases.HostApp -f net10.0-android -t:Run
export DEVICE_UDID=$(adb devices | grep device | awk '{print $1}' | head -1)
dotnet test TestCases.Android.Tests --filter "IssueXXXXX"
```

**iOS:**
```bash
UDID=$(xcrun simctl list devices available --json | jq -r '...')
xcrun simctl boot $UDID
xcrun simctl install $UDID path/to/app
export DEVICE_UDID=$UDID
dotnet test TestCases.iOS.Tests --filter "IssueXXXXX"
```
```

### 8. Performance Considerations - REQUIRES ADDITION ➕

**Generic C# Expert Guidance:**
- Generic performance advice
- "Optimize hot paths when measured"
- Span/Memory/pooling

**MAUI-Specific Reality:**
- Mobile performance constraints (memory, CPU, battery)
- Layout performance is critical
- Platform-specific performance considerations

**Recommended Addition:**
```markdown
### MAUI-Specific Performance Patterns

1. **Layout Performance**
   - Minimize layout passes
   - Avoid nested layouts when possible
   - Use absolute positioning for performance-critical scenarios
   - Cache computed sizes

2. **Mobile Memory Constraints**
   - Be aggressive about releasing image resources
   - Avoid large object allocations
   - Consider platform-specific memory pressure events

3. **Platform View Recycling**
   - Handlers reuse platform views when possible
   - Don't hold references to platform views
   - Clean up event handlers properly

4. **Startup Performance**
   - Lazy-load dependencies
   - Minimize work in Application constructor
   - Defer non-critical initialization

5. **Measure on Device**
   - Emulator performance != device performance
   - Test on lower-end devices
   - Monitor memory and CPU usage
```

## Summary of Recommendations

### High Priority Modifications

1. **Test Framework Guidance** - Critical to clarify xUnit vs NUnit usage
2. **Platform-Specific Patterns** - Essential for MAUI development
3. **UI Test Pattern** - Unique to MAUI, must be documented
4. **Interface Usage Clarification** - Current guidance conflicts with MAUI architecture

### Medium Priority Additions

5. **MAUI Build Requirements** - Prevents common setup issues
6. **Handler Pattern** - Core MAUI concept
7. **Performance Patterns** - Important for mobile development

### Low Priority Enhancements

8. **Async UI Patterns** - Minor additions to existing guidance
9. **Platform Exception Handling** - Supplements existing error handling guidance

## Proposed Agent Structure

Recommend creating a MAUI-specific agent that:

1. **Inherits** generic C# Expert guidance for:
   - Basic C# conventions
   - Error handling fundamentals
   - General async patterns
   - SOLID principles

2. **Overrides/Extends** for MAUI specifics:
   - Testing patterns (xUnit vs NUnit)
   - Platform-specific code organization
   - Handler pattern
   - Interface usage

3. **Adds** MAUI-unique guidance:
   - Build requirements
   - UI test two-project pattern
   - Mobile performance considerations
   - Platform threading considerations

## Implementation Approach

### Option A: Modify Existing C# Expert Agent
Add MAUI-specific sections directly to `.github/agents/my-agent.agent.md`

**Pros:** Single source of truth
**Cons:** Makes agent less reusable, mixes concerns

### Option B: Create MAUI-Specific Agent (RECOMMENDED)
Create `.github/agents/maui-csharp-expert.agent.md` that references the generic agent

**Pros:** Separation of concerns, both agents remain useful
**Cons:** Need to maintain synchronization

### Option C: Extend via Instructions
Keep generic agent, add MAUI specifics to `.github/copilot-instructions.md`

**Pros:** Uses existing instruction mechanism
**Cons:** Instructions are less structured than agents

## Next Steps

1. **Decision**: Choose implementation approach (recommend Option B)
2. **Create**: MAUI-specific agent with modifications outlined above
3. **Test**: Validate agent guidance matches actual codebase practices
4. **Document**: Update repository documentation to reference new agent
5. **Iterate**: Refine based on actual usage and feedback

---

## Appendix: Code Examples Analysis

### Example 1: Handler Pattern
File: `src/Core/src/Handlers/ImageButton/ImageButtonHandler.cs`

```csharp
// Platform type aliases - MAUI-specific pattern
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.ImageView.ShapeableImageView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#endif

public partial class ImageButtonHandler : IImageButtonHandler
{
    // Property mapper - MAUI core pattern
    public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper = 
        new PropertyMapper<IImageButton, IImageButtonHandler>(ImageMapper)
    {
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        // ...
    };
}
```

**Analysis**: This pattern is specific to MAUI and not covered by generic C# guidance.

### Example 2: UI Test Pattern
Files: 
- `src/Controls/tests/TestCases.HostApp/Issues/Issue1799.cs`
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue1799.cs`

**HostApp:**
```csharp
[Issue(IssueTracker.Github, 1799, "[iOS] listView without data crash on ipad.", PlatformAffected.iOS)]
public class Issue1799 : TestContentPage
{
    const string ListView = "ListView1799";
    
    protected override void Init()
    {
        var listView = new ListView { 
            AutomationId = ListView,  // Required for test automation
            // ...
        };
    }
}
```

**Test:**
```csharp
public class Issue1799 : _IssuesUITest
{
    public Issue1799(TestDevice testDevice) : base(testDevice) { }
    
    [Test]
    [Category(UITestCategories.ListView)]
    public void ListViewWithoutDataDoesNotCrash()
    {
        App.WaitForElement("ListView1799");  // Uses AutomationId
        // ...
    }
}
```

**Analysis**: Two-project pattern is unique to MAUI UI testing, not covered by generic test guidance.

### Example 3: Platform-Specific Files
Files:
- `src/Controls/src/Core/ImageButton/ImageButton.Android.cs`
- `src/Controls/src/Core/ImageButton/ImageButton.iOS.cs`
- `src/Controls/src/Core/ImageButton/ImageButton.Windows.cs`

**Analysis**: File extension-based platform targeting is MAUI-specific and not typical .NET pattern.

## Conclusion

The generic C# Expert agent provides a solid foundation for .NET development, but MAUI's unique architecture (handlers, platform-specific code, multi-targeted projects, UI testing pattern) requires significant MAUI-specific guidance. The most effective approach is to create a MAUI-specific agent that references and extends the generic C# Expert agent.
