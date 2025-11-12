# Microsoft.Maui.AppleIntegrationTests

This is a standalone integration test project for Apple platform (iOS) template testing, using **xUnit** as the testing framework.

## Purpose

This project contains integration tests specifically for Apple (iOS) MAUI templates. It tests:
- MAUI template creation and building for iOS simulator
- Blazor-MAUI hybrid templates for iOS
- Native AOT compilation scenarios
- Various trimming modes (full trimming)
- Both Debug and Release configurations

## Structure

```
Microsoft.Maui.AppleIntegrationTests/
├── Apple/
│   ├── Codesign.cs         # Code signing utilities
│   └── Simulator.cs        # iOS simulator management
├── Utilities/
│   ├── Categories.cs       # Test categories
│   ├── DotnetInternal.cs   # .NET CLI wrapper utilities
│   ├── FileUtilities.cs    # File manipulation utilities
│   ├── TestEnvironment.cs  # Environment configuration
│   ├── ToolRunner.cs       # Process execution utilities
│   └── XHarness.cs         # XHarness test runner utilities
├── AppleTemplateTests.cs   # Main test class with iOS template tests
├── BaseBuildTest.cs        # Base test infrastructure
├── Usings.cs               # Global usings
└── Microsoft.Maui.AppleIntegrationTests.csproj
```

## Requirements

- macOS (tests will be ignored on other platforms)
- .NET 8.0 SDK or higher
- Xcode with iOS simulators
- XHarness CLI tool (installed via dotnet tool)
- Built MAUI artifacts in the repository's `artifacts/` directory

## Environment Variables

- `MAUI_PACKAGE_VERSION`: Required - specifies the MAUI package version being tested
- `IOS_TEST_DEVICE`: Optional - simulator identifier (default: "ios-simulator-64")
- `AGENT_TEMPDIRECTORY`: Optional - Azure Pipelines temp directory
- `AGENT_NAME`: Optional - Azure Pipelines agent name (for CI detection)
- `LogDirectory`: Optional - custom log directory path

## Running Tests

### Prerequisites

1. Restore tools and build the repository:
   ```bash
   cd /path/to/maui
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   dotnet cake --target=dotnet-pack
   ```

2. Set the MAUI_PACKAGE_VERSION environment variable:
   ```bash
   export MAUI_PACKAGE_VERSION=$(cat artifacts/version.txt)
   ```

### Execute Tests

Run all iOS template tests:
```bash
dotnet test src/TestUtils/src/Microsoft.Maui.AppleIntegrationTests/Microsoft.Maui.AppleIntegrationTests.csproj
```

Run specific test cases:
```bash
dotnet test src/TestUtils/src/Microsoft.Maui.AppleIntegrationTests/Microsoft.Maui.AppleIntegrationTests.csproj --filter "FullyQualifiedName~RunOniOS"
```

Run only with specific trait:
```bash
dotnet test src/TestUtils/src/Microsoft.Maui.AppleIntegrationTests/Microsoft.Maui.AppleIntegrationTests.csproj --filter "Category=RunOniOS"
```

## Test Categories

- `RunOniOS`: Tests that run on iOS simulator (uses xUnit `[Trait]` attribute)

## How It Works

1. **Setup Phase** (Fixture Constructor):
   - Locates MAUI artifacts directory
   - Copies required NuGet packages to a test-specific location
   - Sets up custom NuGet.config to use only test packages

2. **Per-Test Setup** (Constructor):
   - Verifies macOS environment (skips test if not on macOS)
   - Launches iOS simulator
   - Shows simulator window
   - Sets up ITestOutputHelper for logging

3. **Test Execution** (Theory methods with InlineData):
   - Creates a new MAUI project from template using `dotnet new`
   - Configures build properties (NativeAOT, trimming, etc.)
   - Builds the project for iOS simulator
   - Deploys and runs the app via XHarness
   - Validates app launches successfully (timeout indicates success)

4. **Teardown** (IDisposable.Dispose):
   - Shuts down simulator
   - Cleans up test resources

## Key Differences from Original IntegrationTests

This standalone project:
- Uses **xUnit** instead of NUnit for testing
- Contains only Apple/iOS-specific test infrastructure
- Has its own namespace (`Microsoft.Maui.AppleIntegrationTests`)
- Can be run independently without the full integration test suite
- Uses `ITestOutputHelper` for test output instead of `TestContext`
- Implements xUnit patterns: `IClassFixture` for shared setup, `IDisposable` for cleanup
- Uses `[Theory]` and `[InlineData]` instead of `[TestCase]`
- Easier to maintain and understand for iOS-specific testing scenarios
- Reduced dependencies and faster to compile

## Debugging Tips

- Test output directories are created in `bin/test-dir/` under the MAUI repository root
- Build logs (`.binlog`) are created in each test's project directory
- XHarness logs are placed in the `xh-results/` subdirectory of each test
- Use `ITestOutputHelper` to write debug information: `_outputHelper.WriteLine()`
- xUnit runs tests in parallel by default; use `[Collection]` attributes if you need to serialize tests
