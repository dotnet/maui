---
name: android-slim-bindings
description: Create and update slim/native platform interop bindings for Android in .NET MAUI and .NET for Android projects. Guides through creating Java/Kotlin wrappers, configuring Gradle projects, resolving Maven dependencies, generating C# bindings, and integrating native Android libraries using the Native Library Interop (NLI) approach. Use when asked about Android bindings, AAR/JAR integration, Kotlin interop, Maven dependencies, or bridging native Android SDKs to .NET.
---

# When to use this skill

Activate this skill when the user asks:
- How do I create Android bindings for a native library?
- How do I wrap an Android SDK for use in .NET MAUI?
- How do I create slim bindings for Android?
- How do I use Native Library Interop for Android?
- How do I bind a Kotlin library to .NET?
- How do I bind a Java library to .NET?
- How do I integrate an AAR or JAR into .NET MAUI?
- How do I create a Java/Kotlin wrapper for a native Android library?
- How do I update Android bindings when the native SDK changes?
- How do I fix Android binding build errors?
- How do I expose native Android APIs to C#?
- How do I resolve Maven dependencies for Android bindings?
- How do I handle AndroidX dependencies in bindings?
- How do I fix "Java dependency is not satisfied" errors?
- How do I use AndroidMavenLibrary in my binding project?

# Overview

This skill guides the creation of **Native Library Interop (Slim Bindings)** for Android. This modern approach creates a thin native Java/Kotlin wrapper exposing only the APIs you need from a native Android library, making bindings easier to create and maintain.

## When to Use Slim Bindings vs Traditional Bindings

| Scenario | Recommended Approach |
|----------|---------------------|
| Need only a subset of library functionality | **Slim Bindings** ✓ |
| Easier maintenance when SDK updates | **Slim Bindings** ✓ |
| Prefer working in Java/Kotlin for wrapper | **Slim Bindings** ✓ |
| Better isolation from breaking changes | **Slim Bindings** ✓ |
| Complex libraries with many dependencies | **Slim Bindings** ✓ |
| Need entire library API surface | Traditional Bindings |
| Creating bindings for third-party developers | Traditional Bindings |
| Already maintaining traditional bindings | Traditional Bindings |

# Inputs

| Parameter | Required | Example | Notes |
|-----------|----------|---------|-------|
| libraryName | yes | `FirebaseMessaging`, `OkHttp` | Name of the native Android library to bind |
| bindingProjectName | yes | `MyBinding.Android` | Name for the C# binding project |
| dependencySource | no | `maven`, `aar`, `jar` | How the native library is distributed |
| targetFrameworks | no | `net9.0-android` | Target frameworks (default: latest .NET Android) |
| exposedApis | no | List of specific APIs | Which native APIs to expose (helps scope the wrapper) |
| mavenCoordinates | no | `com.example:library:1.0.0` | Maven coordinates if library is from Maven repository |

# Project Structure

The recommended project structure for Native Library Interop:

```
MyBinding/
├── android/
│   ├── native/                              # Android Studio/Gradle project
│   │   ├── app/
│   │   │   ├── src/main/
│   │   │   │   └── java/com/example/mybinding/
│   │   │   │       └── DotnetMyBinding.java  # Java wrapper implementation
│   │   │   │   └── kotlin/com/example/mybinding/
│   │   │   │       └── DotnetMyBinding.kt    # Or Kotlin wrapper
│   │   │   └── build.gradle.kts
│   │   ├── settings.gradle.kts
│   │   └── build.gradle.kts
│   └── MyBinding.Android.Binding/
│       ├── MyBinding.Android.Binding.csproj
│       └── Transforms/
│           └── Metadata.xml
├── sample/
│   └── MauiSample/                          # Sample MAUI app
│       ├── MauiSample.csproj
│       └── MainPage.xaml.cs
└── README.md
```

# Step-by-step Process

## Step 1: Analyze Dependencies First

Before creating any bindings, analyze the complete dependency tree of your target library. This is the most critical step and where most binding projects fail.

### Prerequisites

Ensure you have:
- JDK 17+
- Android SDK with `ANDROID_HOME` environment variable set
- .NET SDK 9+ (recommended for Java Dependency Verification)

> **Note:** You don't need Gradle installed globally—we'll use the Gradle Wrapper which downloads the correct version automatically.

## Step 2: Create the Android Library Project

There are multiple approaches to create the native Android wrapper project. Choose the one that best fits your workflow.

### Option A: Use Android Studio (Recommended for GUI)

The most reliable way to create a properly configured Android library:

1. **Open Android Studio**
2. **File → New → New Project**
3. Select **"No Activity"** template
4. Configure:
   - Name: `MyBindingNative`
   - Package name: `com.example.mybinding`
   - Language: Java or Kotlin
   - Minimum SDK: API 21
5. **File → New → New Module**
6. Select **"Android Library"**
7. Name it `app` (or your preferred module name)

This creates a properly structured project with the latest Gradle plugin versions.

### Option B: Use `gradle init` with Wrapper (CLI)

Use Gradle's built-in initialization to scaffold a project, then convert it to Android:

```bash
# Set your binding name
BINDING_NAME="MyBinding"
PACKAGE_NAME="com.example.mybinding"

# Create directory structure
mkdir -p ${BINDING_NAME}/android/native
mkdir -p ${BINDING_NAME}/android/${BINDING_NAME}.Android.Binding/Transforms
mkdir -p ${BINDING_NAME}/sample/MauiSample

cd ${BINDING_NAME}/android/native

# Download and use Gradle wrapper (no global Gradle install needed)
# This downloads gradle-wrapper.jar and creates gradlew scripts
curl -sL https://services.gradle.org/distributions/gradle-9.2.1-bin.zip -o gradle.zip
unzip -q gradle.zip
./gradle-9.2.1/bin/gradle wrapper --gradle-version 9.2.1
rm -rf gradle.zip gradle-9.2.1

# Initialize a basic Kotlin library project
./gradlew init --type kotlin-library --dsl kotlin --project-name ${BINDING_NAME}Native --package ${PACKAGE_NAME} --no-split-project --java-version 17

# The project needs to be converted to Android Library (see next section)
```

> **Note:** `gradle init` creates a standard JVM library, not an Android library. You'll need to modify the generated files to add Android support (see "Converting to Android Library" below).

### Option C: Clone from Template Repository

Use the Community Toolkit template as a starting point:

```bash
BINDING_NAME="MyBinding"

# Clone the template
git clone https://github.com/AdrianSimionescu/NativeLibraryInterop-Template.git ${BINDING_NAME}
cd ${BINDING_NAME}

# Or use the official CommunityToolkit repo structure
git clone --depth 1 https://github.com/AdrianSimionescu/NativeLibraryInterop-Template.git ${BINDING_NAME}

# Rename files and references
find . -type f -name "*.gradle*" -exec sed -i '' 's/TemplateBinding/'"${BINDING_NAME}"'/g' {} \;
```

### Option D: Manual Creation with Gradle Wrapper

For full control, create the project structure manually but use proper Gradle wrapper initialization:

```bash
BINDING_NAME="MyBinding"
PACKAGE_NAME="com.example.mybinding"
PACKAGE_PATH="${PACKAGE_NAME//./\/}"

# Create directory structure
mkdir -p ${BINDING_NAME}/android/native/app/src/main/java/${PACKAGE_PATH}
mkdir -p ${BINDING_NAME}/android/${BINDING_NAME}.Android.Binding/Transforms
mkdir -p ${BINDING_NAME}/sample/MauiSample

cd ${BINDING_NAME}/android/native

# Create Gradle wrapper (downloads correct Gradle version automatically)
# Using gradle wrapper from a temporary init
mkdir -p tmp && cd tmp
curl -sL https://services.gradle.org/distributions/gradle-9.2.1-bin.zip -o gradle.zip
unzip -q gradle.zip && ./gradle-9.2.1/bin/gradle wrapper --gradle-version 9.2.1
mv gradlew gradlew.bat gradle ../ && cd .. && rm -rf tmp

# Now create the project files (see below)
```

### Creating Gradle Build Files

After setting up the wrapper, create the build files. These files define the project structure and can be version-controlled:

**settings.gradle.kts:**

```kotlin
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
        // Add custom repositories as needed
        // maven { url = uri("https://jitpack.io") }
    }
}

rootProject.name = "MyBindingNative"
include(":app")
```

**build.gradle.kts (root):**

```kotlin
plugins {
    // Use version catalog or explicit versions
    // Check latest versions at: https://developer.android.com/build/releases/gradle-plugin
    id("com.android.library") version "8.2.2" apply false
    id("org.jetbrains.kotlin.android") version "1.9.22" apply false
}
```

**app/build.gradle.kts:**

```kotlin
plugins {
    id("com.android.library")
    // Include kotlin plugin only if using Kotlin
    // id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.example.mybinding"
    
    // Check latest SDK versions: https://developer.android.com/about/versions
    compileSdk = 34

    defaultConfig {
        minSdk = 21
        consumerProguardFiles("consumer-rules.pro")
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    
    // Only needed if using Kotlin
    // kotlinOptions {
    //     jvmTarget = "17"
    // }
}

dependencies {
    // Add the native library you want to wrap
    // implementation("com.squareup.okhttp3:okhttp:4.12.0")
    
    // Common dependencies (only add what you need)
    // implementation("androidx.core:core-ktx:1.12.0")
}
```

### Checking for Latest Versions

Always use current versions for Gradle plugins and dependencies:

```bash
# Check latest Android Gradle Plugin version
curl -s "https://maven.google.com/web/index.html" | grep -o 'com.android.tools.build:gradle:[0-9.]*' | head -1

# Or check the release notes
open "https://developer.android.com/build/releases/gradle-plugin"

# Check latest Kotlin version
curl -s "https://api.github.com/repos/JetBrains/kotlin/releases/latest" | grep '"tag_name"' | cut -d'"' -f4
```

## Step 3: Analyze the Full Dependency Tree

Add your target library to the dependencies and run:

```bash
# Get full dependency tree
./gradlew app:dependencies --configuration releaseRuntimeClasspath

# Get flat list of all dependencies
./gradlew app:dependencies --configuration releaseRuntimeClasspath | grep -E "^[+\\\\]" | sed 's/.*--- //' | sort -u

# For insight into a specific dependency
./gradlew app:dependencyInsight --dependency kotlin-stdlib --configuration releaseRuntimeClasspath
```

**Understanding the output:**
- `->` indicates version conflict resolution (e.g., `1.9.10 -> 1.9.21`)
- `(*)` indicates the dependency was already shown elsewhere (deduplication)
- `(c)` indicates a constraint

### Document Your Dependencies

Create a dependency mapping document:

| Maven Artifact | Version | NuGet Package | Strategy |
|----------------|---------|---------------|----------|
| `androidx.core:core` | 1.12.0 | `Xamarin.AndroidX.Core` | NuGet |
| `org.jetbrains.kotlin:kotlin-stdlib` | 1.9.22 | `Xamarin.Kotlin.StdLib` | NuGet |
| `com.example:internal-lib` | 1.0.0 | N/A | `Bind="false"` |
| `org.jetbrains:annotations` | 23.0.0 | N/A | Ignore |

## Step 4: Create the Wrapper Class

Choose Java or Kotlin based on your preference and the native library's language.

### Java Wrapper

Create `app/src/main/java/com/example/mybinding/DotnetMyBinding.java`:

```bash
mkdir -p app/src/main/java/com/example/mybinding

cat > app/src/main/java/com/example/mybinding/DotnetMyBinding.java << 'EOF'
package com.example.mybinding;

import android.content.Context;
import android.util.Log;
import android.view.View;

/**
 * Main binding class exposed to .NET
 * Keep method signatures simple for easy marshalling
 */
public class DotnetMyBinding {
    private static final String TAG = "DotnetMyBinding";
    private static boolean initialized = false;

    // MARK: - Initialization

    /**
     * Initialize the native library
     * Call this from your .NET app's startup
     */
    public static void initialize(Context context, boolean debug) {
        if (initialized) {
            Log.w(TAG, "Already initialized");
            return;
        }
        
        // Initialize your native library here
        // TheNativeLibrary.configure(context, debug);
        
        initialized = true;
        Log.d(TAG, "MyBinding initialized");
    }

    /**
     * Check if the library is initialized
     */
    public static boolean isInitialized() {
        return initialized;
    }

    // MARK: - Synchronous Methods

    /**
     * Get library version
     */
    public static String getVersion() {
        return "1.0.0";
    }

    /**
     * Process data synchronously
     * @param input The input string to process
     * @return The processed result, or null on failure
     */
    public static String processData(String input) {
        if (!initialized) {
            throw new IllegalStateException("Must call initialize() first");
        }
        
        // Process using native library
        // return TheNativeLibrary.process(input);
        return "Processed: " + input;
    }

    // MARK: - Asynchronous Methods (Callback Pattern)

    /**
     * Callback interface for async operations
     * .NET will implement this interface
     */
    public interface DataCallback {
        void onSuccess(String result);
        void onError(String error);
    }

    /**
     * Fetch data asynchronously
     * @param query The query string
     * @param callback Callback for result/error
     */
    public static void fetchDataAsync(String query, final DataCallback callback) {
        if (!initialized) {
            callback.onError("Not initialized");
            return;
        }

        // Simulate async operation (replace with real native library call)
        new Thread(() -> {
            try {
                Thread.sleep(100); // Simulate network delay
                String result = "Result for: " + query;
                callback.onSuccess(result);
            } catch (Exception e) {
                callback.onError(e.getMessage());
            }
        }).start();
    }

    /**
     * Callback with multiple result types
     */
    public interface ComplexCallback {
        void onSuccess(String data, int count, boolean hasMore);
        void onError(int errorCode, String errorMessage);
    }

    /**
     * Complex async operation with multiple callback parameters
     */
    public static void performComplexOperation(
            String input, 
            int options, 
            final ComplexCallback callback) {
        
        // Perform operation...
        callback.onSuccess("data", 42, true);
    }

    // MARK: - View Creation

    /**
     * Create a native view to embed in .NET MAUI
     * @param context Android context
     * @return The native view
     */
    public static View createView(Context context) {
        // Create and return your native view
        // return new TheNativeLibrary.CustomView(context);
        
        View view = new View(context);
        view.setBackgroundColor(0xFF0066CC);
        return view;
    }

    // MARK: - Event Handling

    /**
     * Event listener interface
     */
    public interface EventListener {
        void onEvent(String eventType, String eventData);
    }

    private static EventListener eventListener;

    /**
     * Register an event listener
     */
    public static void setEventListener(EventListener listener) {
        eventListener = listener;
        
        // Wire up to native library's events
        // TheNativeLibrary.setEventHandler(event -> {
        //     if (eventListener != null) {
        //         eventListener.onEvent(event.type, event.data);
        //     }
        // });
    }

    /**
     * Remove the event listener
     */
    public static void removeEventListener() {
        eventListener = null;
    }

    // MARK: - Configuration

    /**
     * Set configuration options
     * Use simple types that marshal easily
     */
    public static void configure(
            String apiKey,
            String apiSecret,
            int timeout,
            boolean enableLogging) {
        
        // Apply configuration to native library
        Log.d(TAG, "Configured with apiKey: " + apiKey);
    }
}
EOF
```

### Kotlin Wrapper (Alternative)

Create `app/src/main/kotlin/com/example/mybinding/DotnetMyBinding.kt`:

```bash
mkdir -p app/src/main/kotlin/com/example/mybinding

cat > app/src/main/kotlin/com/example/mybinding/DotnetMyBinding.kt << 'EOF'
package com.example.mybinding

import android.content.Context
import android.util.Log
import android.view.View

/**
 * Main binding class exposed to .NET
 * Use @JvmStatic for static method access from C#
 */
object DotnetMyBinding {
    private const val TAG = "DotnetMyBinding"
    private var initialized = false

    // MARK: - Initialization

    @JvmStatic
    fun initialize(context: Context, debug: Boolean) {
        if (initialized) {
            Log.w(TAG, "Already initialized")
            return
        }
        
        // Initialize your native library here
        // TheNativeLibrary.configure(context, debug)
        
        initialized = true
        Log.d(TAG, "MyBinding initialized")
    }

    @JvmStatic
    fun isInitialized(): Boolean = initialized

    // MARK: - Synchronous Methods

    @JvmStatic
    fun getVersion(): String = "1.0.0"

    @JvmStatic
    fun processData(input: String): String {
        check(initialized) { "Must call initialize() first" }
        return "Processed: $input"
    }

    // MARK: - Asynchronous Methods

    /**
     * Callback interface - use Java-style interface for better interop
     */
    interface DataCallback {
        fun onSuccess(result: String)
        fun onError(error: String)
    }

    @JvmStatic
    fun fetchDataAsync(query: String, callback: DataCallback) {
        if (!initialized) {
            callback.onError("Not initialized")
            return
        }

        Thread {
            try {
                Thread.sleep(100)
                callback.onSuccess("Result for: $query")
            } catch (e: Exception) {
                callback.onError(e.message ?: "Unknown error")
            }
        }.start()
    }

    // MARK: - View Creation

    @JvmStatic
    fun createView(context: Context): View {
        return View(context).apply {
            setBackgroundColor(0xFF0066CC.toInt())
        }
    }

    // MARK: - Event Handling

    interface EventListener {
        fun onEvent(eventType: String, eventData: String)
    }

    private var eventListener: EventListener? = null

    @JvmStatic
    fun setEventListener(listener: EventListener?) {
        eventListener = listener
    }

    @JvmStatic
    fun removeEventListener() {
        eventListener = null
    }

    // MARK: - Configuration

    @JvmStatic
    fun configure(
        apiKey: String,
        apiSecret: String,
        timeout: Int,
        enableLogging: Boolean
    ) {
        Log.d(TAG, "Configured with apiKey: $apiKey")
    }
}
EOF
```

**Key Points for Kotlin:**
- Use `@JvmStatic` on all methods for static access from C#
- Use `object` for singleton pattern (maps to static methods)
- Use Java-style callback interfaces (not Kotlin lambdas)
- Avoid coroutines in the public API (use callbacks instead)

### Add Your Native Library Dependency

Update `app/build.gradle.kts` dependencies:

```kotlin
dependencies {
    // The native library you're wrapping
    implementation("com.thirdparty:awesomelib:1.0.0")
    
    // Common dependencies
    implementation("androidx.core:core-ktx:1.12.0")
}
```

## Step 5: Build the AAR

```bash
cd android/native

# Build release AAR
./gradlew :app:assembleRelease

# The AAR will be at:
# app/build/outputs/aar/app-release.aar
```

Verify the AAR contents:

```bash
unzip -l app/build/outputs/aar/app-release.aar
```

## Step 6: Create the C# Binding Project

### Create the Binding Project with dotnet new

```bash
cd ../..  # Back to MyBinding root
cd android

# Create the binding project using the template
dotnet new androidbinding -n ${BINDING_NAME}.Android.Binding

cd ${BINDING_NAME}.Android.Binding
```

This creates a properly structured binding project with:
- `Transforms/Metadata.xml` - For customizing the generated bindings
- `Transforms/EnumFields.xml` - For enum mappings (if needed)
- `Transforms/EnumMethods.xml` - For enum method mappings (if needed)

### Add the AAR Reference

Edit the generated `.csproj` to add your AAR and dependencies:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>
  </PropertyGroup>

  <!-- Your wrapper AAR -->
  <ItemGroup>
    <AndroidLibrary Include="../native/app/build/outputs/aar/app-release.aar" />
  </ItemGroup>

  <!-- Dependencies: NuGet packages for common libraries -->
  <ItemGroup>
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.4" />
    <PackageReference Include="Xamarin.Kotlin.StdLib" Version="1.9.22.1" />
    
    <!-- Add more based on your dependency analysis -->
    <!-- <PackageReference Include="Xamarin.AndroidX.AppCompat" Version="1.6.1.6" /> -->
  </ItemGroup>

  <!-- Dependencies without NuGet: Include but don't bind -->
  <ItemGroup>
    <!-- The library you're wrapping - include but don't generate C# bindings for it -->
    <!-- <AndroidMavenLibrary Include="com.thirdparty:awesomelib" Version="1.0.0" Bind="false" /> -->
  </ItemGroup>

  <!-- Compile-time only dependencies to ignore -->
  <ItemGroup>
    <AndroidIgnoredJavaDependency Include="org.jetbrains:annotations" Version="*" />
    <!-- <AndroidIgnoredJavaDependency Include="com.google.errorprone:error_prone_annotations" Version="*" /> -->
  </ItemGroup>
</Project>
```

### Update Metadata.xml

Edit the generated `Transforms/Metadata.xml` to customize the bindings:

```xml
<metadata>
  <!-- Rename the Java package to a .NET-friendly namespace -->
  <attr path="/api/package[@name='com.example.mybinding']" 
        name="managedName">MyBinding</attr>
  
  <!-- Remove obfuscated or internal classes if needed -->
  <!-- <remove-node path="/api/package[@name='com.example.mybinding']/class[@name='InternalHelper']" /> -->
  
  <!-- Rename parameters from p0, p1 to meaningful names -->
  <attr path="/api/package[@name='com.example.mybinding']/class[@name='DotnetMyBinding']/method[@name='initialize']/parameter[@name='p0']" 
        name="name">context</attr>
  <attr path="/api/package[@name='com.example.mybinding']/class[@name='DotnetMyBinding']/method[@name='initialize']/parameter[@name='p1']" 
        name="name">debug</attr>
        
  <attr path="/api/package[@name='com.example.mybinding']/class[@name='DotnetMyBinding']/method[@name='processData']/parameter[@name='p0']" 
        name="name">input</attr>
        
  <attr path="/api/package[@name='com.example.mybinding']/class[@name='DotnetMyBinding']/method[@name='fetchDataAsync']/parameter[@name='p0']" 
        name="name">query</attr>
  <attr path="/api/package[@name='com.example.mybinding']/class[@name='DotnetMyBinding']/method[@name='fetchDataAsync']/parameter[@name='p1']" 
        name="name">callback</attr>
</metadata>
```

## Step 7: Resolve Dependencies

This is the critical step. When you build, .NET 9+ will report dependency errors:

```bash
cd android/${BINDING_NAME}.Android.Binding
dotnet build
```

### Understanding Dependency Errors

| Error | Meaning | Solution |
|-------|---------|----------|
| **XA4241** | No known NuGet package exists | Use `AndroidMavenLibrary` with `Bind="false"` or `AndroidIgnoredJavaDependency` |
| **XA4242** | Microsoft maintains a NuGet package | Install the suggested NuGet package |

### Dependency Resolution Decision Tree

```
For each unsatisfied dependency:
│
├─ Is there a XA4242 suggestion?
│   └─ YES → Install the suggested NuGet package
│
├─ Is the dependency needed from C#?
│   ├─ YES → Find/create a binding NuGet or create your own binding
│   └─ NO → Use AndroidMavenLibrary with Bind="false"
│
├─ Is it a compile-time only dependency (annotations, processors)?
│   └─ YES → Use AndroidIgnoredJavaDependency
│
└─ Otherwise → Create a separate binding project or include AAR/JAR directly
```

### Finding NuGet Packages for Maven Dependencies

Microsoft-maintained packages include artifact tags. Search by tag:

```bash
# Search NuGet for a specific Maven artifact
dotnet package search "artifact=androidx.core:core" --source https://api.nuget.org/v3/index.json

# Or use curl
curl -s "https://azuresearch-usnc.nuget.org/query?q=tags:artifact=androidx.core:core&take=10" | jq '.data[] | {id: .id, version: .version}'
```

### Common Maven-to-NuGet Mappings

| Maven Artifact | NuGet Package |
|----------------|---------------|
| `androidx.annotation:annotation` | `Xamarin.AndroidX.Annotation` |
| `androidx.core:core` | `Xamarin.AndroidX.Core` |
| `androidx.core:core-ktx` | `Xamarin.AndroidX.Core.Core.Ktx` |
| `androidx.appcompat:appcompat` | `Xamarin.AndroidX.AppCompat` |
| `androidx.fragment:fragment` | `Xamarin.AndroidX.Fragment` |
| `androidx.activity:activity` | `Xamarin.AndroidX.Activity` |
| `androidx.recyclerview:recyclerview` | `Xamarin.AndroidX.RecyclerView` |
| `androidx.lifecycle:lifecycle-common` | `Xamarin.AndroidX.Lifecycle.Common` |
| `org.jetbrains.kotlin:kotlin-stdlib` | `Xamarin.Kotlin.StdLib` |
| `org.jetbrains.kotlinx:kotlinx-coroutines-core` | `Xamarin.KotlinX.Coroutines.Core` |
| `com.google.android.material:material` | `Xamarin.Google.Android.Material` |
| `com.squareup.okhttp3:okhttp` | `Square.OkHttp3` |
| `com.google.code.gson:gson` | `GoogleGson` |

### Practical Example: Complex Library Binding

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
  </PropertyGroup>

  <!-- Your wrapper AAR -->
  <ItemGroup>
    <AndroidLibrary Include="../native/app/build/outputs/aar/app-release.aar" />
  </ItemGroup>

  <!-- Dependencies satisfied by NuGet (preferred) -->
  <ItemGroup>
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.4" />
    <PackageReference Include="Xamarin.AndroidX.AppCompat" Version="1.6.1.6" />
    <PackageReference Include="Xamarin.Kotlin.StdLib" Version="1.9.22.1" />
    <PackageReference Include="Square.OkHttp3" Version="4.12.0.1" />
  </ItemGroup>

  <!-- The library being wrapped - include but don't bind -->
  <ItemGroup>
    <AndroidMavenLibrary 
      Include="com.thirdparty:awesomelib" 
      Version="1.0.0" 
      Bind="false" />
  </ItemGroup>

  <!-- Dependencies with no NuGet - include but don't bind -->
  <ItemGroup>
    <AndroidMavenLibrary 
      Include="com.thirdparty:internal-util" 
      Version="1.0.0" 
      Bind="false" />
  </ItemGroup>

  <!-- Compile-time only dependencies to ignore -->
  <ItemGroup>
    <AndroidIgnoredJavaDependency Include="org.jetbrains:annotations" Version="*" />
    <AndroidIgnoredJavaDependency Include="com.google.errorprone:error_prone_annotations" Version="*" />
    <AndroidIgnoredJavaDependency Include="com.google.code.findbugs:jsr305" Version="*" />
  </ItemGroup>
</Project>
```

## Step 8: Customize Bindings with Metadata

### Common Metadata Transforms

#### Rename Java Packages to .NET Namespaces

```xml
<metadata>
  <attr path="/api/package[@name='com.example.mybinding']" 
        name="managedName">MyBinding</attr>
  <attr path="/api/package[@name='com.example.mybinding.utils']" 
        name="managedName">MyBinding.Utils</attr>
</metadata>
```

#### Rename Classes and Methods

```xml
<metadata>
  <!-- Rename a class -->
  <attr path="/api/package[@name='com.example']/class[@name='Util']" 
        name="managedName">Utilities</attr>
  
  <!-- Rename a method -->
  <attr path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='getData']" 
        name="managedName">GetData</attr>
</metadata>
```

#### Rename Parameters

```xml
<metadata>
  <!-- Rename p0, p1 to meaningful names -->
  <attr path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='process']/parameter[@name='p0']" 
        name="name">input</attr>
  <attr path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='process']/parameter[@name='p1']" 
        name="name">options</attr>
</metadata>
```

#### Remove Internal or Obfuscated Types

```xml
<metadata>
  <!-- Remove internal packages -->
  <remove-node path="/api/package[starts-with(@name, 'com.example.internal')]" />
  
  <!-- Remove obfuscated packages -->
  <remove-node path="/api/package[@name='a']" />
  <remove-node path="/api/package[@name='b']" />
  
  <!-- Remove specific classes -->
  <remove-node path="/api/package[@name='com.example']/class[@name='InternalHelper']" />
  
  <!-- Remove classes with $ (often internal/anonymous) -->
  <remove-node path="/api/package/class[contains(@name, '$')]" />
</metadata>
```

#### Fix Visibility Issues

```xml
<metadata>
  <attr path="/api/package[@name='com.example']/class[@name='Helper']" 
        name="visibility">public</attr>
</metadata>
```

### Examining api.xml for XPath Construction

After building, examine the generated API definition:

```bash
cat obj/Debug/net9.0-android/api.xml
```

This helps construct the correct XPath for your transforms.

## Step 9: Build and Verify

```bash
dotnet build -c Release
```

Check for errors and resolve any remaining dependency issues.

## Step 10: Use in Your MAUI App

### Add Project Reference

In your MAUI app's `.csproj`:

```xml
<ItemGroup Condition="$(TargetFramework.Contains('android'))">
  <ProjectReference Include="..\android\MyBinding.Android.Binding\MyBinding.Android.Binding.csproj" />
</ItemGroup>
```

### Initialize in MauiProgram.cs

```csharp
using Microsoft.Maui.Hosting;

#if ANDROID
using MyBinding;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if ANDROID
        // Initialize in a platform-specific lifecycle hook
        builder.ConfigureLifecycleEvents(lifecycle =>
        {
            lifecycle.AddAndroid(android =>
            {
                android.OnCreate((activity, bundle) =>
                {
                    DotnetMyBinding.Initialize(activity, debug: true);
                });
            });
        });
#endif

        return builder.Build();
    }
}
```

### Implement Callback Interface

```csharp
#if ANDROID
using MyBinding;
using Android.Runtime;

// Implement the callback interface
public class MyDataCallback : Java.Lang.Object, DotnetMyBinding.IDataCallback
{
    private readonly Action<string> _onSuccess;
    private readonly Action<string> _onError;

    public MyDataCallback(Action<string> onSuccess, Action<string> onError)
    {
        _onSuccess = onSuccess;
        _onError = onError;
    }

    public void OnSuccess(string result) => _onSuccess(result);
    public void OnError(string error) => _onError(error);
}
#endif
```

### Use in Your Page

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnSyncButtonClicked(object sender, EventArgs e)
    {
#if ANDROID
        // Synchronous method
        var version = DotnetMyBinding.GetVersion();
        var result = DotnetMyBinding.ProcessData("test input");
        DisplayAlert("Result", $"Version: {version}\nResult: {result}", "OK");
#endif
    }

    private void OnAsyncButtonClicked(object sender, EventArgs e)
    {
#if ANDROID
        // Async with callback
        DotnetMyBinding.FetchDataAsync("my query", new MyDataCallback(
            onSuccess: result => MainThread.BeginInvokeOnMainThread(() => 
                DisplayAlert("Success", result, "OK")),
            onError: error => MainThread.BeginInvokeOnMainThread(() => 
                DisplayAlert("Error", error, "OK"))
        ));
#endif
    }

    private void OnCreateViewClicked(object sender, EventArgs e)
    {
#if ANDROID
        var context = Platform.CurrentActivity;
        var nativeView = DotnetMyBinding.CreateView(context);
        
        // Use with a custom handler or Android.Views.View host
#endif
    }
}
```

### Register Event Listener

```csharp
#if ANDROID
public class MyEventListener : Java.Lang.Object, DotnetMyBinding.IEventListener
{
    private readonly Action<string, string> _onEvent;

    public MyEventListener(Action<string, string> onEvent)
    {
        _onEvent = onEvent;
    }

    public void OnEvent(string eventType, string eventData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _onEvent(eventType, eventData);
        });
    }
}

// In your page
protected override void OnAppearing()
{
    base.OnAppearing();
    
#if ANDROID
    DotnetMyBinding.SetEventListener(new MyEventListener((type, data) =>
    {
        StatusLabel.Text = $"Event: {type} - {data}";
    }));
#endif
}

protected override void OnDisappearing()
{
    base.OnDisappearing();
    
#if ANDROID
    DotnetMyBinding.RemoveEventListener();
#endif
}
#endif
```

# Updating Bindings When Native SDK Changes

## Step-by-step Update Process

### 1. Update Native Dependency Version

Edit `app/build.gradle.kts`:

```kotlin
dependencies {
    implementation("com.thirdparty:awesomelib:2.0.0")  // Updated version
}
```

### 2. Re-analyze Dependencies

```bash
cd android/native
./gradlew app:dependencies --configuration releaseRuntimeClasspath
```

Compare with your previous dependency mapping and identify:
- New dependencies added
- Dependencies with version changes
- Dependencies removed

### 3. Update the Wrapper (If Needed)

Review release notes for the native library and update `DotnetMyBinding.java`:
- Add new methods for new APIs
- Update method signatures for changed APIs
- Remove deprecated API wrappers
- Handle any breaking changes

### 4. Rebuild the AAR

```bash
./gradlew :app:assembleRelease
```

### 5. Update Binding Project Dependencies

Update NuGet packages and Maven dependencies in the binding `.csproj` based on your new dependency analysis.

### 6. Rebuild and Fix Errors

```bash
cd ../MyBinding.Android.Binding
dotnet build
```

Address any new XA4241/XA4242 errors.

### 7. Update Metadata.xml (If Needed)

If new classes/methods were added to your wrapper, update parameter names and namespace mappings.

### 8. Test

Build and run the sample app to verify functionality.

# Handling Native Libraries (.so files)

Some Android libraries include native code (`.so` files). If you get `java.lang.UnsatisfiedLinkError`, ensure native libraries are properly included.

### Including .so Files

For AAR files, `.so` files are usually included automatically. For JAR files, add them manually:

```xml
<ItemGroup>
  <AndroidNativeLibrary Include="libs\arm64-v8a\libmynative.so">
    <Abi>arm64-v8a</Abi>
  </AndroidNativeLibrary>
  <AndroidNativeLibrary Include="libs\armeabi-v7a\libmynative.so">
    <Abi>armeabi-v7a</Abi>
  </AndroidNativeLibrary>
  <AndroidNativeLibrary Include="libs\x86_64\libmynative.so">
    <Abi>x86_64</Abi>
  </AndroidNativeLibrary>
</ItemGroup>
```

### Manually Loading Native Libraries

Sometimes you need to explicitly load the native library:

```csharp
// In your Android initialization code
Java.Lang.JavaSystem.LoadLibrary("mynative");
```

# Troubleshooting

## Build Errors

### "Java dependency 'X' is not satisfied" (XA4241/XA4242)

Missing transitive dependency.

**Solution:** Follow the dependency resolution decision tree:
1. If XA4242 suggests a NuGet package → Install it
2. If you don't need the API in C# → Use `AndroidMavenLibrary` with `Bind="false"`
3. If it's compile-time only → Use `AndroidIgnoredJavaDependency`

### "Type is defined multiple times"

Duplicate classes from including the same dependency twice.

**Solution:** Check for overlap between NuGet packages and AAR contents. Use `Bind="false"` or remove redundant references.

### "Class 'X' does not implement interface member 'Y'"

Covariant return types or interface implementation issues.

**Solution:** Add custom implementation in `Additions/` folder:

```csharp
// Additions/MyClassAdditions.cs
namespace Com.Example
{
    public partial class MyClass
    {
        Java.Lang.Object SomeInterface.SomeMethod()
        {
            return RawSomeMethod();
        }
    }
}
```

### "Cannot find symbol" / "Package does not exist"

Wrapper code references classes not available.

**Solution:** Verify all dependencies are declared in `build.gradle.kts` and rebuild the AAR.

### Gradle Build Failures

```bash
# Clean and rebuild
./gradlew clean
./gradlew :app:assembleRelease --stacktrace
```

## Runtime Errors

### `Java.Lang.NoClassDefFoundError`

Missing dependency at runtime.

**Solution:** A required dependency was ignored. Remove it from `AndroidIgnoredJavaDependency` and properly satisfy it.

### `Java.Lang.UnsatisfiedLinkError`

Native library (.so) not loaded.

**Solution:** 
1. Ensure .so files are included in the AAR
2. Call `JavaSystem.LoadLibrary()` if needed
3. Check ABI compatibility (arm64-v8a, armeabi-v7a, x86_64)

### `Java.Lang.IllegalStateException: Must call initialize() first`

Library not initialized.

**Solution:** Ensure you call `DotnetMyBinding.Initialize()` before using other methods.

### Callback Not Invoked

**Causes & Solutions:**
1. **Callback on wrong thread** - Use `MainThread.BeginInvokeOnMainThread()` for UI updates
2. **Callback garbage collected** - Store a strong reference to the callback object
3. **Exception in native code** - Add try/catch in wrapper and check logcat

## IntelliSense Issues

**IntelliSense shows errors but project compiles:**
This is normal for binding projects. The binding generator doesn't use source generators. Build the project first and IntelliSense will catch up (though some errors may persist).

# Quick Reference

## Wrapper Design Guidelines

### Type Mapping

Keep method signatures simple with types that marshal easily:

| Java/Kotlin Type | C# Type | Notes |
|------------------|---------|-------|
| `String` | `string` | Direct mapping |
| `boolean` / `Boolean` | `bool` | Direct mapping |
| `int` / `Integer` | `int` | Direct mapping |
| `long` / `Long` | `long` | Direct mapping |
| `double` / `Double` | `double` | Direct mapping |
| `float` / `Float` | `float` | Direct mapping |
| `byte[]` | `byte[]` | Direct mapping |
| `Context` | `Android.Content.Context` | Pass from platform |
| `View` | `Android.Views.View` | For view creation |
| `JSONObject` | `Org.Json.JSONObject` | For complex data |
| Callback interface | `IJavaObject` implementation | See callback pattern |

### Callback Interface Pattern

```java
// Java - define a simple callback interface
public interface DataCallback {
    void onSuccess(String result);
    void onError(String error);
}
```

```csharp
// C# - implement the generated interface
public class MyCallback : Java.Lang.Object, DotnetMyBinding.IDataCallback
{
    private readonly Action<string> _onSuccess;
    private readonly Action<string> _onError;

    public MyCallback(Action<string> onSuccess, Action<string> onError)
    {
        _onSuccess = onSuccess;
        _onError = onError;
    }

    public void OnSuccess(string result) => _onSuccess(result);
    public void OnError(string error) => _onError(error);
}
```

### Things to Avoid in Wrapper API

| Avoid | Use Instead |
|-------|-------------|
| Kotlin coroutines | Callback interfaces |
| Kotlin lambdas | Java-style interfaces |
| Kotlin data classes | Simple classes with getters |
| Kotlin sealed classes | Enums or class hierarchy |
| Generics (complex) | Specific types or Object |
| Rx/Flow observables | Callback interfaces |

## MSBuild Items Reference

| Item | Purpose |
|------|---------|
| `<AndroidLibrary>` | Include AAR/JAR in binding |
| `<AndroidMavenLibrary>` | Download and include from Maven |
| `<AndroidIgnoredJavaDependency>` | Ignore a dependency (compile-time only) |
| `<AndroidNativeLibrary>` | Include .so files |
| `<TransformFile>` | Metadata transform files |
| `<PackageReference>` | NuGet package reference |

## Common Attributes

| Attribute | Values | Purpose |
|-----------|--------|---------|
| `Bind` | `true`/`false` | Whether to generate C# bindings |
| `Pack` | `true`/`false` | Whether to include in NuGet package |
| `JavaArtifact` | `groupId:artifactId` | Declare what Maven artifact this satisfies |
| `JavaVersion` | `version` | Version of the Maven artifact |
| `Repository` | `Central`/`Google`/URL | Maven repository to download from |

## Complete Script: Create Android Binding Project

```bash
#!/bin/bash
set -e

# Configuration
BINDING_NAME="${1:-MyBinding}"
PACKAGE_NAME="${2:-com.example.mybinding}"
MIN_SDK="${3:-21}"
GRADLE_VERSION="${4:-9.2.1}"
AGP_VERSION="${5:-8.2.2}"

echo "Creating Android binding project: ${BINDING_NAME}"
echo "  Package: ${PACKAGE_NAME}"
echo "  Min SDK: ${MIN_SDK}"
echo "  Gradle: ${GRADLE_VERSION}"

# Create directory structure
mkdir -p ${BINDING_NAME}/android/native/app/src/main/java/${PACKAGE_NAME//./\/}
mkdir -p ${BINDING_NAME}/android/${BINDING_NAME}.Android.Binding/Transforms
mkdir -p ${BINDING_NAME}/sample/MauiSample

cd ${BINDING_NAME}/android/native

# Download and setup Gradle Wrapper (no global Gradle install needed)
echo "Setting up Gradle Wrapper..."
GRADLE_DIST_URL="https://services.gradle.org/distributions/gradle-${GRADLE_VERSION}-bin.zip"
WRAPPER_JAR_URL="https://raw.githubusercontent.com/gradle/gradle/v${GRADLE_VERSION}/gradle/wrapper/gradle-wrapper.jar"

mkdir -p gradle/wrapper
cat > gradle/wrapper/gradle-wrapper.properties << EOF
distributionBase=GRADLE_USER_HOME
distributionPath=wrapper/dists
distributionUrl=https\://services.gradle.org/distributions/gradle-${GRADLE_VERSION}-bin.zip
networkTimeout=10000
validateDistributionUrl=true
zipStoreBase=GRADLE_USER_HOME
zipStorePath=wrapper/dists
EOF

# Create gradlew script
cat > gradlew << 'GRADLEW_EOF'
#!/bin/sh
# Gradle Wrapper script - downloads Gradle if needed
exec java -jar "$(dirname "$0")/gradle/wrapper/gradle-wrapper.jar" "$@"
GRADLEW_EOF
chmod +x gradlew

# Download gradle-wrapper.jar
curl -sL -o gradle/wrapper/gradle-wrapper.jar \
  "https://raw.githubusercontent.com/gradle/gradle/v${GRADLE_VERSION}/gradle/wrapper/gradle-wrapper.jar" 2>/dev/null || \
  echo "Note: Could not download gradle-wrapper.jar. Run './gradlew' to auto-download."

# Create settings.gradle.kts
cat > settings.gradle.kts << EOF
pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "${BINDING_NAME}Native"
include(":app")
EOF

# Create root build.gradle.kts
cat > build.gradle.kts << EOF
plugins {
    id("com.android.library") version "${AGP_VERSION}" apply false
}
EOF

# Create app build.gradle.kts
cat > app/build.gradle.kts << EOF
plugins {
    id("com.android.library")
}

android {
    namespace = "${PACKAGE_NAME}"
    compileSdk = 34

    defaultConfig {
        minSdk = ${MIN_SDK}
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }
    
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
}

dependencies {
    // Add your native library dependency here
    // implementation("com.thirdparty:awesomelib:1.0.0")
    
    implementation("androidx.core:core:1.12.0")
}
EOF

# Create Java wrapper
cat > app/src/main/java/${PACKAGE_NAME//./\/}/Dotnet${BINDING_NAME}.java << EOF
package ${PACKAGE_NAME};

import android.content.Context;
import android.util.Log;

public class Dotnet${BINDING_NAME} {
    private static final String TAG = "Dotnet${BINDING_NAME}";
    private static boolean initialized = false;

    public static void initialize(Context context, boolean debug) {
        if (initialized) return;
        initialized = true;
        Log.d(TAG, "${BINDING_NAME} initialized");
    }

    public static boolean isInitialized() {
        return initialized;
    }

    public static String getVersion() {
        return "1.0.0";
    }
    
    public interface DataCallback {
        void onSuccess(String result);
        void onError(String error);
    }
    
    public static void fetchDataAsync(String query, DataCallback callback) {
        if (!initialized) {
            callback.onError("Not initialized");
            return;
        }
        callback.onSuccess("Result for: " + query);
    }
}
EOF

cd ../..

# Create binding project using dotnet new template
dotnet new androidbinding -n ${BINDING_NAME}.Android.Binding -o ${BINDING_NAME}.Android.Binding

# Add the AAR reference to the generated csproj
# The template creates a basic project, we need to add our specific items
cat >> ${BINDING_NAME}.Android.Binding/${BINDING_NAME}.Android.Binding.csproj << EOF

  <!-- Your wrapper AAR -->
  <ItemGroup>
    <AndroidLibrary Include="../native/app/build/outputs/aar/app-release.aar" />
  </ItemGroup>

  <!-- Dependencies -->
  <ItemGroup>
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.4" />
  </ItemGroup>

  <ItemGroup>
    <AndroidIgnoredJavaDependency Include="org.jetbrains:annotations" Version="*" />
  </ItemGroup>
EOF

# Fix the csproj by moving the added items inside the Project element
# (The template ends with </Project>, so we need to insert before it)
sed -i '' 's/<\\/Project>//' ${BINDING_NAME}.Android.Binding/${BINDING_NAME}.Android.Binding.csproj
echo "</Project>" >> ${BINDING_NAME}.Android.Binding/${BINDING_NAME}.Android.Binding.csproj

# Update Metadata.xml with our namespace mapping
cat > ${BINDING_NAME}.Android.Binding/Transforms/Metadata.xml << EOF
<metadata>
  <attr path="/api/package[@name='${PACKAGE_NAME}']" 
        name="managedName">${BINDING_NAME}</attr>
</metadata>
EOF

cd ..

echo ""
echo "✅ Created ${BINDING_NAME} Android binding project!"
echo ""
echo "Next steps:"
echo "  1. cd ${BINDING_NAME}/android/native"
echo "  2. Add your native library to app/build.gradle.kts"
echo "  3. ./gradlew :app:assembleRelease"
echo "  4. cd ../${BINDING_NAME}.Android.Binding"
echo "  5. dotnet build"
echo "  6. Resolve any dependency errors"
```

Save as `create-android-binding.sh` and run:

```bash
chmod +x create-android-binding.sh

# Basic usage (uses defaults)
./create-android-binding.sh MyAwesomeBinding

# Full customization
./create-android-binding.sh MyAwesomeBinding com.mycompany.mybinding 21 9.2.1 8.2.2
#                          ^name             ^package                ^sdk ^gradle ^agp
```

> **Tip:** The script sets up Gradle Wrapper, so you don't need Gradle installed globally. The first `./gradlew` run will download the correct Gradle version automatically.

# Resources

## Official Documentation
- [Binding a Java Library (Microsoft Docs)](https://learn.microsoft.com/dotnet/android/binding-libs/binding-java-libs/binding-java-library)
- [Binding from Maven (Microsoft Docs)](https://learn.microsoft.com/dotnet/android/binding-libs/binding-java-libs/binding-java-maven-library)
- [Java Dependency Verification](https://learn.microsoft.com/dotnet/android/binding-libs/advanced-concepts/java-dependency-verification)
- [Resolving Java Dependencies](https://learn.microsoft.com/dotnet/android/binding-libs/advanced-concepts/resolving-java-dependencies)
- [Java Bindings Metadata](https://learn.microsoft.com/dotnet/android/binding-libs/customizing-bindings/java-bindings-metadata)
- [Native Library Interop - .NET Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/native-library-interop)

## Version Discovery
- [Android Gradle Plugin Releases](https://developer.android.com/build/releases/gradle-plugin) - Check for latest AGP version
- [Gradle Releases](https://gradle.org/releases/) - Check for latest Gradle version
- [Kotlin Releases](https://kotlinlang.org/docs/releases.html) - Check for latest Kotlin version

## Templates and Examples
- [Maui.NativeLibraryInterop Repository](https://github.com/CommunityToolkit/Maui.NativeLibraryInterop)
- [Xamarin.AndroidX Repository](https://github.com/xamarin/AndroidX)

## Related Skills
- See [docs/android-bindings-guide.md](../../../docs/android-bindings-guide.md) for comprehensive reference
- See [ios-slim-bindings](../ios-slim-bindings/SKILL.md) for iOS bindings

# Output Format

When assisting with Android slim bindings, provide:

1. **Dependency analysis** - Full dependency tree and NuGet mapping strategy
2. **Project structure** - File/folder layout for the binding
3. **Wrapper code** - Complete Java or Kotlin implementation
4. **Gradle configuration** - build.gradle.kts with dependencies
5. **C# binding project** - `.csproj` with proper dependency references
6. **Metadata transforms** - Metadata.xml for namespace and parameter renaming
7. **Usage examples** - How to call the binding from MAUI/C#
8. **Troubleshooting guidance** - Common issues and solutions for the specific library

Always verify:
- All transitive dependencies are accounted for
- NuGet packages match required Maven versions
- Wrapper methods use simple, marshallable types
- Callback interfaces are properly designed
- Metadata.xml renames parameters to meaningful names
