# Integration Test Results - iOS RunOniOS Tests

**Test Date**: 2025-12-14  
**Configuration**: Release (and Debug for some tests)  
**iOS Simulator**: iPhone Xs (iOS 18.5) via `IOS_TEST_DEVICE=ios-simulator-64_18.5`  
**Total Tests**: 7  
**Passed**: 1  
**Failed**: 6  

---

## ✅ PASSING TEST

### Test 1: RunOniOS("maui","Release","net10.0","iossimulator-x64",NativeAOT,null)
- **Status**: ✅ **PASSED** (2 minutes 9 seconds)
- **Template**: maui
- **Configuration**: Release
- **Runtime**: NativeAOT
- **Architecture**: iossimulator-x64
- **Result**: Successfully built, deployed, and ran on iOS simulator

**Why it passed**: NativeAOT tests explicitly set `RuntimeIdentifier=iossimulator-x64`, which forces the correct architecture build.

---

## ❌ FAILING TESTS

### Root Cause: Architecture Mismatch

**Problem**: Tests expect `iossimulator-x64` but builds are producing `iossimulator-arm64` apps.

- **Build output shows**: `-> /Users/shneuvil/Projects/maui/artifacts/bin/.../iossimulator-arm64/...`
- **XHarness looks for**: `.../iossimulator-x64/...app`
- **Error**: XHarness can't find Info.plist because it's looking in the wrong architecture folder

---

### Test 2: RunOniOS("maui","Debug","net10.0","iossimulator-x64",Mono,null)
- **Status**: ❌ FAILED (53 seconds)
- **Template**: maui
- **Configuration**: Debug
- **Runtime**: Mono
- **Architecture Expected**: iossimulator-x64
- **Architecture Built**: iossimulator-arm64

**Error**:
```
XHarness exit code: 4 (PACKAGE_NOT_FOUND)
fail: Failed to find Info.plist inside the app bundle at: 
'/Users/shneuvil/Projects/maui/bin/test-dir/RunOniOSmauiDeb742902583/bin/Debug/net10.0-ios/iossimulator-x64/RunOniOSmauiDeb742902583.app/Info.plist'
```

**Build Output**:
```
RunOniOSmauiDeb742902583 -> /Users/shneuvil/Projects/maui/artifacts/bin/RunOniOSmauiDeb742902583/Debug/net10.0-ios/iossimulator-arm64/RunOniOSmauiDeb742902583.dll
```

---

### Test 3: RunOniOS("maui","Release","net10.0","iossimulator-x64",Mono,null)
- **Status**: ❌ FAILED (2 minutes 11 seconds)
- **Template**: maui
- **Configuration**: Release
- **Runtime**: Mono
- **Architecture Expected**: iossimulator-x64
- **Architecture Built**: iossimulator-arm64

**Error**:
```
XHarness exit code: 4 (PACKAGE_NOT_FOUND)
fail: Failed to find Info.plist inside the app bundle at: 
'/Users/shneuvil/Projects/maui/bin/test-dir/RunOniOSmauiRel853892963/bin/Release/net10.0-ios/iossimulator-x64/RunOniOSmauiRel853892963.app/Info.plist'
```

**Build Output**:
```
RunOniOSmauiRel853892963 -> /Users/shneuvil/Projects/maui/artifacts/bin/RunOniOSmauiRel853892963/Release/net10.0-ios/iossimulator-arm64/RunOniOSmauiRel853892963.dll
```

---

### Test 4: RunOniOS("maui","Release","net10.0","iossimulator-x64",Mono,"full")
- **Status**: ❌ FAILED (2 minutes 18 seconds)
- **Template**: maui
- **Configuration**: Release
- **Runtime**: Mono
- **Trim Mode**: full
- **Architecture Expected**: iossimulator-x64
- **Architecture Built**: iossimulator-arm64

**Error**:
```
XHarness exit code: 4 (PACKAGE_NOT_FOUND)
fail: Failed to find Info.plist inside the app bundle at: 
'/Users/shneuvil/Projects/maui/bin/test-dir/RunOniOSmauiRel1399428667/bin/Release/net10.0-ios/iossimulator-x64/RunOniOSmauiRel1399428667.app/Info.plist'
```

**Build Output**:
```
RunOniOSmauiRel1399428667 -> /Users/shneuvil/Projects/maui/artifacts/bin/RunOniOSmauiRel1399428667/Release/net10.0-ios/iossimulator-arm64/RunOniOSmauiRel1399428667.dll
```

---

### Test 5: RunOniOS("maui-blazor","Debug","net10.0","iossimulator-x64",Mono,null)
- **Status**: ❌ FAILED (32 seconds)
- **Template**: maui-blazor
- **Configuration**: Debug
- **Runtime**: Mono
- **Architecture Expected**: iossimulator-x64
- **Architecture Built**: iossimulator-arm64

**Error**:
```
XHarness exit code: 4 (PACKAGE_NOT_FOUND)
fail: Failed to find Info.plist inside the app bundle at: 
'/Users/shneuvil/Projects/maui/bin/test-dir/RunOniOSmauibla831892514/bin/Debug/net10.0-ios/iossimulator-x64/RunOniOSmauibla831892514.app/Info.plist'
```

**Build Output**:
```
RunOniOSmauibla831892514 -> /Users/shneuvil/Projects/maui/artifacts/bin/RunOniOSmauibla831892514/Debug/net10.0-ios/iossimulator-arm64/RunOniOSmauibla831892514.dll
```

---

### Test 6: RunOniOS("maui-blazor","Release","net10.0","iossimulator-x64",Mono,null)
- **Status**: ❌ FAILED (2 minutes 20 seconds)
- **Template**: maui-blazor
- **Configuration**: Release
- **Runtime**: Mono
- **Architecture Expected**: iossimulator-x64
- **Architecture Built**: iossimulator-arm64

**Error**:
```
XHarness exit code: 4 (PACKAGE_NOT_FOUND)
fail: Failed to find Info.plist inside the app bundle at: 
'/Users/shneuvil/Projects/maui/bin/test-dir/RunOniOSmauibla1663827343/bin/Release/net10.0-ios/iossimulator-x64/RunOniOSmauibla1663827343.app/Info.plist'
```

**Build Output**:
```
RunOniOSmauibla1663827343 -> /Users/shneuvil/Projects/maui/artifacts/bin/RunOniOSmauibla1663827343/Release/net10.0-ios/iossimulator-arm64/RunOniOSmauibla1663827343.dll
```

---

### Test 7: RunOniOS("maui-blazor","Release","net10.0","iossimulator-x64",Mono,"full")
- **Status**: ❌ FAILED (19 seconds) - **DIFFERENT FAILURE**
- **Template**: maui-blazor
- **Configuration**: Release
- **Runtime**: Mono
- **Trim Mode**: full
- **Architecture**: N/A (build failed)

**Error**: Build failure due to IL trimming errors

```
error IL2111: Method 'Microsoft.AspNetCore.Components.Routing.Router.NotFoundPage.set' 
with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed 
via reflection. Trimmer can't guarantee availability of the requirements of the method.
```

**Location**:
```
/Users/shneuvil/Projects/maui/artifacts/obj/RunOniOSmauibla1714563275/Release/net10.0-ios/iossimulator-arm64/
Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/
Components_Routes_razor.g.cs(81,13)
```

---

## Summary of Issues

### Issue 1: Architecture Mismatch (6 tests)
**Affected Tests**: All Mono runtime tests (non-NativeAOT)

**Problem**: 
- Test specifies `runtimeIdentifier = "iossimulator-x64"` in test code
- But builds default to `iossimulator-arm64` (Apple Silicon default)
- NativeAOT test explicitly sets `-p:RuntimeIdentifier=iossimulator-x64` which works
- Mono tests don't set this property, so they build for arm64

**Why NativeAOT works**:
```csharp
// From AppleTemplateTests.cs line 48-54
if (runtimeVariant == RuntimeVariant.NativeAOT)
{
    buildProps.Add("PublishAot=true");
    buildProps.Add("PublishAotUsingRuntimePack=true");
    buildProps.Add("_IsPublishing=true");
    buildProps.Add($"RuntimeIdentifier={runtimeIdentifier}");  // ← This forces x64!
    buildProps.Add("IlcTreatWarningsAsErrors=false");
}
```

**Solution Options**:
1. **Option A**: Add `RuntimeIdentifier` property to all builds (not just NativeAOT)
2. **Option B**: Change test to use `iossimulator-arm64` for Mono tests
3. **Option C**: Update test path logic to detect actual architecture built

---

### Issue 2: IL Trimming Error in Blazor Full Trim (1 test)
**Affected Tests**: RunOniOS("maui-blazor","Release","net10.0","iossimulator-x64",Mono,"full")

**Problem**: 
- Blazor Router's `NotFoundPage` property has `DynamicallyAccessedMembersAttribute`
- Full trimming mode treats this as an error when accessed via reflection
- This is a known IL trimming warning that's being treated as an error

**Solution Options**:
1. **Option A**: Add trimming suppression for this specific Blazor component
2. **Option B**: Disable `TreatWarningsAsErrors` for full trimming Blazor tests
3. **Option C**: Update Blazor templates to handle this trimming warning

---

## What Was Fixed to Get Here

1. ✅ **iOS Simulator Version** - Added `IOS_TEST_DEVICE=ios-simulator-64_18.5` environment variable support
2. ✅ **SDK Version Mismatch** - Removed conflicting `10.0.20-dev` SDK pack
3. ✅ **Xcode Version Validation** - Added `SKIP_XCODE_VERSION_CHECK=true` environment variable to bypass Xcode 26.0 vs 26.1.1 mismatch
4. ✅ **Output Path Redirection** - Created empty `Directory.Build.props` and `Directory.Build.targets` in test directory to prevent inheritance from repo root

---

## Next Steps

To fix the remaining 6 failures, choose one of these approaches:

### Approach 1: Force x64 for All Tests (Recommended)
Add `RuntimeIdentifier` property for all iOS builds, not just NativeAOT:

```csharp
// In AppleTemplateTests.cs, before line 48 (before NativeAOT check)
buildProps.Add($"RuntimeIdentifier={runtimeIdentifier}");
```

### Approach 2: Update Tests to Use ARM64
Change test cases to use `iossimulator-arm64` instead of `iossimulator-x64`:

```csharp
// Change all test cases from:
[TestCase("maui", "Debug", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
// To:
[TestCase("maui", "Debug", DotNetCurrent, "iossimulator-arm64", RuntimeVariant.Mono, null)]
```

### Approach 3: Fix Path Detection Logic
Update `AppleTemplateTests.cs` line 67 to detect the actual architecture built instead of assuming the test parameter.

---

## CI vs Local Differences

**Why CI might work**:
- CI agents may be running on Intel Macs (x64), so `iossimulator-x64` is the natural default
- Local machine is Apple Silicon (ARM64), so `iossimulator-arm64` is the natural default
- Without explicit `RuntimeIdentifier` property, the build defaults to host architecture
