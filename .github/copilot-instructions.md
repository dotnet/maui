---
title: ".NET MAUI Development Environment Instructions"
description: "Comprehensive guidance for GitHub Copilot and AI agents working on the .NET MAUI repository"
type: "development-guide"
audience: ["copilot", "ai-agents", "developers"]
repository: "dotnet/maui"
updated: "2025-10"
version: "3.0"
---

<!-- 
MAINTAINER NOTES:
This file serves as the primary context document for GitHub Copilot and AI coding assistants.
It must remain actionable, precise, and focused on automation-friendly instructions.

Key principles:
- Use action-oriented headings with explicit DO/DON'T sections
- Include exact commands with parameters - no ambiguity
- Enumerate steps clearly for complex workflows
- Add HTML comments to guide future updates
- Maintain separation between human-readable docs and AI-consumable instructions

Common update scenarios:
- .NET version changes: Update global.json references and build commands
- New platform support: Add to platform-specific sections
- Build system changes: Update automation commands and validation steps
- New testing patterns: Add to testing workflows with exact examples

Path-specific instructions:
- Template development: See .github/copilot-instructions/templates.md for template-specific guidance
-->

# .NET MAUI Development Environment Instructions

**Purpose**: This document provides comprehensive, automation-friendly guidance for GitHub Copilot and AI coding assistants working on the .NET MAUI repository.

**Scope**: Core framework development, testing, building, and contribution workflows for the cross-platform .NET MAUI framework.

## 🎯 Repository Overview and Context

**.NET MAUI** is Microsoft's cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code enabling single-codebase development for Android, iOS, iPadOS, macOS, and Windows.

### 🔧 Technology Stack and Versioning

<!-- AI Note: Always check global.json for current .NET SDK version before building -->

**Core Technologies:**
- **.NET SDK**: Version is **ALWAYS** defined in `global.json` at repository root
  - Current: .NET 10 (10.0.100-rtm or later)
- **C#** and **XAML**: Primary development languages
- **Cake Build System**: Used for compilation, packaging, and automation
- **MSBuild**: Custom build tasks and project orchestration
- **xUnit + NUnit**: Testing frameworks (xUnit for unit tests, NUnit for UI tests)


**Branch-Specific .NET Versions:**
- `main` branch → Currently .NET 10 (check `global.json` for exact version)
- `net10.0` branch → .NET 10 SDK previews and RTM
- Feature branches → Correlate to their respective .NET versions

**🚨 CRITICAL**: Before any build operation, verify .NET SDK version:
```bash
# Check required version
cat global.json | grep -A 1 '"dotnet"'

# Verify installed version matches
dotnet --version
```

## ⚡ Quick Start: Essential Setup Commands

<!-- AI Note: These commands MUST be run in exact order before any development work -->

### Prerequisites Checklist

**✅ Required for ALL platforms:**
- .NET SDK (version from `global.json` - currently .NET 10)
- Git (for repository operations)

**✅ Additional per platform:**
- **Linux Development**: Follow [Microsoft Linux .NET installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
- **Android Development**: OpenJDK 17 + Android SDK
- **iOS Development** (macOS only): Current stable Xcode from App Store or Apple Developer portal
- **VS Code Users**: .NET MAUI Dev Kit extension

### 🚀 Mandatory Setup Sequence

**Step 1: Clone and Navigate**
```bash
git clone https://github.com/dotnet/maui.git
cd maui
```

**Step 2: Restore Tools (REQUIRED - Do NOT skip)**
```bash
dotnet tool restore
```

**Step 3: Build Core Tasks (REQUIRED before IDE)**
```bash
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

**⚠️ FAILURE RECOVERY**: If Step 2 or 3 fails:
```bash
# Clean and retry
dotnet clean
rm -rf bin/ obj/
dotnet tool restore --force
dotnet build ./Microsoft.Maui.BuildTasks.slnf --verbosity normal
```


## 📁 Project Structure and Navigation Guide

<!-- AI Note: Use these paths for automated file operations and navigation -->

### 🎯 Primary Development Directories

**Core Framework Locations:**
```
src/Core/                    # 🔥 Core MAUI framework implementation
src/Controls/                # 🎨 UI controls and visual components
src/Essentials/             # 📱 Platform APIs and device essentials
src/TestUtils/              # 🧪 Testing utilities and shared infrastructure
```

**Infrastructure and Tooling:**
```
docs/                       # 📚 Development documentation
eng/                        # ⚙️ Build engineering and MSBuild tooling
.github/                    # 🤖 GitHub workflows and automation
.github/copilot-instructions/  # 📋 Path-specific Copilot instructions
```

### 🔍 Platform-Specific Code Organization

**Directory Naming Conventions:**
- `Android/` → Android-specific implementations
- `iOS/` → iOS-specific implementations
- `MacCatalyst/` → Mac Catalyst-specific implementations
- `Windows/` → Windows-specific implementations

**File Extension Conventions:**
```csharp
*.windows.cs        // Windows TFM only
*.android.cs        // Android TFM only
*.ios.cs           // iOS + MacCatalyst TFMs
*.maccatalyst.cs   // MacCatalyst TFM only
```

### 🧪 Sample Projects for Testing and Validation

**Full-Featured Samples:**
```
src/Controls/samples/Maui.Controls.Sample/           # Complete gallery with all controls
src/Essentials/samples/Essentials.Sample/            # Platform API demonstrations
src/BlazorWebView/samples/BlazorWinFormsApp/         # BlazorWebView for WinForms
src/BlazorWebView/samples/BlazorWpfApp/              # BlazorWebView for WPF
```

**Development and Testing:**
```
src/Controls/samples/Maui.Controls.Sample.Sandbox/   # Empty project for issue reproduction
```

**🎯 AI Usage Tip**: Use the Sandbox project for quick testing and issue reproduction during development.


## 🏗️ Build and Development Workflow

<!-- AI Note: Always use Cake for production builds, direct dotnet commands for development iteration -->

### ✅ Primary Build Commands

**🎯 Recommended: Cake Build System**
```bash
# Full repository build (use for final validation)
dotnet cake

# Create NuGet packages (for testing integrations)
dotnet cake --target=dotnet-pack

# Clean build (when things go wrong)
dotnet cake --target=clean
```

**⚡ Development Iteration** (faster for incremental changes):
```bash
# Build specific solution file
dotnet build Microsoft.Maui.sln

# Build specific project
dotnet build src/Core/src/Microsoft.Maui.csproj

# Build with detailed output for troubleshooting
dotnet build Microsoft.Maui.sln --verbosity detailed
```

### 🧪 Testing Strategy and Commands

#### Unit Testing (Core Framework)

**Test Project Locations:**
```bash
src/Core/tests/UnitTests/Core.UnitTests.csproj
src/Essentials/test/UnitTests/Essentials.UnitTests.csproj
src/Compatibility/Core/tests/Compatibility.UnitTests/Compatibility.Core.UnitTests.csproj
src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj
src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj
```

**Running Tests:**
```bash
# Run all unit tests
dotnet test Microsoft.Maui.sln

# Run specific test project
dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj

# Run tests with detailed output
dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj --logger "console;verbosity=detailed"
```

#### UI Testing (Critical Two-Part Implementation)

<!-- AI Note: UI tests require BOTH HostApp page AND NUnit test - never create just one -->

**🚨 MANDATORY: UI tests require implementation in TWO separate projects:**

**1. HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`):
```xml
<!-- Example: IssueXXXXX.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.Issues.IssueXXXXX">
    <StackLayout>
        <Button Text="Test Button" AutomationId="TestButton" Clicked="OnButtonClicked"/>
        <Label x:Name="ResultLabel" AutomationId="ResultLabel" Text="Initial"/>
    </StackLayout>
</ContentPage>
```

**2. NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`):
```csharp
// Example: IssueXXXXX.cs
public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Description of the issue being tested";
    
    public IssueXXXXX(TestDevice device) : base(device) { }
    
    [Test]
    [Category(UITestCategories.Layout)] // Use ONE appropriate category
    public void TestMethodName()
    {
        App.WaitForElement("TestButton");
        App.Tap("TestButton");
        // Add assertions to verify expected behavior
        Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Expected"));
    }
}
```

**✅ UI Test Validation Checklist:**
1. HostApp XAML file created with proper `AutomationId` attributes
2. NUnit test file created with matching file name
3. Test inherits from `_IssuesUITest`
4. Appropriate `[Category(UITestCategories.XYZ)]` attribute applied (only ONE category per test)
5. Both projects compile without errors
6. `AutomationId` references match between XAML and test code

**⚠️ IMPORTANT**: When a new UI test category is added to `UITestCategories.cs`, the `ui-tests.yml` workflow must also be updated to include this new category.


### 🎨 Code Formatting and Style Compliance

<!-- AI Note: ALWAYS run formatting before committing - it's automated and required -->

**🚨 MANDATORY: Format before every commit**
```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

**Command Breakdown:**
- `--no-restore`: Faster execution (dependencies already restored)
- `--exclude Templates/src`: Skip template directory (handled by template-specific rules)
- `--exclude-diagnostics CA1822`: Skip "member can be static" warnings

**Style Guidelines:**
- Follow [.NET Foundation coding style](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md)
- Consult `.editorconfig` for specific formatting rules
- Templates have special formatting rules - see `.github/copilot-instructions/templates.md`

**Validation Commands:**
```bash
# Check formatting without applying changes
dotnet format Microsoft.Maui.sln --verify-no-changes --exclude Templates/src

# Format specific file
dotnet format path/to/file.cs
```

### 🔧 Branch-Specific Development

**Local .NET SDK Usage** (when version conflicts occur):
```bash
# Linux/macOS
./dotnet-local.sh build

# Windows
./dotnet-local.cmd build
```

**Branch Selection Strategy:**
- `main` → Bug fixes without API changes
- `net10.0` → New features + API changes
- Feature branches → Match target .NET version

## 🔧 Platform-Specific Development Requirements

<!-- AI Note: Platform requirements are cumulative - check all applicable sections -->

### Android Development
**✅ Requirements:**
- Android SDK (install missing components via Android SDK Manager)
- OpenJDK 17 (Microsoft OpenJDK recommended)

**🔧 Setup Commands:**
```bash
# Install Android SDK components (after dotnet tool restore)
android

# Verify Android setup
dotnet build -f net10.0-android -p:AndroidSdkDirectory=/path/to/android-sdk
```

**📚 Reference**: [Android SDK Manager documentation](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk)

### iOS Development (macOS Only)
**✅ Requirements:**
- Current stable Xcode from [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer Portal](https://developer.apple.com/download/more/?name=Xcode)
- For Windows development: Pair to Mac setup required

**🔧 Validation:**
```bash
# Verify Xcode installation
xcode-select --print-path

# Check iOS build capability
dotnet build -f net10.0-ios
```

### Windows Development
**✅ Requirements:**
- Windows SDK (latest recommended)
- Visual Studio 2022 17.12+ with MAUI workload (for full IDE support)

### macOS/Mac Catalyst Development
**✅ Requirements:**
- Xcode (same as iOS requirements)
- macOS development tools

**🔧 Build Verification:**
```bash
# Test Mac Catalyst build
dotnet build -f net10.0-maccatalyst
```


## 🤝 Contribution Guidelines and Automation

<!-- AI Note: Follow this exact workflow for all contributions - deviation causes process failures -->

### 🔍 Issue-Driven Development Workflow

**🚨 CRITICAL REQUIREMENT: Always develop your solution first, then compare with existing PRs**

**Step-by-Step Process:**

1. **🎯 Independent Solution Development**
   - Analyze the issue requirements without looking at existing PRs
   - Design and implement your own solution approach
   - Create tests and validate your implementation

2. **🔍 Existing PR Discovery and Analysis**
   ```bash
   # Search for related PRs using GitHub CLI
   gh pr list --search "issue-number OR keywords"
   
   # Alternative: Check issue comments for linked PRs
   gh issue view [issue-number] --comments
   ```

3. **⚖️ Solution Comparison and Decision**
   - Compare your approach with existing PR implementations
   - Evaluate code quality, test coverage, performance implications
   - Consider maintainability and adherence to repository patterns

4. **📝 Documentation Requirements**
   - **MANDATORY**: Include comparison summary in your PR description
   - Explain why you chose your approach over alternatives
   - Document specific issues or concerns with other solutions
   - If adopting existing solution: identify and implement improvements

5. **🔄 Integration Options**
   - **Your solution is better**: Proceed with your implementation
   - **Existing solution is better**: Pull changes and enhance with improvements
   - **Hybrid approach**: Combine best aspects of both solutions

### 🚫 Files to NEVER Commit

<!-- AI Note: Use exact git commands below to reset these files if accidentally modified -->

**🚨 AUTO-GENERATED FILES - NEVER COMMIT:**
- `cgmanifest.json` files (CI-generated component manifests)
- `templatestrings.json` files (Auto-generated localization)

**🔧 File Reset Commands:**
```bash
# Reset CGManifest files
find . -name "cgmanifest.json" -exec git checkout HEAD -- {} \;

# Reset template strings
find . -name "templatestrings.json" -exec git checkout HEAD -- {} \;

# Verify no unwanted files are staged
git status --porcelain | grep -E "(cgmanifest\.json|templatestrings\.json)"
```

**✅ AI Agent Guidelines:**
Since coding agents function as both CI and pair programmers, they must:
- **Always reset changes to `cgmanifest.json` files** - Generated during CI builds
- **Always reset changes to `templatestrings.json` files** - Auto-generated localization files

### 📋 PublicAPI.Unshipped.txt Management

**🚨 CRITICAL: Proper API management prevents CI failures**

**❌ NEVER DO:**
- Turn off analyzers to bypass PublicAPI issues
- Add `<NoWarn>` tags for API analyzer warnings
- Delete existing API entries

**✅ ALWAYS DO:**
```bash
# Step 1: Revert all PublicAPI.Unshipped.txt changes
find . -name "PublicAPI.Unshipped.txt" -exec git checkout HEAD -- {} \;

# Step 2: Build to generate new API requirements
dotnet build Microsoft.Maui.sln

# Step 3: Auto-fix API files using format analyzers
dotnet format analyzers Microsoft.Maui.sln

# Step 4: Verify only new APIs are added
git diff --name-only | grep "PublicAPI.Unshipped.txt"
```

**🔧 Troubleshooting API Issues:**
```bash
# If format analyzers don't work, manually check build output
dotnet build Microsoft.Maui.sln | grep -i "publicapi"

# Show required API additions
dotnet build 2>&1 | grep -A 5 "must declare the member in PublicAPI.Unshipped.txt"
```

### 🌿 Branch Strategy and Version Targeting

**Branch Selection Rules:**
- `main` → Bug fixes without API changes (currently .NET 10)
- `net10.0` → New features and API changes (.NET 10 previews/RTM)
- Feature branches → Match their respective .NET version

**⚠️ Important**: Main branch uses latest stable .NET SDK regardless of LTS status. Check `global.json` before starting work.

### 📚 Documentation Requirements

**When to Update Documentation:**
- Public API changes → Update XML documentation
- New features → Update relevant files in `docs/` folder
- Breaking changes → Update migration guides
- Template changes → Update `.github/copilot-instructions/templates.md`

**Style Requirements:**
- Follow existing XML documentation patterns
- Include code examples for new APIs
- Update changelog if applicable

### 📋 Pull Request Requirements

**🚨 MANDATORY PR Template Header:**

All PRs MUST include this exact header at the top of the description:

```markdown
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

**❌ CRITICAL**: Without this header, users cannot test PR builds and your work loses impact.

**✅ Additional PR Checklist:**
- [ ] Enable "Allow edits from maintainers" checkbox
- [ ] Include comparison with existing PRs (if any)
- [ ] Add test coverage for new functionality
- [ ] Run `dotnet format` before committing
- [ ] Verify PublicAPI.Unshipped.txt changes are minimal and correct
- [ ] Reset any auto-generated files (cgmanifest.json, templatestrings.json)
- [ ] Update ui-tests.yml if new UITestCategories were added


## 🤖 AI Agent and Automation Guidelines

<!-- AI Note: This section contains specific guidance for automated development processes -->

### 🎯 Decision Trees for Common Scenarios

**Scenario 1: Build Failures**
```
Build fails?
├─ Check .NET SDK version matches global.json?
│  ├─ No → Install correct version, retry
│  └─ Yes → Check build tasks?
│     ├─ Not built → Run: dotnet build ./Microsoft.Maui.BuildTasks.slnf
│     └─ Built → Check for dependency conflicts
│        ├─ Conflicts found → Run: dotnet clean && dotnet restore
│        └─ No conflicts → Check platform-specific requirements
```

**Scenario 2: Test Failures**
```
Tests failing?
├─ New UI test failing?
│  ├─ Missing HostApp page? → Create XAML + code-behind
│  ├─ Missing NUnit test? → Create test class inheriting _IssuesUITest
│  └─ AutomationId mismatch? → Verify XAML and test code match
├─ Unit test failing?
│  ├─ API change related? → Check PublicAPI.Unshipped.txt files
│  └─ Logic change related? → Review test assertions
```

**Scenario 3: API Changes**
```
Adding public API?
├─ Build fails with API analyzer errors?
│  ├─ Yes → Run: dotnet format analyzers Microsoft.Maui.sln
│  └─ Still failing → Manually add entries to PublicAPI.Unshipped.txt
├─ Breaking change?
│  ├─ Yes → Must target feature branch (net10.0)
│  └─ No → Can target main branch
```

### 🔧 Automation Command Patterns

**Pre-Development Setup:**
```bash
# Standard initialization sequence
cd /path/to/maui
dotnet tool restore
dotnet build ./Microsoft.Maui.BuildTasks.slnf
dotnet restore Microsoft.Maui.sln
```

**Development Iteration:**
```bash
# Quick development cycle
dotnet build [specific-project.csproj]
dotnet test [test-project.csproj] --no-build
dotnet format [specific-file.cs]
```

**Pre-Commit Validation:**
```bash
# Full validation sequence
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
dotnet build Microsoft.Maui.sln
dotnet test Microsoft.Maui.sln --no-build

# Reset auto-generated files
find . -name "cgmanifest.json" -exec git checkout HEAD -- {} \;
find . -name "templatestrings.json" -exec git checkout HEAD -- {} \;

# Final status check
git status --porcelain
```

### 🚨 Critical Automation Checkpoints

**Before Making Changes:**
1. Verify .NET SDK version matches `global.json`
2. Ensure build tasks are compiled
3. Confirm base branch is correct for change type

**During Development:**
1. Format code after each significant change
2. Run relevant tests frequently
3. Check for API analyzer warnings

**Before Committing:**
1. Run full format command
2. Execute complete build
3. Reset auto-generated files
4. Verify PublicAPI.Unshipped.txt changes are minimal

## 📝 Edge Cases and Troubleshooting

<!-- AI Note: Add new edge cases here as they are discovered -->

### Common Build Issues and Solutions

**Issue: "Build tasks not found"**
```bash
# Solution: Rebuild build tasks
dotnet clean ./Microsoft.Maui.BuildTasks.slnf
dotnet build ./Microsoft.Maui.BuildTasks.slnf
```

**Issue: "Dependency version conflicts"**
```bash
# Solution: Full clean and restore
dotnet clean Microsoft.Maui.sln
rm -rf bin/ obj/
dotnet restore Microsoft.Maui.sln --force
```

**Issue: "Android SDK not found"**
```bash
# Solution: Check and install Android components
android # Opens Android SDK Manager
# Or set environment variable: export ANDROID_HOME=/path/to/android-sdk
```

**Issue: "PublicAPI analyzer failures"**
```bash
# Solution: Use format analyzers first, then manual additions
dotnet format analyzers Microsoft.Maui.sln
# If still failing, check build output for required API entries
```

### Platform-Specific Troubleshooting

**iOS/Mac Build Issues:**
- Verify Xcode command line tools: `xcode-select --install`
- Check Xcode version compatibility with .NET MAUI version
- Ensure provisioning profiles are current (for device testing)

**Android Build Issues:**
- Verify OpenJDK 17 installation: `java -version`
- Check ANDROID_HOME environment variable
- Update Android SDK components via Android SDK Manager

**Windows Build Issues:**
- Ensure Windows SDK is installed
- Verify Visual Studio workloads include .NET MAUI development
- Check for missing NuGet packages: `dotnet restore --force`

### Template Development Edge Cases

When working with files in `src/Templates/`:
- **ALWAYS** consult `.github/copilot-instructions/templates.md` for template-specific rules
- Use proper conditional compilation markers (`//-:cnd:noEmit` for platform directives)
- Never modify `cgmanifest.json` or `templatestrings.json` in templates
- Test template instantiation: `dotnet new maui -n TestApp`

## 🔗 Additional Resources and Documentation

- [Development Guide](.github/DEVELOPMENT.md) - Detailed setup and platform-specific instructions
- [Development Tips](docs/DevelopmentTips.md) - Advanced development techniques and troubleshooting
- [Contributing Guidelines](.github/CONTRIBUTING.md) - Community contribution standards and CLA requirements
- [Testing Wiki](https://github.com/dotnet/maui/wiki/Testing) - Comprehensive testing strategies and tools
- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui) - Official framework documentation
- [Testing PR Builds](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) - How users can test PR artifacts
- [Template Instructions](.github/copilot-instructions/templates.md) - Template-specific development guidance

---

<!-- MAINTAINER NOTES:
Future update areas to consider:
1. Performance optimization guidelines for AI agents
2. Integration patterns for new platform support
3. Advanced debugging techniques for complex cross-platform issues
4. CI/CD integration patterns for automated testing
5. Memory profiling and performance testing automation

Last updated: 2025-10 - Version 3.0
Next review: When major .NET version changes or significant build system updates occur
-->