---
name: maui-unit-testing
description: >
  xUnit testing guidance for .NET MAUI apps — ViewModel testing, mocking MAUI
  services, test project setup, code coverage, and on-device test runners.
  USE FOR: "unit test MAUI", "xUnit test", "ViewModel testing", "mock MAUI services",
  "test project setup", "code coverage", "on-device test runner", "test MAUI app",
  "IServiceProvider mock", "Moq MAUI".
  DO NOT USE FOR: UI automation testing (use appium-automation),
  performance profiling (use maui-performance), or dependency injection setup (use maui-dependency-injection).
---

# .NET MAUI Unit Testing with xUnit

## Test Project Setup

Create a class library targeting the same TFM as your MAUI app:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0;net10.0</TargetFrameworks>
    <IsPackable>false</IsPackable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="coverlet.collector" Version="6.*" />
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp\MyApp.csproj" />
  </ItemGroup>
</Project>
```

Use `net9.0` or `net10.0` (not a platform-specific TFM like `net9.0-ios`) so xUnit can run on the desktop test host.

### Conditional OutputType for App Projects

If your test project references an app project directly, prevent the app from building as an executable for the test TFM:

```xml
<!-- In the MAUI app .csproj -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <OutputType>Library</OutputType>
</PropertyGroup>
<PropertyGroup Condition="'$(TargetFramework)' != 'net9.0'">
  <OutputType>Exe</OutputType>
</PropertyGroup>
```

## xUnit Fundamentals

### [Fact] — Single Test Case

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(2, 3);

        // Assert
        Assert.Equal(5, result);
    }
}
```

### [Theory] / [InlineData] — Parameterised Tests

```csharp
public class ConverterTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    public void CelsiusToFahrenheit_ReturnsExpected(double celsius, double expected)
    {
        var result = TemperatureConverter.CelsiusToFahrenheit(celsius);
        Assert.Equal(expected, result, precision: 2);
    }
}
```

## Interface-First Architecture

Define service interfaces so ViewModels can be tested without MAUI platform dependencies:

```csharp
public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();
}

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message);
}
```

Register implementations in `MauiProgram.cs`; inject interfaces into ViewModels.

## ViewModel Testing Pattern

```csharp
public class ItemsViewModelTests
{
    private readonly Mock<IItemService> _itemServiceMock = new();
    private readonly Mock<INavigationService> _navMock = new();

    private ItemsViewModel CreateSut() =>
        new(_itemServiceMock.Object, _navMock.Object);

    [Fact]
    public async Task LoadItems_PopulatesCollection()
    {
        // Arrange
        var items = new List<Item> { new("A"), new("B") };
        _itemServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);
        var sut = CreateSut();

        // Act
        await sut.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, sut.Items.Count);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task SelectItem_NavigatesToDetail()
    {
        // Arrange
        var sut = CreateSut();
        var item = new Item("Test");

        // Act
        await sut.SelectItemCommand.ExecuteAsync(item);

        // Assert
        _navMock.Verify(n => n.GoToAsync($"detail?id={item.Id}"), Times.Once);
    }
}
```

## Mocking Common MAUI Services

| MAUI Service       | Mock Strategy                                    |
| ------------------- | ------------------------------------------------ |
| `ISecureStorage`    | `Mock<ISecureStorage>` — stub `GetAsync`/`SetAsync` |
| `IPreferences`      | `Mock<IPreferences>` — stub `Get`/`Set`/`Remove` |
| `IConnectivity`     | `Mock<IConnectivity>` — return `NetworkAccess`   |
| `IGeolocation`      | `Mock<IGeolocation>` — return fixed `Location`   |
| `IFilePicker`       | `Mock<IFilePicker>` — return `FileResult`        |
| `IMediaPicker`      | `Mock<IMediaPicker>` — return `FileResult`       |
| Shell navigation    | Abstract behind `INavigationService`             |
| `IDispatcher`       | Stub `Dispatch` to invoke action synchronously   |

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Filter by class or method
dotnet test --filter "FullyQualifiedName~ItemsViewModelTests"

# Code coverage with coverlet (outputs to TestResults/)
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML coverage report (requires reportgenerator tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/**/coverage.cobertura.xml \
  -targetdir:TestResults/CoverageReport -reporttypes:Html
```

## On-Device Testing

For tests requiring real platform APIs (sensors, camera, Bluetooth), use the `xunit.runner.devices` package to run xUnit tests inside a MAUI app on a simulator or physical device. This is separate from `dotnet test` and runs within the app process.

## Tips

- Keep ViewModels free of `Application.Current`, `Shell.Current` — wrap in injectable services.
- Use `ObservableCollection<T>` and `[ObservableProperty]` (MVVM Toolkit) for testable state.
- Assert on ViewModel properties and `CanExecute`, not UI bindings.
- Use `TaskCompletionSource` to test async waiting flows.
- Run `dotnet test` in CI to catch regressions early.
