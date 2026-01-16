# Windows Copilot Runtime Packaging Diagnostic

## Your Environment ✅
- Windows 11 24H2 (build 26100.x) - **CORRECT**
- AI Dev Gallery works - **CORRECT**
- Windows App SDK 2.0-Exp4 - **CORRECT**
- Phi Silica in Settings - **CORRECT**

## Configuration ✅
- ✅ `launchSettings.json` has `"commandName": "MsixPackage"`
- ✅ `WindowsPackageType` is NOT set to `None`
- ✅ Manifest has `systemAIModels` capability
- ✅ SDK version meets requirements

## The Problem

**"Not declared by app"** error means the app isn't running with package identity.

## How Are You Running the Tests?

### ❌ If running via Test Explorer:
Test Explorer might NOT respect the `MsixPackage` launch profile. It often runs tests **unpackaged**.

### ✅ Correct Way: Deploy and run as MSIX

**Option 1: Deploy via Visual Studio (Recommended)**
1. Set the project as startup project
2. Select "Windows Machine" profile in toolbar
3. Press F5 to deploy and run
4. This will create a packaged MSIX deployment

**Option 2: Manual deployment**
```powershell
# Build and package
dotnet build src/AI/tests/Essentials.AI.DeviceTests/Essentials.AI.DeviceTests.csproj -f net9.0-windows10.0.26100.0 -c Debug /p:GenerateAppxPackageOnBuild=true

# Find the generated MSIX
Get-ChildItem -Path "src/AI/tests/Essentials.AI.DeviceTests/bin" -Filter "*.msix" -Recurse

# Install it
Add-AppxPackage -Path "path\to\generated.msix"
```

## Quick Check: Is Your App Actually Packaged?

Add this diagnostic code to your test:

```csharp
using Windows.ApplicationModel;

try
{
    var packageId = Package.Current.Id;
    var familyName = packageId.FamilyName;
    Console.WriteLine($"✅ PACKAGED: {familyName}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ NOT PACKAGED: {ex.Message}");
}
```

If you see "❌ NOT PACKAGED", the test is running unpackaged and will **always fail** with "Not declared by app".

## Solution

**You need to ensure GenerateAppxPackageOnBuild is enabled for Windows:**

Add this to your `.csproj`:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-windows'))">
  <GenerateAppxPackageOnBuild>true</GenerateAppxPackageOnBuild>
  <AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
  <AppxAutoIncrementPackageRevision>true</AppxAutoIncrementPackageRevision>
</PropertyGroup>
```

Then **deploy via F5**, not Test Explorer.

## Test Explorer Workaround

If you MUST use Test Explorer, you might need to:
1. Build/deploy the app once via F5
2. Leave it installed
3. Test Explorer MIGHT then use the installed package

But this is unreliable. **Best practice: Use F5 deployment for Windows Copilot Runtime tests.**
