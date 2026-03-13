# Creating Bindings for .NET iOS Projects

This guide covers the process of creating C# bindings to interface with native iOS platform libraries (xcframeworks) in .NET MAUI, .NET for iOS, and .NET for Mac Catalyst projects.

## Overview

When you need to use a third-party iOS library not written in C#, you must create a **binding** that exposes the native API to .NET. There are two primary approaches:

1. **Traditional Full Bindings** - Bind the entire native library's API surface using Objective Sharpie
2. **Native Library Interop (Slim Bindings)** - Create a thin native wrapper exposing only the APIs you need

---

## Approach Comparison

### When to Use Traditional Full Bindings

- You need access to the majority of a library's API surface
- You're a vendor creating bindings for third-party developers
- You're already maintaining traditional bindings and comfortable with the workflow
- The library has a stable API that doesn't change frequently

### When to Use Native Library Interop (Slim Bindings)

- You only need a small subset of the library's functionality
- You want easier maintenance when the underlying SDK updates
- You prefer working in native languages (Swift/Objective-C) for the wrapper implementation
- You want better isolation from breaking changes in the underlying SDK

---

## Traditional Full Bindings

This approach binds the entire native library directly, generating C# interfaces for all public APIs.

### Prerequisites

- .NET 9 SDK (or your target .NET version)
- Xcode and Xcode Command Line Tools
- Objective Sharpie (requires Xamarin.iOS legacy installation)
- macOS development environment

### Step 1: Create the Binding Project

```bash
dotnet new iosbinding -n "MyLibrary.iOS"
```

This generates three files:
- `MyLibrary.iOS.csproj` - Project configuration
- `ApiDefinition.cs` - C# interface definitions
- `StructsAndEnums.cs` - Enumerations and structures

### Step 2: Prepare the Native Framework

Most modern iOS libraries are distributed as XCFrameworks. If you have source code, you'll need to build it into an XCFramework.

#### Building from Source (Swift Library Example)

1. Open the Xcode project/workspace
2. Navigate to **Build Settings**
3. Set **Mach-O Type** to **Static Library**
4. Set **Build Libraries for Distribution** to **Yes**
5. Set **Skip Install** to **No**

Build for iOS device:
```bash
xcodebuild archive \
  -scheme "YourScheme" \
  -destination 'generic/platform=iOS' \
  -archivePath ./build/YourLibrary \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES
```

Build for iOS Simulator:
```bash
xcodebuild archive \
  -scheme "YourScheme" \
  -destination 'generic/platform=iOS Simulator' \
  -archivePath ./build/YourLibrarySim \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES
```

Create the XCFramework:
```bash
xcodebuild -create-xcframework \
  -framework ./build/YourLibrary.xcarchive/Products/Library/Frameworks/YourLibrary.framework \
  -framework ./build/YourLibrarySim.xcarchive/Products/Library/Frameworks/YourLibrary.framework \
  -output ./build/YourLibrary.xcframework
```

### Step 3: Configure the Binding Project

Add the native reference to your `.csproj`:

```xml
<ItemGroup>
  <ObjcBindingApiDefinition Include="ApiDefinition.cs"/>
  <ObjcBindingCoreSource Include="StructsAndEnums.cs"/>
</ItemGroup>

<ItemGroup>
  <NativeReference Include="YourLibrary.xcframework">
    <Kind>Framework</Kind>
    <Frameworks>Foundation UIKit CoreGraphics</Frameworks>
  </NativeReference>
</ItemGroup>
```

#### NativeReference Properties

| Property | Description |
|----------|-------------|
| `Kind` | `Framework` for .framework/.xcframework, `Static` for .a files |
| `Frameworks` | Space-separated list of Apple frameworks the library depends on |
| `LinkerFlags` | Additional linker flags (e.g., Swift library paths) |
| `SmartLink` | Enable/disable smart linking (default: true) |
| `ForceLoad` | Force load all symbols (use if symbols missing at runtime) |

Example with Swift support:
```xml
<NativeReference Include="YourLibrary.xcframework">
  <Kind>Framework</Kind>
  <Frameworks>Foundation UIKit</Frameworks>
  <LinkerFlags>-L "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphoneos" -Wl,-rpath -Wl,@executable_path/Frameworks</LinkerFlags>
  <SmartLink>False</SmartLink>
  <ForceLoad>True</ForceLoad>
</NativeReference>
```

### Step 4: Generate Bindings with Objective Sharpie

Install Objective Sharpie:
```bash
brew install --cask objectivesharpie
```

Check available SDKs:
```bash
sharpie xcode -sdks
```

Generate bindings:
```bash
sharpie bind \
  --sdk=iphoneos18.1 \
  --output=SharpieOutput \
  --namespace=YourNamespace \
  --framework=/path/to/YourLibrary.xcframework/ios-arm64/YourLibrary.framework
```

For header files specifically:
```bash
sharpie bind \
  --sdk=iphoneos18.1 \
  --output=SharpieOutput \
  --namespace=YourNamespace \
  --scope=Headers \
  Headers/YourLibrary-Swift.h
```

### Step 5: Clean Up Generated Bindings

Objective Sharpie generates code that often requires manual cleanup:

#### Common Issues to Fix

1. **Namespace Addition** - Add your namespace to both generated files

2. **Remove Problematic InitWithCoder**
   ```csharp
   // REMOVE these - they conflict with linker-added methods
   [Export("initWithCoder:")]
   NativeHandle Constructor(NSCoder coder);
   ```

3. **Fix Protocol/Interface Type Mismatches**
   ```csharp
   // Change concrete types to interfaces where needed
   // Before: CAAnimation animation
   // After: ICAAnimation animation
   ```

4. **Handle Verify Attributes** - Review and remove `[Verify]` attributes after confirming correctness

#### ApiDefinition.cs Structure

```csharp
using System;
using Foundation;
using UIKit;
using ObjCRuntime;

namespace YourNamespace
{
    // @interface YourClass : NSObject
    [BaseType(typeof(NSObject))]
    interface YourClass
    {
        // -(void)doSomething;
        [Export("doSomething")]
        void DoSomething();

        // @property (nonatomic, strong) NSString *name;
        [Export("name", ArgumentSemantic.Strong)]
        string Name { get; set; }

        // +(instancetype)sharedInstance;
        [Static]
        [Export("sharedInstance")]
        YourClass SharedInstance { get; }
    }

    // @protocol YourDelegate <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface YourDelegate
    {
        // @optional -(void)didComplete:(BOOL)success;
        [Export("didComplete:")]
        void DidComplete(bool success);
    }
}
```

### Step 6: Build and Package

```bash
dotnet build MyLibrary.iOS.csproj -c Release
```

The NuGet package will be in `bin/Release/`.

---

## Native Library Interop (Slim Bindings)

This modern approach creates a thin native wrapper exposing only the APIs you need.

### Project Structure

```
MyBinding/
├── macios/
│   ├── native/
│   │   └── MyBinding/           # Xcode project
│   │       ├── MyBinding.xcodeproj
│   │       └── MyBinding/
│   │           └── DotnetMyBinding.swift
│   └── MyBinding.MaciOS.Binding/
│       ├── MyBinding.MaciOS.Binding.csproj
│       └── ApiDefinition.cs
└── sample/
    └── MauiSample/              # Sample app
```

### Step 1: Set Up Using the Template

Clone the Community Toolkit template:
```bash
git clone https://github.com/CommunityToolkit/Maui.NativeLibraryInterop
cp -r Maui.NativeLibraryInterop/template ./MyBinding
```

### Step 2: Configure the Xcode Wrapper Project

The native wrapper is located at `macios/native/NewBinding/`.

#### Add Native Dependencies

You can add dependencies via:
- **CocoaPods**: Add to Podfile and run `pod install`
- **Swift Package Manager**: Add through Xcode's package dependencies
- **Manual**: Drag xcframeworks into the project

#### Create the Wrapper API

Edit `DotnetNewBinding.swift`:

```swift
import Foundation
import UIKit
import TheNativeLibrary  // Your native library

@objc(MauiMyBinding)
public class MauiMyBinding : NSObject {
    
    @objc(initialize)
    public static func initialize() {
        TheNativeLibrary.configure()
    }
    
    @objc(doSomethingWithValue:completion:)
    public static func doSomething(
        value: String,
        completion: @escaping (String?, NSError?) -> Void
    ) {
        TheNativeLibrary.process(value) { result, error in
            completion(result, error as NSError?)
        }
    }
    
    @objc(createViewWithFrame:)
    public static func createView(frame: CGRect) -> UIView {
        return TheNativeLibrary.createCustomView(frame: frame)
    }
}
```

**Key Requirements:**
- Classes must be `public` and annotated with `@objc(ClassName)`
- Methods must be `public` and annotated with `@objc(methodName:parameter:)`
- Use only types .NET already knows: `NSObject`, `NSData`, `NSError`, `String`, `UIView`, primitives
- Callbacks should use `@escaping` completion handlers

### Step 3: Configure the Binding Project

The `.csproj` uses `XcodeProject` instead of `NativeReference`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <IsBindingProject>true</IsBindingProject>
  </PropertyGroup>

  <ItemGroup>
    <XcodeProject Include="../native/MyBinding/MyBinding.xcodeproj">
      <SchemeName>MyBinding</SchemeName>
      <!-- Optional: Override defaults -->
      <!-- <Kind>Framework</Kind> -->
      <!-- <SmartLink>true</SmartLink> -->
    </XcodeProject>
  </ItemGroup>

  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinition.cs" />
  </ItemGroup>
</Project>
```

### Step 4: Generate and Update ApiDefinition.cs

Build the binding project first:
```bash
dotnet build
```

Then run Objective Sharpie on the generated xcframework:
```bash
sharpie bind \
  --output=sharpie-out \
  --namespace=MyBindingMaciOS \
  --sdk=iphoneos18.0 \
  --scope=Headers \
  Headers/MyBinding-Swift.h
```

The generated header is located at:
```
bin/Debug/net9.0-ios/MyBinding.MaciOS.Binding.resources/MyBindingiOS.xcframework/ios-arm64/MyBinding.framework/Headers/MyBinding-Swift.h
```

Update `ApiDefinition.cs`:

```csharp
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace MyBinding
{
    // @interface MauiMyBinding : NSObject
    [BaseType(typeof(NSObject))]
    interface MauiMyBinding
    {
        // +(void)initialize;
        [Static]
        [Export("initialize")]
        void Initialize();

        // +(void)doSomethingWithValue:(NSString *)value completion:(void (^)(NSString *, NSError *))completion;
        [Static]
        [Export("doSomethingWithValue:completion:")]
        [Async]  // Generates async/await version
        void DoSomething(string value, Action<string?, NSError?> completion);

        // +(UIView *)createViewWithFrame:(CGRect)frame;
        [Static]
        [Export("createViewWithFrame:")]
        UIView CreateView(CGRect frame);
    }
}
```

### Step 5: Reference in Your App

Add to your MAUI app's `.csproj`:

```xml
<!-- Reference to MaciOS Binding project -->
<ItemGroup Condition="$(TargetFramework.Contains('ios')) Or $(TargetFramework.Contains('maccatalyst'))">
  <ProjectReference Include="..\..\macios\MyBinding.MaciOS.Binding\MyBinding.MaciOS.Binding.csproj" />
</ItemGroup>
```

### Step 6: Use in Your App

```csharp
#if IOS || MACCATALYST
using MyBinding;
#endif

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
#if IOS || MACCATALYST
        MauiMyBinding.Initialize();
#endif
    }

    private async void OnButtonClicked(object sender, EventArgs e)
    {
#if IOS || MACCATALYST
        try
        {
            // Using the async version (generated by [Async] attribute)
            var result = await MauiMyBinding.DoSomethingAsync("test");
            await DisplayAlert("Result", result, "OK");
        }
        catch (NSErrorException ex)
        {
            await DisplayAlert("Error", ex.Error.LocalizedDescription, "OK");
        }
#endif
    }
}
```

---

## Handling Native Dependencies

### Understanding Dependency Types

1. **System Frameworks** - Apple's built-in frameworks (Foundation, UIKit, etc.)
2. **Third-Party Frameworks** - External xcframeworks/frameworks
3. **Static Libraries** - `.a` files
4. **Swift Runtime** - Required for Swift-based libraries

### Declaring System Framework Dependencies

In traditional bindings, list all required frameworks:

```xml
<NativeReference Include="MyLibrary.xcframework">
  <Kind>Framework</Kind>
  <Frameworks>Foundation UIKit CoreGraphics CoreImage QuartzCore AVFoundation</Frameworks>
</NativeReference>
```

**Finding Dependencies:**
- Check the native library's documentation
- Look at the Xcode project's "Link Binary with Libraries" build phase
- Review `Package.swift` for Swift packages
- Check the podspec for CocoaPods

### Handling Third-Party Dependencies

#### Option 1: Vendor Dependencies (Include in Binding)

For static libraries, dependencies can be statically linked into your wrapper:

```swift
// In your Xcode project, ensure dependencies are linked statically
// Check Build Settings > Mach-O Type = Static Library
```

#### Option 2: Separate Bindings for Each Dependency

Create separate binding projects for each dependency:

```xml
<!-- In your main app project -->
<ItemGroup Condition="$(TargetFramework.Contains('ios'))">
  <ProjectReference Include="..\Bindings\MyLibrary.iOS\MyLibrary.iOS.csproj" />
  <ProjectReference Include="..\Bindings\DependencyA.iOS\DependencyA.iOS.csproj" />
  <ProjectReference Include="..\Bindings\DependencyB.iOS\DependencyB.iOS.csproj" />
</ItemGroup>
```

#### Option 3: CocoaPods in Native Wrapper (Slim Bindings)

For Native Library Interop, dependencies can be managed in the Xcode wrapper project:

```ruby
# Podfile in macios/native/MyBinding/
platform :ios, '15.0'

target 'MyBinding' do
  use_frameworks! :linkage => :static
  
  pod 'FirebaseMessaging', '~> 10.0'
  pod 'SomeOtherLibrary'
end
```

### Swift Runtime Considerations

Swift libraries require the Swift runtime. Add linker flags:

```xml
<NativeReference Include="SwiftLibrary.xcframework">
  <Kind>Framework</Kind>
  <Frameworks>Foundation UIKit</Frameworks>
  <LinkerFlags>
    -L "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphoneos"
    -L "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/iphonesimulator"
    -Wl,-rpath -Wl,@executable_path/Frameworks
  </LinkerFlags>
</NativeReference>
```

---

## Troubleshooting Common Issues

### Build Errors

#### "Framework not found"
- Verify the xcframework path is correct
- Check that all architectures are included (arm64 for device, x86_64/arm64 for simulator)
- Ensure `Kind` is set correctly (`Framework` vs `Static`)

#### "Undefined symbols"
- Add missing frameworks to the `<Frameworks>` element
- Try setting `<ForceLoad>True</ForceLoad>`
- Check if the library requires Swift runtime support

#### "No type or protocol named..."
- Review ApiDefinition.cs for incorrect type mappings
- Protocol types should use interfaces (e.g., `ICAAnimation` not `CAAnimation`)

### Runtime Errors

#### "Native class hasn't been loaded"
- Ensure the xcframework is properly embedded
- Try setting `<SmartLink>False</SmartLink>` and `<ForceLoad>True</ForceLoad>`
- Verify the binding project builds successfully

#### "Library not loaded: @rpath/..."
- Add appropriate linker flags for Swift runtime
- Check that all dependencies are properly linked
- Verify the framework is embedded (not just linked)

### IntelliSense Not Working

This is expected behavior for binding projects:
- Binding projects don't use source generators
- Build the binding project first, then reload the solution
- The app will compile even if IntelliSense shows errors

---

## Best Practices

### API Design for Slim Bindings

1. **Keep wrapper APIs simple** - Use only primitive types and types .NET knows
2. **Use completion handlers** - Better than synchronous calls for async operations
3. **Return UIView** - For native views that need to be displayed
4. **Handle errors properly** - Convert errors to NSError for proper propagation

### Maintenance

1. **Version your bindings** - Use semantic versioning matching the native library
2. **Document dependencies** - List all required frameworks and their versions
3. **Test on real devices** - Simulator-only testing may miss linking issues
4. **Keep wrappers thin** - Don't add business logic to the native wrapper

### Project Organization

```
Solution/
├── src/
│   └── MyApp/
│       └── MyApp.csproj
├── bindings/
│   ├── ios/
│   │   ├── native/           # Xcode wrapper projects
│   │   └── MyLibrary.iOS/    # Binding projects
│   └── android/
│       └── ...
└── MyApp.sln
```

---

## Resources

- [Maui.NativeLibraryInterop Repository](https://github.com/CommunityToolkit/Maui.NativeLibraryInterop)
- [Native Library Interop Documentation](https://learn.microsoft.com/dotnet/communitytoolkit/maui/native-library-interop)
- [Objective Sharpie Documentation](https://learn.microsoft.com/previous-versions/xamarin/cross-platform/macios/binding/objective-sharpie/)
- [iOS Binding Project Migration Guide](https://learn.microsoft.com/dotnet/maui/migration/ios-binding-projects)

---

## Quick Reference: ApiDefinition Attributes

| Attribute | Usage |
|-----------|-------|
| `[BaseType(typeof(NSObject))]` | Base class for the interface |
| `[Static]` | Static method or property |
| `[Export("selector:")]` | Objective-C selector mapping |
| `[Async]` | Generate async wrapper for completion handler methods |
| `[Protocol]` | Marks an Objective-C protocol |
| `[Model]` | Protocol implementation base class |
| `[Abstract]` | Required protocol method |
| `[NullAllowed]` | Parameter or return value can be null |
| `[Internal]` | Don't expose publicly |

## Quick Reference: NativeReference Options

```xml
<NativeReference Include="Library.xcframework">
  <Kind>Framework|Static</Kind>
  <Frameworks>Space separated framework names</Frameworks>
  <LinkerFlags>Additional linker flags</LinkerFlags>
  <SmartLink>true|false</SmartLink>
  <ForceLoad>true|false</ForceLoad>
  <IsCxx>true|false</IsCxx>
</NativeReference>
```