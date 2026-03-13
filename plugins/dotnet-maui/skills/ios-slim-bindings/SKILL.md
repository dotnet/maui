---
name: ios-slim-bindings
description: Create and update slim/native platform interop bindings for iOS in .NET MAUI and .NET for iOS projects. Guides through creating Swift/Objective-C wrappers, configuring Xcode projects, generating C# API definitions, and integrating native iOS libraries using the Native Library Interop (NLI) approach. Use when asked about iOS bindings, xcframework integration, Swift interop, Objective Sharpie, or bridging native iOS SDKs to .NET.
---

# When to use this skill

Activate this skill when the user asks:
- How do I create iOS bindings for a native library?
- How do I wrap an iOS SDK for use in .NET MAUI?
- How do I create slim bindings for iOS?
- How do I use Native Library Interop for iOS?
- How do I bind a Swift library to .NET?
- How do I use Objective Sharpie for iOS bindings?
- How do I integrate an xcframework into .NET MAUI?
- How do I create a Swift wrapper for a native iOS library?
- How do I update iOS bindings when the native SDK changes?
- How do I fix iOS binding build errors?
- How do I expose native iOS APIs to C#?
- How do I handle CocoaPods dependencies in iOS bindings?
- How do I handle Swift Package Manager dependencies?

# Overview

This skill guides the creation of **Native Library Interop (Slim Bindings)** for iOS. This modern approach creates a thin native Swift/Objective-C wrapper exposing only the APIs you need from a native iOS library, making bindings easier to create and maintain.

## When to Use Slim Bindings vs Traditional Bindings

| Scenario | Recommended Approach |
|----------|---------------------|
| Need only a subset of library functionality | **Slim Bindings** ✓ |
| Easier maintenance when SDK updates | **Slim Bindings** ✓ |
| Prefer working in Swift/Objective-C for wrapper | **Slim Bindings** ✓ |
| Better isolation from breaking changes | **Slim Bindings** ✓ |
| Need entire library API surface | Traditional Bindings |
| Creating bindings for third-party developers | Traditional Bindings |
| Already maintaining traditional bindings | Traditional Bindings |

# Inputs

| Parameter | Required | Example | Notes |
|-----------|----------|---------|-------|
| libraryName | yes | `FirebaseMessaging`, `Lottie` | Name of the native iOS library to bind |
| bindingProjectName | yes | `MyBinding.MaciOS` | Name for the C# binding project |
| dependencySource | no | `cocoapods`, `spm`, `xcframework` | How the native library is distributed |
| targetFrameworks | no | `net9.0-ios;net9.0-maccatalyst` | Target frameworks (default: latest .NET iOS + Mac Catalyst) |
| exposedApis | no | List of specific APIs | Which native APIs to expose (helps scope the wrapper) |

# Project Structure

The recommended project structure for Native Library Interop:

```
MyBinding/
├── macios/
│   ├── native/
│   │   └── MyBinding/                    # Xcode project
│   │       ├── MyBinding.xcodeproj/
│   │       │   └── project.pbxproj
│   │       ├── MyBinding/
│   │       │   └── DotnetMyBinding.swift  # Swift wrapper implementation
│   │       └── Podfile                    # If using CocoaPods
│   │       └── Package.swift              # If using Swift Package Manager
│   └── MyBinding.MaciOS.Binding/
│       ├── MyBinding.MaciOS.Binding.csproj
│       └── ApiDefinition.cs
├── sample/
│   └── MauiSample/                        # Sample MAUI app
│       ├── MauiSample.csproj
│       └── MainPage.xaml.cs
└── README.md
```

# Step-by-step Process

## Step 1: Create Project Structure from Command Line

This section shows how to create the entire binding project structure using only command-line tools—no GUI or template cloning required.

### Prerequisites

Install XcodeGen (generates Xcode projects from YAML):

```bash
brew install xcodegen
```

### Create Directory Structure

```bash
# Set your binding name
BINDING_NAME="MyBinding"

# Create the full directory structure
mkdir -p ${BINDING_NAME}/macios/native/${BINDING_NAME}/${BINDING_NAME}
mkdir -p ${BINDING_NAME}/macios/${BINDING_NAME}.MaciOS.Binding
mkdir -p ${BINDING_NAME}/sample/MauiSample

cd ${BINDING_NAME}
```

## Step 2: Create the Xcode Project with XcodeGen

### Create the XcodeGen Project Spec

Create `macios/native/${BINDING_NAME}/project.yml`:

```bash
cat > macios/native/${BINDING_NAME}/project.yml << 'EOF'
name: MyBinding
options:
  bundleIdPrefix: com.example
  deploymentTarget:
    iOS: "15.0"
    macOS: "12.0"
  xcodeVersion: "15.0"
  generateEmptyDirectories: true

settings:
  base:
    MARKETING_VERSION: "1.0.0"
    CURRENT_PROJECT_VERSION: "1"
    BUILD_LIBRARY_FOR_DISTRIBUTION: YES
    SKIP_INSTALL: NO
    MACH_O_TYPE: staticlib
    SWIFT_VERSION: "5.0"
    ENABLE_BITCODE: NO
    DEFINES_MODULE: YES

targets:
  MyBinding:
    type: framework
    platform: iOS
    sources:
      - path: MyBinding
        type: group
    settings:
      base:
        INFOPLIST_FILE: MyBinding/Info.plist
        PRODUCT_BUNDLE_IDENTIFIER: com.example.mybinding
        PRODUCT_NAME: MyBinding
        TARGETED_DEVICE_FAMILY: "1,2"
    scheme:
      gatherCoverageData: false
      shared: true
EOF
```

### Create the Swift Source File

Create the Swift wrapper file:

```bash
cat > macios/native/${BINDING_NAME}/${BINDING_NAME}/Dotnet${BINDING_NAME}.swift << 'EOF'
import Foundation
import UIKit

/// Main binding class exposed to .NET
@objc(DotnetMyBinding)
public class DotnetMyBinding: NSObject {
    
    /// Initialize the native library
    @objc(initialize)
    public static func initialize() {
        // Initialize your native library here
        print("MyBinding initialized")
    }
    
    /// Example synchronous method
    @objc(getVersion)
    public static func getVersion() -> String {
        return "1.0.0"
    }
    
    /// Example async method with completion handler
    @objc(fetchDataWithQuery:completion:)
    public static func fetchData(
        query: String,
        completion: @escaping (String?, NSError?) -> Void
    ) {
        // Simulate async operation
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            completion("Result for: \(query)", nil)
        }
    }
    
    /// Example view creation
    @objc(createViewWithFrame:)
    public static func createView(frame: CGRect) -> UIView {
        let view = UIView(frame: frame)
        view.backgroundColor = .systemBlue
        return view
    }
}
EOF
```

### Create Info.plist

```bash
cat > macios/native/${BINDING_NAME}/${BINDING_NAME}/Info.plist << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>$(DEVELOPMENT_LANGUAGE)</string>
    <key>CFBundleExecutable</key>
    <string>$(EXECUTABLE_NAME)</string>
    <key>CFBundleIdentifier</key>
    <string>$(PRODUCT_BUNDLE_IDENTIFIER)</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>$(PRODUCT_NAME)</string>
    <key>CFBundlePackageType</key>
    <string>$(PRODUCT_BUNDLE_PACKAGE_TYPE)</string>
    <key>CFBundleShortVersionString</key>
    <string>$(MARKETING_VERSION)</string>
    <key>CFBundleVersion</key>
    <string>$(CURRENT_PROJECT_VERSION)</string>
    <key>NSPrincipalClass</key>
    <string></string>
</dict>
</plist>
EOF
```

### Generate the Xcode Project

```bash
cd macios/native/${BINDING_NAME}
xcodegen generate
cd ../../..
```

This creates `MyBinding.xcodeproj` with all the correct build settings.

### Verify the Generated Project

```bash
# List the generated files
ls -la macios/native/${BINDING_NAME}/

# Verify the scheme was created and is shared
ls -la macios/native/${BINDING_NAME}/${BINDING_NAME}.xcodeproj/xcshareddata/xcschemes/
```

## Step 3: Create the C# Binding Project

### Create the Binding .csproj

```bash
cat > macios/${BINDING_NAME}.MaciOS.Binding/${BINDING_NAME}.MaciOS.Binding.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <IsBindingProject>true</IsBindingProject>
    
    <!-- Package metadata -->
    <PackageId>MyBinding.MaciOS</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>iOS bindings for MyBinding</Description>
  </PropertyGroup>

  <!-- Reference the Xcode project -->
  <ItemGroup>
    <XcodeProject Include="../native/MyBinding/MyBinding.xcodeproj">
      <SchemeName>MyBinding</SchemeName>
    </XcodeProject>
  </ItemGroup>

  <!-- API definition -->
  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinition.cs" />
  </ItemGroup>
</Project>
EOF
```

### Create Initial ApiDefinition.cs

```bash
cat > macios/${BINDING_NAME}.MaciOS.Binding/ApiDefinition.cs << 'EOF'
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace MyBinding
{
    // @interface DotnetMyBinding : NSObject
    [BaseType(typeof(NSObject))]
    interface DotnetMyBinding
    {
        // +(void)initialize;
        [Static]
        [Export("initialize")]
        void Initialize();

        // +(NSString * _Nonnull)getVersion;
        [Static]
        [Export("getVersion")]
        string GetVersion();

        // +(void)fetchDataWithQuery:(NSString * _Nonnull)query completion:(void (^ _Nonnull)(NSString * _Nullable, NSError * _Nullable))completion;
        [Static]
        [Export("fetchDataWithQuery:completion:")]
        [Async]
        void FetchData(string query, Action<string?, NSError?> completion);

        // +(UIView * _Nonnull)createViewWithFrame:(CGRect)frame;
        [Static]
        [Export("createViewWithFrame:")]
        UIView CreateView(CGRect frame);
    }
}
EOF
```

## Step 4: Build and Verify

### Build the Binding Project

```bash
cd macios/${BINDING_NAME}.MaciOS.Binding
dotnet build
```

This will:
1. Invoke XcodeBuild to compile the native framework
2. Create the xcframework
3. Generate the C# binding assembly

### Verify the Build Output

```bash
# Check that the xcframework was created
find bin -name "*.xcframework" -type d

# Find the generated Swift header (for updating ApiDefinition.cs later)
find bin -name "*-Swift.h" -type f
```

## Optional: Add CocoaPods Support

If your native library uses CocoaPods dependencies:

### Create Podfile

```bash
cat > macios/native/${BINDING_NAME}/Podfile << 'EOF'
platform :ios, '15.0'

target 'MyBinding' do
  use_frameworks! :linkage => :static
  
  # Add your pods here
  # pod 'FirebaseMessaging', '~> 10.0'
end

post_install do |installer|
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['BUILD_LIBRARY_FOR_DISTRIBUTION'] = 'YES'
      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'
    end
  end
end
EOF
```

### Install Pods and Update Project Reference

```bash
cd macios/native/${BINDING_NAME}
pod install
cd ../../..

# Update the binding project to use xcworkspace instead of xcodeproj
sed -i '' 's/\.xcodeproj/\.xcworkspace/g' macios/${BINDING_NAME}.MaciOS.Binding/${BINDING_NAME}.MaciOS.Binding.csproj
```

## Complete Script: Create Binding Project

Here's a complete bash script that creates everything:

```bash
#!/bin/bash
set -e

# Configuration
BINDING_NAME="${1:-MyBinding}"
BUNDLE_ID_PREFIX="${2:-com.example}"
MIN_IOS_VERSION="${3:-15.0}"

echo "Creating iOS binding project: ${BINDING_NAME}"

# Check prerequisites
if ! command -v xcodegen &> /dev/null; then
    echo "Installing xcodegen..."
    brew install xcodegen
fi

# Create directory structure
mkdir -p ${BINDING_NAME}/macios/native/${BINDING_NAME}/${BINDING_NAME}
mkdir -p ${BINDING_NAME}/macios/${BINDING_NAME}.MaciOS.Binding
cd ${BINDING_NAME}

# Create XcodeGen project spec
cat > macios/native/${BINDING_NAME}/project.yml << EOF
name: ${BINDING_NAME}
options:
  bundleIdPrefix: ${BUNDLE_ID_PREFIX}
  deploymentTarget:
    iOS: "${MIN_IOS_VERSION}"
    macOS: "12.0"
  xcodeVersion: "15.0"
  generateEmptyDirectories: true

settings:
  base:
    MARKETING_VERSION: "1.0.0"
    CURRENT_PROJECT_VERSION: "1"
    BUILD_LIBRARY_FOR_DISTRIBUTION: YES
    SKIP_INSTALL: NO
    MACH_O_TYPE: staticlib
    SWIFT_VERSION: "5.0"
    ENABLE_BITCODE: NO
    DEFINES_MODULE: YES

targets:
  ${BINDING_NAME}:
    type: framework
    platform: iOS
    sources:
      - path: ${BINDING_NAME}
        type: group
    settings:
      base:
        INFOPLIST_FILE: ${BINDING_NAME}/Info.plist
        PRODUCT_BUNDLE_IDENTIFIER: ${BUNDLE_ID_PREFIX}.${BINDING_NAME,,}
        PRODUCT_NAME: ${BINDING_NAME}
        TARGETED_DEVICE_FAMILY: "1,2"
    scheme:
      gatherCoverageData: false
      shared: true
EOF

# Create Swift wrapper
cat > macios/native/${BINDING_NAME}/${BINDING_NAME}/Dotnet${BINDING_NAME}.swift << EOF
import Foundation
import UIKit

@objc(Dotnet${BINDING_NAME})
public class Dotnet${BINDING_NAME}: NSObject {
    
    @objc(initialize)
    public static func initialize() {
        print("${BINDING_NAME} initialized")
    }
    
    @objc(getVersion)
    public static func getVersion() -> String {
        return "1.0.0"
    }
}
EOF

# Create Info.plist
cat > macios/native/${BINDING_NAME}/${BINDING_NAME}/Info.plist << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>$(DEVELOPMENT_LANGUAGE)</string>
    <key>CFBundleExecutable</key>
    <string>$(EXECUTABLE_NAME)</string>
    <key>CFBundleIdentifier</key>
    <string>$(PRODUCT_BUNDLE_IDENTIFIER)</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>$(PRODUCT_NAME)</string>
    <key>CFBundlePackageType</key>
    <string>$(PRODUCT_BUNDLE_PACKAGE_TYPE)</string>
    <key>CFBundleShortVersionString</key>
    <string>$(MARKETING_VERSION)</string>
    <key>CFBundleVersion</key>
    <string>$(CURRENT_PROJECT_VERSION)</string>
</dict>
</plist>
EOF

# Generate Xcode project
cd macios/native/${BINDING_NAME}
xcodegen generate
cd ../../..

# Create binding .csproj
cat > macios/${BINDING_NAME}.MaciOS.Binding/${BINDING_NAME}.MaciOS.Binding.csproj << EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <IsBindingProject>true</IsBindingProject>
    <PackageId>${BINDING_NAME}.MaciOS</PackageId>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <XcodeProject Include="../native/${BINDING_NAME}/${BINDING_NAME}.xcodeproj">
      <SchemeName>${BINDING_NAME}</SchemeName>
    </XcodeProject>
  </ItemGroup>

  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinition.cs" />
  </ItemGroup>
</Project>
EOF

# Create ApiDefinition.cs
cat > macios/${BINDING_NAME}.MaciOS.Binding/ApiDefinition.cs << EOF
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace ${BINDING_NAME}
{
    [BaseType(typeof(NSObject))]
    interface Dotnet${BINDING_NAME}
    {
        [Static]
        [Export("initialize")]
        void Initialize();

        [Static]
        [Export("getVersion")]
        string GetVersion();
    }
}
EOF

echo ""
echo "✅ Created ${BINDING_NAME} binding project!"
echo ""
echo "Structure:"
find . -type f -name "*.swift" -o -name "*.cs" -o -name "*.csproj" -o -name "project.yml" | sort
echo ""
echo "Next steps:"
echo "  1. cd ${BINDING_NAME}/macios/${BINDING_NAME}.MaciOS.Binding"
echo "  2. dotnet build"
echo "  3. Add your native library code to Dotnet${BINDING_NAME}.swift"
echo "  4. Update ApiDefinition.cs to match your Swift API"
```

Save as `create-ios-binding.sh` and run:
```bash
chmod +x create-ios-binding.sh
./create-ios-binding.sh MyAwesomeBinding com.mycompany 15.0
```

---

## Alternative: Create Xcode Project Manually (Without XcodeGen)

If you prefer not to use XcodeGen, you can create a minimal Xcode project using `plutil` and direct file creation. However, this is more complex and error-prone.

### Using Swift Package as Alternative

For simpler cases, you can use Swift Package Manager instead of an Xcode project:

```bash
cd macios/native
mkdir ${BINDING_NAME}
cd ${BINDING_NAME}

# Initialize Swift package
swift package init --type library --name ${BINDING_NAME}

# The binding project can reference the Package.swift
```

Then update the binding `.csproj` to use `<XcodeProject>` pointing to the directory containing `Package.swift`.

> **Note:** The `<XcodeProject>` MSBuild item supports both `.xcodeproj` and Swift Package directories.

## Step 5: Add Native Library Dependencies

Choose the appropriate method for your library's distribution:

### Option A: CocoaPods

Create `macios/native/MyBinding/Podfile`:

```ruby
platform :ios, '15.0'

target 'MyBinding' do
  use_frameworks! :linkage => :static
  
  # Add your native library pod
  pod 'FirebaseMessaging', '~> 10.0'
  # Add other dependencies as needed
  pod 'FirebaseCore'
end

post_install do |installer|
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['BUILD_LIBRARY_FOR_DISTRIBUTION'] = 'YES'
      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'
    end
  end
end
```

Install dependencies:

```bash
cd macios/native/MyBinding
pod install
# After this, open MyBinding.xcworkspace instead of .xcodeproj
```

### Option B: Swift Package Manager

In Xcode:
1. **File → Add Package Dependencies**
2. Enter the package repository URL
3. Select version rules
4. Add to your target

Or create `Package.swift`:

```swift
// swift-tools-version:5.9
import PackageDescription

let package = Package(
    name: "MyBinding",
    platforms: [.iOS(.v15), .macCatalyst(.v15)],
    products: [
        .library(name: "MyBinding", type: .static, targets: ["MyBinding"])
    ],
    dependencies: [
        .package(url: "https://github.com/example/SomeLibrary.git", from: "1.0.0")
    ],
    targets: [
        .target(
            name: "MyBinding",
            dependencies: [
                .product(name: "SomeLibrary", package: "SomeLibrary")
            ]
        )
    ]
)
```

### Option C: Manual XCFramework

1. Drag the `.xcframework` into the Xcode project
2. Ensure "Copy items if needed" is checked
3. Add to target's "Frameworks and Libraries" section
4. Set "Embed" to **Do Not Embed** (for static linking)

## Step 6: Implement the Swift Wrapper

Create `macios/native/MyBinding/MyBinding/DotnetMyBinding.swift`:

```swift
import Foundation
import UIKit
import TheNativeLibrary  // Import your native library

/// Main binding class exposed to .NET
/// The @objc attribute with explicit name ensures stable Objective-C naming
@objc(DotnetMyBinding)
public class DotnetMyBinding: NSObject {
    
    // MARK: - Initialization
    
    /// Initialize the native library
    /// Call this from your .NET app's startup (e.g., MauiProgram.cs)
    @objc(initializeWithApiKey:)
    public static func initialize(apiKey: String) {
        TheNativeLibrary.configure(withApiKey: apiKey)
    }
    
    /// Check if the library is initialized
    @objc(isInitialized)
    public static func isInitialized() -> Bool {
        return TheNativeLibrary.isConfigured
    }
    
    // MARK: - Synchronous Methods
    
    /// Get a simple value from the native library
    @objc(getVersion)
    public static func getVersion() -> String {
        return TheNativeLibrary.version
    }
    
    /// Process data and return result
    @objc(processDataWithInput:)
    public static func processData(input: String) -> String? {
        guard let result = TheNativeLibrary.process(input) else {
            return nil
        }
        return result.stringValue
    }
    
    // MARK: - Asynchronous Methods (Completion Handlers)
    
    /// Perform async operation with completion handler
    /// .NET can use [Async] attribute to generate async/await version
    @objc(fetchDataWithQuery:completion:)
    public static func fetchData(
        query: String,
        completion: @escaping (String?, NSError?) -> Void
    ) {
        TheNativeLibrary.fetch(query: query) { result in
            switch result {
            case .success(let data):
                completion(data.stringValue, nil)
            case .failure(let error):
                completion(nil, error as NSError)
            }
        }
    }
    
    /// Async method with complex result data
    @objc(performOperationWithConfig:completion:)
    public static func performOperation(
        config: NSDictionary,
        completion: @escaping (NSData?, NSError?) -> Void
    ) {
        guard let configDict = config as? [String: Any] else {
            let error = NSError(
                domain: "DotnetMyBinding",
                code: -1,
                userInfo: [NSLocalizedDescriptionKey: "Invalid configuration"]
            )
            completion(nil, error)
            return
        }
        
        TheNativeLibrary.performOperation(config: configDict) { result in
            switch result {
            case .success(let data):
                completion(data, nil)
            case .failure(let error):
                completion(nil, error as NSError)
            }
        }
    }
    
    // MARK: - View Creation
    
    /// Create a native view to embed in .NET MAUI
    /// Return UIView for cross-platform compatibility
    @objc(createViewWithFrame:)
    public static func createView(frame: CGRect) -> UIView {
        let nativeView = TheNativeLibrary.createCustomView()
        nativeView.frame = frame
        return nativeView
    }
    
    /// Create a configured view with options
    @objc(createViewWithFrame:options:)
    public static func createView(frame: CGRect, options: NSDictionary) -> UIView {
        let config = options as? [String: Any] ?? [:]
        let nativeView = TheNativeLibrary.createCustomView(options: config)
        nativeView.frame = frame
        return nativeView
    }
    
    // MARK: - Delegate/Callback Pattern
    
    private static var callbackHandler: ((String) -> Void)?
    
    /// Register a callback for events
    /// .NET will pass an Action<string> that gets invoked
    @objc(registerCallbackWithHandler:)
    public static func registerCallback(handler: @escaping (String) -> Void) {
        callbackHandler = handler
        TheNativeLibrary.setEventHandler { event in
            callbackHandler?(event.description)
        }
    }
    
    /// Unregister the callback
    @objc(unregisterCallback)
    public static func unregisterCallback() {
        callbackHandler = nil
        TheNativeLibrary.setEventHandler(nil)
    }
}
```

### Swift Wrapper Design Guidelines

#### Type Mapping Rules

Only use types that .NET already knows how to marshal:

| Swift Type | Objective-C Type | C# Type |
|------------|------------------|---------|
| `String` | `NSString *` | `string` |
| `Bool` | `BOOL` | `bool` |
| `Int`, `Int32` | `int` | `int` |
| `Int64` | `long long` | `long` |
| `Double` | `double` | `double` |
| `Float` | `float` | `float` |
| `Data` | `NSData *` | `NSData` |
| `[String: Any]` | `NSDictionary *` | `NSDictionary` |
| `[Any]` | `NSArray *` | `NSArray` |
| `UIView` | `UIView *` | `UIView` |
| `UIImage` | `UIImage *` | `UIImage` |
| `URL` | `NSURL *` | `NSUrl` |
| Custom Class | Must inherit `NSObject` | Interface with `[BaseType]` |

#### Required Annotations

```swift
// Class: Must be public and have @objc with explicit name
@objc(ClassName)
public class ClassName: NSObject {

    // Method: Must be public with @objc selector
    @objc(methodNameWithParam:anotherParam:)
    public func methodName(param: String, anotherParam: Int) -> Bool {
        // Implementation
    }
    
    // Static method
    @objc(staticMethodWithValue:)
    public static func staticMethod(value: String) -> String {
        // Implementation
    }
    
    // Property (read-only)
    @objc(propertyName)
    public var propertyName: String {
        return "value"
    }
    
    // Property (read-write)
    @objc
    public var readWriteProperty: String = ""
}
```

#### Completion Handler Pattern

For async operations, use completion handlers that .NET can convert to `async`/`await`:

```swift
// Swift
@objc(operationWithInput:completion:)
public static func operation(
    input: String,
    completion: @escaping (String?, NSError?) -> Void  // Result, Error
) {
    // Async work...
    DispatchQueue.main.async {
        completion(result, nil)  // Success
        // OR
        completion(nil, error as NSError)  // Failure
    }
}
```

```csharp
// C# ApiDefinition.cs - Add [Async] for automatic async wrapper
[Static]
[Export("operationWithInput:completion:")]
[Async]
void Operation(string input, Action<string?, NSError?> completion);

// Usage in C#
var result = await DotnetMyBinding.OperationAsync("input");
```

#### Error Handling Pattern

Always convert errors to `NSError` for proper propagation:

```swift
@objc(riskyOperationWithCompletion:)
public static func riskyOperation(completion: @escaping (Bool, NSError?) -> Void) {
    do {
        try TheNativeLibrary.riskyOperation()
        completion(true, nil)
    } catch {
        let nsError = NSError(
            domain: "DotnetMyBinding",
            code: (error as NSError).code,
            userInfo: [
                NSLocalizedDescriptionKey: error.localizedDescription,
                NSUnderlyingErrorKey: error
            ]
        )
        completion(false, nsError)
    }
}
```

## Step 7: Create the C# Binding Project (If Not Using Script)

If you created the project using the script in Step 1-4, skip to Step 8.

Create `macios/MyBinding.MaciOS.Binding/MyBinding.MaciOS.Binding.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net9.0-ios;net9.0-maccatalyst</TargetFrameworks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <IsBindingProject>true</IsBindingProject>
    
    <!-- Optional: Package metadata for NuGet -->
    <PackageId>MyBinding.MaciOS</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>iOS bindings for MyLibrary</Description>
  </PropertyGroup>

  <!-- Reference the Xcode project - MSBuild will build it automatically -->
  <ItemGroup>
    <XcodeProject Include="../native/MyBinding/MyBinding.xcodeproj">
      <SchemeName>MyBinding</SchemeName>
      <!-- Optional overrides -->
      <!-- <Configuration>Release</Configuration> -->
      <!-- <Kind>Framework</Kind> -->
      <!-- <SmartLink>true</SmartLink> -->
    </XcodeProject>
  </ItemGroup>

  <!-- If using xcworkspace (CocoaPods), reference it instead -->
  <!--
  <ItemGroup>
    <XcodeProject Include="../native/MyBinding/MyBinding.xcworkspace">
      <SchemeName>MyBinding</SchemeName>
    </XcodeProject>
  </ItemGroup>
  -->

  <!-- API definition file -->
  <ItemGroup>
    <ObjcBindingApiDefinition Include="ApiDefinition.cs" />
  </ItemGroup>
</Project>
```

### XcodeProject Properties

| Property | Description | Default |
|----------|-------------|---------|
| `SchemeName` | Xcode scheme to build | Required |
| `Configuration` | Build configuration | `Release` |
| `Kind` | `Framework` or `Static` | Auto-detected |
| `SmartLink` | Enable smart linking | `true` |
| `ForceLoad` | Force load all symbols | `false` |

## Step 8: Build and Generate API Definition

### Initial Build

Build the binding project to compile the native framework:

```bash
cd macios/MyBinding.MaciOS.Binding
dotnet build
```

This creates the xcframework at:
```
bin/Debug/net9.0-ios/MyBinding.MaciOS.Binding.resources/MyBindingiOS.xcframework/
```

### Locate the Generated Swift Header

After building, find the generated Objective-C header:

```bash
# Find the Swift header
find bin -name "*-Swift.h" -type f

# Typical location:
# bin/Debug/net9.0-ios/MyBinding.MaciOS.Binding.resources/
#   MyBindingiOS.xcframework/ios-arm64/MyBinding.framework/Headers/MyBinding-Swift.h
```

### Generate ApiDefinition.cs with Objective Sharpie

Install Objective Sharpie if not already installed:

```bash
brew install --cask objectivesharpie
```

Check available iOS SDKs:

```bash
sharpie xcode -sdks
```

Generate bindings:

```bash
# Set variables for clarity
HEADER_PATH="bin/Debug/net9.0-ios/MyBinding.MaciOS.Binding.resources/MyBindingiOS.xcframework/ios-arm64/MyBinding.framework/Headers/MyBinding-Swift.h"
SDK_VERSION="iphoneos18.0"  # Use your installed SDK version
NAMESPACE="MyBinding"

sharpie bind \
  --output=sharpie-output \
  --namespace=$NAMESPACE \
  --sdk=$SDK_VERSION \
  --scope=Headers \
  "$HEADER_PATH"
```

### Review and Clean Up Generated Code

The generated `ApiDefinition.cs` requires cleanup:

```csharp
using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace MyBinding
{
    // @interface DotnetMyBinding : NSObject
    [BaseType(typeof(NSObject))]
    interface DotnetMyBinding
    {
        // +(void)initializeWithApiKey:(NSString * _Nonnull)apiKey;
        [Static]
        [Export("initializeWithApiKey:")]
        void Initialize(string apiKey);

        // +(BOOL)isInitialized;
        [Static]
        [Export("isInitialized")]
        bool IsInitialized { get; }

        // +(NSString * _Nonnull)getVersion;
        [Static]
        [Export("getVersion")]
        string GetVersion();

        // +(NSString * _Nullable)processDataWithInput:(NSString * _Nonnull)input;
        [Static]
        [Export("processDataWithInput:")]
        [return: NullAllowed]
        string ProcessData(string input);

        // +(void)fetchDataWithQuery:(NSString * _Nonnull)query 
        //                completion:(void (^ _Nonnull)(NSString * _Nullable, NSError * _Nullable))completion;
        [Static]
        [Export("fetchDataWithQuery:completion:")]
        [Async]  // Generates FetchDataAsync method
        void FetchData(string query, Action<string?, NSError?> completion);

        // +(void)performOperationWithConfig:(NSDictionary * _Nonnull)config 
        //                        completion:(void (^ _Nonnull)(NSData * _Nullable, NSError * _Nullable))completion;
        [Static]
        [Export("performOperationWithConfig:completion:")]
        [Async]
        void PerformOperation(NSDictionary config, Action<NSData?, NSError?> completion);

        // +(UIView * _Nonnull)createViewWithFrame:(CGRect)frame;
        [Static]
        [Export("createViewWithFrame:")]
        UIView CreateView(CGRect frame);

        // +(UIView * _Nonnull)createViewWithFrame:(CGRect)frame options:(NSDictionary * _Nonnull)options;
        [Static]
        [Export("createViewWithFrame:options:")]
        UIView CreateView(CGRect frame, NSDictionary options);

        // +(void)registerCallbackWithHandler:(void (^ _Nonnull)(NSString * _Nonnull))handler;
        [Static]
        [Export("registerCallbackWithHandler:")]
        void RegisterCallback(Action<string> handler);

        // +(void)unregisterCallback;
        [Static]
        [Export("unregisterCallback")]
        void UnregisterCallback();
    }
}
```

### Common Cleanup Tasks

| Issue | Solution |
|-------|----------|
| Missing namespace | Add `namespace MyBinding { ... }` |
| `[Verify]` attributes | Review each, remove after confirming correctness |
| `InitWithCoder` constructors | Remove - conflicts with linker |
| Protocol type mismatches | Use interface types (e.g., `ICAAnimation`) |
| Missing `[NullAllowed]` | Add for nullable parameters/returns |
| Completion handlers | Add `[Async]` attribute for async generation |

## Step 9: Build the Final Binding

```bash
cd macios/MyBinding.MaciOS.Binding
dotnet build -c Release
```

Verify the output:
```bash
ls -la bin/Release/net9.0-ios/
# Should contain: MyBinding.MaciOS.Binding.dll and resources
```

## Step 10: Use in Your MAUI App

### Add Project Reference

In your MAUI app's `.csproj`:

```xml
<ItemGroup Condition="$(TargetFramework.Contains('ios')) Or $(TargetFramework.Contains('maccatalyst'))">
  <ProjectReference Include="..\..\macios\MyBinding.MaciOS.Binding\MyBinding.MaciOS.Binding.csproj" />
</ItemGroup>
```

### Initialize in MauiProgram.cs

```csharp
using Microsoft.Maui.Hosting;

#if IOS || MACCATALYST
using MyBinding;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if IOS || MACCATALYST
        // Initialize the native library
        DotnetMyBinding.Initialize("your-api-key");
#endif

        return builder.Build();
    }
}
```

### Use Async APIs

```csharp
#if IOS || MACCATALYST
using MyBinding;
#endif

public partial class MainPage : ContentPage
{
    private async void OnFetchClicked(object sender, EventArgs e)
    {
#if IOS || MACCATALYST
        try
        {
            // Using the async version generated by [Async] attribute
            var result = await DotnetMyBinding.FetchDataAsync("my query");
            await DisplayAlert("Success", result ?? "No data", "OK");
        }
        catch (NSErrorException ex)
        {
            await DisplayAlert("Error", ex.Error.LocalizedDescription, "OK");
        }
#endif
    }

    private void OnCreateViewClicked(object sender, EventArgs e)
    {
#if IOS || MACCATALYST
        var nativeView = DotnetMyBinding.CreateView(new CoreGraphics.CGRect(0, 0, 300, 200));
        
        // Add to a MAUI view using a custom handler or platform view
        // This requires additional platform-specific integration
#endif
    }
}
```

### Register Callbacks

```csharp
#if IOS || MACCATALYST
protected override void OnAppearing()
{
    base.OnAppearing();
    DotnetMyBinding.RegisterCallback((message) =>
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = $"Event: {message}";
        });
    });
}

protected override void OnDisappearing()
{
    base.OnDisappearing();
    DotnetMyBinding.UnregisterCallback();
}
#endif
```

# Updating Bindings When Native SDK Changes

## Step-by-step Update Process

### 1. Update Native Dependency Version

**CocoaPods:**
```ruby
# Podfile
pod 'FirebaseMessaging', '~> 11.0'  # Updated version
```

```bash
cd macios/native/MyBinding
pod update
```

**Swift Package Manager:**
Update version in Xcode's Package Dependencies or `Package.swift`

**Manual XCFramework:**
Replace the xcframework file with the new version

### 2. Update Swift Wrapper (If Needed)

Review release notes for the native library and update `DotnetMyBinding.swift`:
- Add new methods for new APIs
- Update method signatures for changed APIs
- Remove deprecated API wrappers
- Handle any breaking changes

### 3. Regenerate API Definition

```bash
# Clean and rebuild
cd macios/MyBinding.MaciOS.Binding
dotnet clean
dotnet build

# Regenerate Objective Sharpie output
sharpie bind \
  --output=sharpie-output-new \
  --namespace=MyBinding \
  --sdk=iphoneos18.0 \
  --scope=Headers \
  "bin/Debug/net9.0-ios/MyBinding.MaciOS.Binding.resources/MyBindingiOS.xcframework/ios-arm64/MyBinding.framework/Headers/MyBinding-Swift.h"
```

### 4. Diff and Merge Changes

Compare the new Sharpie output with existing `ApiDefinition.cs`:

```bash
diff ApiDefinition.cs sharpie-output-new/ApiDefinitions.cs
```

Manually merge:
- Add new method bindings
- Update changed signatures
- Remove deleted methods
- Preserve custom attributes (`[Async]`, `[NullAllowed]`, etc.)

### 5. Test the Updated Bindings

```bash
dotnet build -c Release
dotnet test  # If you have unit tests
```

Run the sample app to verify functionality.

# Troubleshooting

## Build Errors

### "Framework not found" / "Library not found"

**Causes & Solutions:**
1. **XCFramework path incorrect** - Verify the path in `<XcodeProject>` or `<NativeReference>`
2. **Missing architectures** - Ensure xcframework includes arm64 (device) and arm64/x86_64 (simulator)
3. **CocoaPods not installed** - Run `pod install` in the native directory

```bash
# Verify xcframework architectures
lipo -info path/to/Framework.framework/Framework
```

### "Undefined symbols for architecture"

**Causes & Solutions:**
1. **Missing linked frameworks** - Add system frameworks to Xcode project's "Link Binary with Libraries"
2. **Static vs Dynamic mismatch** - Ensure consistent linkage (all static or all dynamic)
3. **Symbol visibility** - Verify Swift classes/methods are `public` and have `@objc`

```xml
<!-- Force load symbols if needed -->
<XcodeProject Include="...">
  <SchemeName>MyBinding</SchemeName>
  <ForceLoad>true</ForceLoad>
  <SmartLink>false</SmartLink>
</XcodeProject>
```

### "No type or protocol named..."

**Causes & Solutions:**
1. **Missing import** - Add required imports to `ApiDefinition.cs` (`using UIKit;`, etc.)
2. **Protocol vs Interface** - Use interface types (`ICAAnimation` not `CAAnimation`)
3. **Namespace mismatch** - Verify namespace matches between wrapper and binding

### "Duplicate symbol" / "Symbol already defined"

**Causes & Solutions:**
1. **Multiple references to same framework** - Check for duplicate `<NativeReference>` entries
2. **Conflicting dependency versions** - Resolve CocoaPods/SPM version conflicts
3. **InitWithCoder constructor** - Remove from ApiDefinition.cs (auto-generated by linker)

### Objective Sharpie Errors

**"Unable to find SDK":**
```bash
# List available SDKs
sharpie xcode -sdks

# Update Xcode command line tools
xcode-select --install
sudo xcode-select -s /Applications/Xcode.app
```

**"Parse error in header":**
- Header may use features Sharpie doesn't support
- Simplify the Swift wrapper to use basic types
- Use `--scope=Headers` to limit parsing

## Runtime Errors

### "Native class hasn't been loaded"

**Causes & Solutions:**
1. **Framework not embedded** - Check that native resources are included in app bundle
2. **Static library not linked** - Verify `<ForceLoad>true</ForceLoad>` is set
3. **Missing Objective-C class registration** - Ensure `@objc(ClassName)` annotation is present

### "unrecognized selector sent to instance"

**Causes & Solutions:**
1. **Selector mismatch** - Verify `[Export("selector:")]` matches Swift `@objc(selector:)` exactly
2. **Method signature mismatch** - Check parameter count and types match
3. **Static vs instance method** - Ensure `[Static]` attribute is correct

### "Library not loaded: @rpath/..."

**Causes & Solutions:**
1. **Swift runtime missing** - Add linker flags for Swift libraries
2. **Framework not embedded** - Set "Embed & Sign" in Xcode for dynamic frameworks
3. **rpath not set** - Add `-Wl,-rpath -Wl,@executable_path/Frameworks`

### Callbacks Not Working

**Causes & Solutions:**
1. **Callback on wrong thread** - Use `DispatchQueue.main.async` in Swift for UI updates
2. **Callback garbage collected** - Store strong reference to callback handler
3. **Missing `@escaping`** - Completion handlers must be `@escaping` in Swift

## IntelliSense Issues

**IntelliSense shows errors but project compiles:**
This is expected behavior. Binding projects don't use source generators. The solution:
1. Build the binding project first
2. Reload the solution/project
3. IntelliSense may still show errors - trust the compiler

# Quick Reference

## ApiDefinition Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[BaseType(typeof(NSObject))]` | Specifies base class | `[BaseType(typeof(UIView))]` |
| `[Static]` | Static method/property | `[Static] [Export("shared")]` |
| `[Export("selector:")]` | Objective-C selector | `[Export("doSomethingWithValue:")]` |
| `[Async]` | Generate async wrapper | On completion handler methods |
| `[NullAllowed]` | Nullable parameter/return | `[return: NullAllowed]` |
| `[Protocol]` | Objective-C protocol | `[Protocol] interface IMyDelegate` |
| `[Model]` | Protocol implementation | Combined with `[Protocol]` |
| `[Abstract]` | Required protocol method | In protocol interface |
| `[Internal]` | Don't expose publicly | Hide helper methods |
| `[Wrap("...")]` | Wrap with helper | Strongly-typed helpers |
| `[Sealed]` | Prevent subclassing | On final classes |

## XcodeProject MSBuild Properties

```xml
<XcodeProject Include="path/to/Project.xcodeproj">
  <SchemeName>MyScheme</SchemeName>           <!-- Required: Xcode scheme -->
  <Configuration>Release</Configuration>       <!-- Build configuration -->
  <Kind>Framework</Kind>                       <!-- Framework or Static -->
  <SmartLink>true</SmartLink>                  <!-- Enable smart linking -->
  <ForceLoad>false</ForceLoad>                 <!-- Force load all symbols -->
</XcodeProject>
```

## NativeReference MSBuild Properties (Traditional Bindings)

```xml
<NativeReference Include="Library.xcframework">
  <Kind>Framework</Kind>                       <!-- Framework or Static -->
  <Frameworks>Foundation UIKit</Frameworks>    <!-- Required Apple frameworks -->
  <LinkerFlags>-lsqlite3</LinkerFlags>         <!-- Additional linker flags -->
  <SmartLink>true</SmartLink>                  <!-- Enable smart linking -->
  <ForceLoad>false</ForceLoad>                 <!-- Force load all symbols -->
  <IsCxx>false</IsCxx>                         <!-- C++ library -->
</NativeReference>
```

## Common Swift-to-C# Type Mappings

```swift
// Swift                          // C# ApiDefinition
String                            string
String?                           [NullAllowed] string
Bool                              bool
Int / Int32                       nint / int
Int64                             long
Double                            double
Float                             float
Data                              NSData
[String: Any]                     NSDictionary
[Any]                             NSArray
URL                               NSUrl
Date                              NSDate
UIView                            UIView
UIImage                           UIImage
CGRect                            CGRect
CGPoint                           CGPoint
CGSize                            CGSize
(Result, Error?) -> Void          Action<Result?, NSError?>
```

# Resources

## Official Documentation
- [Native Library Interop - .NET Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/native-library-interop)
- [iOS Binding Project Migration](https://learn.microsoft.com/dotnet/maui/migration/ios-binding-projects)
- [Binding Objective-C Libraries](https://learn.microsoft.com/xamarin/cross-platform/macios/binding/)
- [Objective Sharpie](https://learn.microsoft.com/previous-versions/xamarin/cross-platform/macios/binding/objective-sharpie/)

## Tools
- [XcodeGen](https://github.com/yonaskolb/XcodeGen) - Generate Xcode projects from YAML specification

## Templates and Examples
- [Maui.NativeLibraryInterop Repository](https://github.com/CommunityToolkit/Maui.NativeLibraryInterop)
- [CommunityToolkit.Maui Bindings](https://github.com/CommunityToolkit/Maui)

## Related Skills
- See [docs/ios-bindings-guide.md](../../../docs/ios-bindings-guide.md) for comprehensive reference
- See [docs/android-bindings-guide.md](../../../docs/android-bindings-guide.md) for Android bindings

# Appendix A: Using the Community Toolkit Template (Alternative)

If you prefer to start from an existing template rather than creating from scratch:

```bash
git clone https://github.com/CommunityToolkit/Maui.NativeLibraryInterop
cp -r Maui.NativeLibraryInterop/template ./MyBinding
cd MyBinding

# Rename files and update references
find . -name "*NewBinding*" -exec bash -c 'mv "$0" "${0//NewBinding/MyBinding}"' {} \;
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.swift" -o -name "*.yml" \) | xargs sed -i '' 's/NewBinding/MyBinding/g'
```

The template includes pre-configured:
- Xcode project with correct build settings
- Binding .csproj with XcodeProject reference
- Sample MAUI app
- GitHub Actions CI/CD workflows

# Output Format

When assisting with iOS slim bindings, provide:

1. **Project structure** - File/folder layout for the binding
2. **Swift wrapper code** - Complete `DotnetMyBinding.swift` implementation
3. **Xcode configuration** - Build settings and dependency setup
4. **C# binding project** - `.csproj` and `ApiDefinition.cs` files
5. **Usage examples** - How to call the binding from MAUI/C#
6. **Troubleshooting guidance** - Common issues and solutions for the specific library

Always verify:
- Swift classes have `@objc(ClassName)` annotations
- Methods have `@objc(selector:)` annotations matching Objective-C conventions
- Types are marshallable between Swift and C#
- Async operations use completion handlers with `[Async]` attribute
- Error handling uses `NSError` for proper propagation
