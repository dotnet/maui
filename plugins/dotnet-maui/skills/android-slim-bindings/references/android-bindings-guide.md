# Creating Bindings for .NET Android Projects

This guide covers the process of creating C# bindings to interface with native Android libraries (AAR/JAR files) in .NET MAUI, and .NET for Android projects. Special emphasis is placed on resolving the complex dependency graphs that are common with Android libraries.

## Overview

When you need to use a third-party Android library not written in C#, you must create a **binding** that wraps the Java/Kotlin code with C# Managed Callable Wrappers (MCW). There are two primary approaches:

1. **Traditional Full Bindings** - Bind the entire native library's API surface automatically
2. **Native Library Interop (Slim Bindings)** - Create a thin Java/Kotlin wrapper exposing only the APIs you need

---

## Understanding Android Library Dependencies

Before diving into binding approaches, it's critical to understand how Android dependencies work, as this is the most challenging aspect of Android bindings.

### The Dependency Challenge

Android libraries commonly depend on:
- **AndroidX libraries** - Google's modern support libraries
- **Kotlin Standard Library** - Required for any Kotlin-based library
- **Google Play Services** - For Firebase, Maps, Auth, etc.
- **Third-party libraries** - OkHttp, Retrofit, Gson, etc.

These dependencies form a **transitive dependency graph** that must be satisfied, or your app will crash at runtime with `Java.Lang.NoClassDefFoundError`.

### How Dependencies Are Declared

Android libraries declare dependencies in their **POM file** (Project Object Model), which lists:
- Required dependencies with minimum versions
- Optional dependencies
- Compile-time only dependencies (annotations, etc.)

### Analyzing the Full Dependency Graph

Before creating bindings, you need to understand the complete transitive dependency tree of your target library. There are several ways to obtain this information.

#### Method 1: Using Gradle Dependencies Task

The most comprehensive way to see the full dependency tree is to create a minimal Gradle project and use the `dependencies` task.

**Step 1: Create a minimal `build.gradle.kts`:**

```kotlin
plugins {
    id("java-library")
}

repositories {
    google()
    mavenCentral()
    // Add other repositories as needed
    maven { url = uri("https://jitpack.io") }
}

dependencies {
    // Add the library you want to analyze
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
}
```

**Step 2: Run the dependencies task:**

```bash
gradle dependencies --configuration runtimeClasspath
```

This produces a tree showing all transitive dependencies:

```
runtimeClasspath
\--- com.squareup.okhttp3:okhttp:4.12.0
     +--- com.squareup.okio:okio:3.6.0
     |    \--- com.squareup.okio:okio-jvm:3.6.0
     |         +--- org.jetbrains.kotlin:kotlin-stdlib-jdk8:1.9.10 -> 1.9.21
     |         |    +--- org.jetbrains.kotlin:kotlin-stdlib:1.9.21
     |         |    |    \--- org.jetbrains:annotations:13.0
     |         |    \--- org.jetbrains.kotlin:kotlin-stdlib-jdk7:1.9.21
     |         |         \--- org.jetbrains.kotlin:kotlin-stdlib:1.9.21 (*)
     |         \--- org.jetbrains.kotlin:kotlin-stdlib-common:1.9.21 -> 1.9.21
     \--- org.jetbrains.kotlin:kotlin-stdlib-jdk8:1.9.10 -> 1.9.21 (*)
```

**Understanding the output:**
- `->` indicates version conflict resolution (e.g., `1.9.10 -> 1.9.21` means 1.9.10 was requested but 1.9.21 was resolved)
- `(*)` indicates the dependency was already shown elsewhere in the tree (deduplication)

**Step 3: Get a flat list of resolved dependencies:**

```bash
gradle dependencies --configuration runtimeClasspath | grep -E "^[+\\\\]" | sed 's/.*--- //' | sort -u
```

Or use the `dependencyInsight` task for a specific dependency:

```bash
gradle dependencyInsight --dependency kotlin-stdlib --configuration runtimeClasspath
```

#### Method 2: Using Maven Dependency Plugin

If you prefer Maven, create a minimal `pom.xml`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 
         http://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>
    
    <groupId>com.example</groupId>
    <artifactId>dependency-analyzer</artifactId>
    <version>1.0.0</version>
    
    <repositories>
        <repository>
            <id>google</id>
            <url>https://maven.google.com</url>
        </repository>
    </repositories>
    
    <dependencies>
        <dependency>
            <groupId>com.squareup.okhttp3</groupId>
            <artifactId>okhttp</artifactId>
            <version>4.12.0</version>
        </dependency>
    </dependencies>
</project>
```

**Get the dependency tree:**

```bash
mvn dependency:tree
```

**Get resolved versions only (flat list):**

```bash
mvn dependency:list
```

**Resolve and display effective versions:**

```bash
mvn dependency:resolve
```

#### Method 3: Inspecting POM Files Directly

For quick analysis, you can read POM files directly from Maven Central or Google's Maven repository:

```bash
# Maven Central
curl -s "https://repo1.maven.org/maven2/com/squareup/okhttp3/okhttp/4.12.0/okhttp-4.12.0.pom"

# Google Maven (AndroidX)
curl -s "https://maven.google.com/androidx/core/core/1.12.0/core-1.12.0.pom"
```

#### Understanding Version Constraints

When analyzing dependencies, pay attention to version constraints in POM files:

| Constraint | Meaning |
|------------|---------|
| `1.0.0` | Recommended version (soft requirement) |
| `[1.0.0]` | Exact version required |
| `[1.0.0,)` | Version 1.0.0 or higher |
| `[1.0.0,2.0.0)` | Between 1.0.0 (inclusive) and 2.0.0 (exclusive) |
| `(,1.0.0]` | Any version up to and including 1.0.0 |

Gradle resolves conflicts by selecting the **highest** version requested across the dependency graph by default.

### Finding NuGet Packages for Maven Dependencies

Once you have your dependency list, you need to find corresponding NuGet packages. Here's how to search effectively.

#### Searching NuGet.org

**Using the NuGet CLI:**

```bash
# Search for packages related to AndroidX Core
nuget search "Xamarin.AndroidX.Core"

# Search using dotnet CLI
dotnet package search "Xamarin.AndroidX"
```

**Using the NuGet API directly:**

```bash
# Search for packages containing "androidx.core"
curl -s "https://azuresearch-usnc.nuget.org/query?q=androidx.core&take=20" | jq '.data[] | {id: .id, version: .version}'
```

#### Microsoft-Maintained Package Tags

Microsoft maintains NuGet bindings for many common Android libraries (AndroidX, Kotlin, Google Play Services, Firebase). These packages include special tags that help identify which Maven artifact they satisfy:

| Tag | Description | Example |
|-----|-------------|---------|
| `artifact` | Maven group and artifact ID | `artifact=androidx.annotation:annotation` |
| `artifact_versioned` | Maven coordinate with version | `artifact_versioned=androidx.annotation:annotation:1.9.1` |

**Searching by artifact tag:**

You can search NuGet for packages that satisfy a specific Maven dependency:

```bash
# Search for packages that bind androidx.core:core
curl -s "https://azuresearch-usnc.nuget.org/query?q=tags:artifact=androidx.core:core&take=10" | jq '.data[] | {id: .id, version: .version}'
```

**Example: Finding the NuGet package for `androidx.annotation:annotation:1.9.1`:**

1. Search NuGet for the artifact tag:
   ```bash
   dotnet package search "artifact=androidx.annotation:annotation" --source https://api.nuget.org/v3/index.json
   ```

2. Or visit NuGet.org and search: `tags:"artifact=androidx.annotation:annotation"`

3. The result will show `Xamarin.AndroidX.Annotation` which includes:
   - Tag: `artifact=androidx.annotation:annotation`
   - Tag: `artifact_versioned=androidx.annotation:annotation:1.9.1`

**Verifying package contents:**

Once you find a candidate package, verify it advertises the correct artifact:

```bash
# Download and inspect the nuspec
nuget install Xamarin.AndroidX.Annotation -Version 1.9.1 -OutputDirectory ./packages
cat ./packages/Xamarin.AndroidX.Annotation.1.9.1/Xamarin.AndroidX.Annotation.nuspec | grep -A2 "<tags>"
```

#### Common Maven-to-NuGet Mappings

Here's a reference for commonly needed packages:

| Maven Artifact | NuGet Package | Tag Search |
|----------------|---------------|------------|
| `androidx.annotation:annotation` | `Xamarin.AndroidX.Annotation` | `artifact=androidx.annotation:annotation` |
| `androidx.core:core` | `Xamarin.AndroidX.Core` | `artifact=androidx.core:core` |
| `androidx.core:core-ktx` | `Xamarin.AndroidX.Core.Core.Ktx` | `artifact=androidx.core:core-ktx` |
| `androidx.appcompat:appcompat` | `Xamarin.AndroidX.AppCompat` | `artifact=androidx.appcompat:appcompat` |
| `androidx.fragment:fragment` | `Xamarin.AndroidX.Fragment` | `artifact=androidx.fragment:fragment` |
| `androidx.activity:activity` | `Xamarin.AndroidX.Activity` | `artifact=androidx.activity:activity` |
| `androidx.lifecycle:lifecycle-common` | `Xamarin.AndroidX.Lifecycle.Common` | `artifact=androidx.lifecycle:lifecycle-common` |
| `org.jetbrains.kotlin:kotlin-stdlib` | `Xamarin.Kotlin.StdLib` | `artifact=org.jetbrains.kotlin:kotlin-stdlib` |
| `org.jetbrains.kotlinx:kotlinx-coroutines-core` | `Xamarin.KotlinX.Coroutines.Core` | `artifact=org.jetbrains.kotlinx:kotlinx-coroutines-core` |
| `com.google.android.material:material` | `Xamarin.Google.Android.Material` | `artifact=com.google.android.material:material` |

#### Handling Version Mismatches

When the NuGet package version doesn't exactly match the Maven version you need:

1. **Check if a higher version works:** NuGet packages often bind newer versions that are backward compatible.

2. **Explicitly declare the artifact version:** If the package doesn't advertise the artifact or version is mismatched:
   ```xml
   <PackageReference 
     Include="Xamarin.AndroidX.Core" 
     Version="1.12.0.3"
     JavaArtifact="androidx.core:core"
     JavaVersion="1.12.0" />
   ```

3. **Use `@(AndroidIgnoredJavaDependency)` for minor version differences** when you've verified compatibility:
   ```xml
   <AndroidIgnoredJavaDependency Include="androidx.core:core:1.13.0" />
   ```

#### When No NuGet Package Exists

If you can't find a NuGet package for a dependency:

1. **Check if it's a transitive-only dependency** - use `Bind="false"`:
   ```xml
   <AndroidMavenLibrary Include="com.example:internal-lib" Version="1.0.0" Bind="false" />
   ```

2. **Check if it's compile-time only** (annotations, processors) - ignore it:
   ```xml
   <AndroidIgnoredJavaDependency Include="com.google.errorprone:error_prone_annotations:2.15.0" />
   ```

3. **Create your own binding** - see the binding project creation steps below.

---

## Traditional Full Bindings

This approach binds an entire AAR/JAR library, automatically generating C# wrappers for all public APIs.

### Prerequisites

- .NET 9 SDK (recommended for Java Dependency Verification)
- JDK 17+
- Android SDK
- Visual Studio 2022+ or VS Code with .NET MAUI extension

### Step 1: Create the Binding Project

```bash
dotnet new androidbinding -n "MyLibrary.Android.Binding"
```

Or in Visual Studio: **File → New Project → Android Java Binding Library**

### Step 2: Add the Native Library

Place your `.aar` or `.jar` file in the project. In .NET 9+, files are automatically detected with appropriate build actions.

#### Project File Structure

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- AAR/JAR files in project folder are auto-detected -->
  <!-- Use Update to modify auto-detected files -->
  <ItemGroup>
    <AndroidLibrary Update="mylibrary.aar" />
  </ItemGroup>
</Project>
```

### Step 3: Binding from Maven (Recommended for .NET 9+)

Instead of manually downloading libraries, use `<AndroidMavenLibrary>`:

```xml
<ItemGroup>
  <!-- Include format is {GroupId}:{ArtifactId} -->
  <AndroidMavenLibrary Include="com.squareup.okhttp3:okhttp" Version="4.12.0" />
</ItemGroup>
```

This automatically:
- Downloads the AAR/JAR from Maven Central
- Downloads the POM file for dependency verification
- Caches the files locally

#### Using Different Maven Repositories

```xml
<!-- Google's Maven Repository -->
<AndroidMavenLibrary 
  Include="androidx.core:core" 
  Version="1.12.0" 
  Repository="Google" />

<!-- Custom Maven Repository -->
<AndroidMavenLibrary 
  Include="com.example:library" 
  Version="1.0.0" 
  Repository="https://maven.example.com/releases" />
```

### Step 4: Manual Library Reference

For libraries not in Maven, add them manually:

```xml
<ItemGroup>
  <!-- Bind and include the library -->
  <AndroidLibrary Include="mylibrary.aar" />
  
  <!-- With POM for dependency verification (.NET 9+) -->
  <AndroidLibrary 
    Include="mylibrary.aar" 
    Manifest="mylibrary.pom"
    JavaArtifact="com.example:mylibrary"
    JavaVersion="1.0.0" />
</ItemGroup>
```

---

## Resolving Dependencies: The Critical Step

This is where most Android binding projects fail. .NET 9+ includes Java Dependency Verification that will surface errors like:

```
error XA4241: Java dependency 'androidx.core:core:1.9.0' is not satisfied.
error XA4242: Java dependency 'org.jetbrains.kotlin:kotlin-stdlib:1.9.0' is not satisfied. 
              Microsoft maintains the NuGet package 'Xamarin.Kotlin.StdLib' that could fulfill this dependency.
```

### Understanding XA4241 vs XA4242

| Error | Meaning |
|-------|---------|
| **XA4241** | No known NuGet package exists for this dependency |
| **XA4242** | Microsoft maintains a NuGet package that could satisfy this dependency |

### Dependency Resolution Strategies

You have four options to satisfy each dependency, and choosing the right one is crucial:

#### Option 1: NuGet Packages (Preferred)

Use existing NuGet bindings maintained by Microsoft or the community.

```xml
<ItemGroup>
  <!-- AndroidX libraries -->
  <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.3" />
  <PackageReference Include="Xamarin.AndroidX.AppCompat" Version="1.6.1.6" />
  <PackageReference Include="Xamarin.AndroidX.RecyclerView" Version="1.3.2.2" />
  
  <!-- Kotlin -->
  <PackageReference Include="Xamarin.Kotlin.StdLib" Version="1.9.22" />
  
  <!-- Google Play Services -->
  <PackageReference Include="Xamarin.GooglePlayServices.Base" Version="118.2.0.3" />
</ItemGroup>
```

**Finding the Right NuGet Package:**

| Maven Artifact | NuGet Package |
|----------------|---------------|
| `androidx.core:core` | `Xamarin.AndroidX.Core` |
| `androidx.appcompat:appcompat` | `Xamarin.AndroidX.AppCompat` |
| `androidx.recyclerview:recyclerview` | `Xamarin.AndroidX.RecyclerView` |
| `androidx.fragment:fragment` | `Xamarin.AndroidX.Fragment` |
| `org.jetbrains.kotlin:kotlin-stdlib` | `Xamarin.Kotlin.StdLib` |
| `com.google.android.gms:play-services-*` | `Xamarin.GooglePlayServices.*` |
| `com.google.firebase:firebase-*` | `Xamarin.Firebase.*` |

**When a NuGet Package Doesn't Advertise Its Artifact:**

Some packages don't include the `artifact=` tag. Explicitly declare what they provide:

```xml
<PackageReference 
  Include="Xamarin.Kotlin.StdLib" 
  Version="1.9.22"
  JavaArtifact="org.jetbrains.kotlin:kotlin-stdlib:1.9.22" />
```

#### Option 2: AndroidMavenLibrary (Without Binding)

Include the Java dependency but don't create C# bindings for it:

```xml
<ItemGroup>
  <!-- Include but don't bind -->
  <AndroidMavenLibrary 
    Include="com.example:dependency-lib" 
    Version="1.0.0" 
    Bind="false" />
</ItemGroup>
```

**When to use this:**
- The dependency is only used internally by your main library
- You don't need to call the dependency's APIs from C#
- No NuGet package exists for the dependency

#### Option 3: Direct AAR/JAR Reference (Without Binding)

For dependencies you have locally:

```xml
<ItemGroup>
  <!-- Include dependency, don't bind it -->
  <AndroidLibrary 
    Include="dependency.aar" 
    JavaArtifact="com.example:dependency:1.0.0"
    Bind="false" />
</ItemGroup>
```

#### Option 4: Ignore Dependency (Last Resort)

For compile-time only dependencies (annotations, etc.):

```xml
<ItemGroup>
  <!-- Ignore this dependency - use with caution! -->
  <AndroidIgnoredJavaDependency 
    Include="com.google.errorprone:error_prone_annotations:2.15.0" />
    
  <!-- Can ignore multiple versions -->
  <AndroidIgnoredJavaDependency 
    Include="org.jetbrains:annotations:13.0;org.jetbrains:annotations:23.0.0" />
</ItemGroup>
```

**Warning:** If the dependency is actually needed at runtime, your app will crash with `Java.Lang.NoClassDefFoundError`. Only use this for:
- Annotation processors
- Compile-time code generators
- Build tools

### Dependency Resolution Decision Tree

```
For each unsatisfied dependency:
│
├─ Is there a XA4242 suggestion?
│   └─ YES → Install the suggested NuGet package
│
├─ Is the dependency needed from C#?
│   ├─ YES → Find/create a binding NuGet or create your own binding project
│   └─ NO → Use AndroidMavenLibrary with Bind="false"
│
├─ Is it a compile-time only dependency (annotations)?
│   └─ YES → Use AndroidIgnoredJavaDependency
│
└─ Otherwise → Create a separate binding project or include AAR/JAR directly
```

### Practical Example: Binding a Complex Library

Let's say you want to bind a library that depends on OkHttp, Kotlin, and AndroidX:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
  </PropertyGroup>

  <!-- The library to bind -->
  <ItemGroup>
    <AndroidMavenLibrary Include="com.example:complexlib" Version="2.0.0" />
  </ItemGroup>

  <!-- Dependencies satisfied by NuGet (preferred) -->
  <ItemGroup>
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.3" />
    <PackageReference Include="Xamarin.AndroidX.AppCompat" Version="1.6.1.6" />
    <PackageReference Include="Xamarin.Kotlin.StdLib" Version="1.9.22" />
    <PackageReference Include="Square.OkHttp3" Version="4.12.0" />
  </ItemGroup>

  <!-- Dependencies with no NuGet - include but don't bind -->
  <ItemGroup>
    <AndroidMavenLibrary 
      Include="com.example:internal-util" 
      Version="1.0.0" 
      Bind="false" />
  </ItemGroup>

  <!-- Compile-time only dependencies -->
  <ItemGroup>
    <AndroidIgnoredJavaDependency 
      Include="org.jetbrains:annotations:23.0.0" />
  </ItemGroup>
</Project>
```

---

## Customizing Bindings with Metadata

Even after resolving dependencies, the generated C# code often needs adjustments. This is done via `Transforms/Metadata.xml`.

### How Binding Generation Works

1. **Parse Phase**: The binding generator reads the JAR/AAR and produces `obj/Debug/api.xml`
2. **Transform Phase**: `Metadata.xml` transforms modify the API definition
3. **Generate Phase**: C# code is generated from the transformed API

### Common Metadata Transforms

#### Removing Obfuscated or Internal Types

```xml
<metadata>
  <!-- Remove obfuscated packages (single letters, $, etc.) -->
  <remove-node path="/api/package[starts-with(@name, 'com.example.internal')]" />
  <remove-node path="/api/package[@name='a']" />
  <remove-node path="/api/package[@name='b']" />
  
  <!-- Remove specific classes -->
  <remove-node path="/api/package[@name='com.example']/class[@name='InternalHelper']" />
  
  <!-- Remove classes with $ (often internal/anonymous) -->
  <remove-node path="/api/package/class[contains(@name, '$')]" />
</metadata>
```

#### Changing Namespaces

```xml
<metadata>
  <!-- Change Java package to .NET namespace -->
  <attr 
    path="/api/package[@name='com.example.library']" 
    name="managedName">Example.Library</attr>
    
  <!-- Multiple packages -->
  <attr path="/api/package[@name='com.example.library.ui']" 
    name="managedName">Example.Library.UI</attr>
</metadata>
```

#### Renaming Types and Members

```xml
<metadata>
  <!-- Rename a class -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='Util']" 
    name="managedName">Utilities</attr>
    
  <!-- Rename a method -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='getData']" 
    name="managedName">GetData</attr>
    
  <!-- Rename parameters (p0, p1 → meaningful names) -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='process']/parameter[@name='p0']" 
    name="name">input</attr>
</metadata>
```

#### Fixing Visibility Issues

```xml
<metadata>
  <!-- Make a class public -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='Helper']" 
    name="visibility">public</attr>
</metadata>
```

#### Handling Obfuscated Code

```xml
<metadata>
  <!-- Mark obfuscated classes to still generate bindings -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='a']" 
    name="obfuscated">false</attr>
</metadata>
```

#### Fixing Return Types

```xml
<metadata>
  <!-- Change method return type -->
  <attr 
    path="/api/package[@name='com.example']/class[@name='MyClass']/method[@name='getObject']" 
    name="managedReturn">Java.Lang.Object</attr>
</metadata>
```

### The api.xml Reference

When troubleshooting, examine `obj/Debug/net9.0-android/api.xml` to see what's being generated. This helps construct the correct XPath for your transforms.

---

## Native Library Interop (Slim Bindings) for Android

This approach creates a thin Java/Kotlin wrapper, exposing only the APIs you need.

### Project Structure

```
MyBinding/
├── android/
│   ├── native/                          # Android Studio project
│   │   ├── app/
│   │   │   └── src/main/java/
│   │   │       └── com/example/
│   │   │           └── DotnetMyBinding.java
│   │   ├── build.gradle.kts
│   │   └── settings.gradle.kts
│   └── MyBinding.Android.Binding/       # .NET binding project
│       ├── MyBinding.Android.Binding.csproj
│       └── Transforms/
│           └── Metadata.xml
└── sample/
    └── MauiSample/
```

### Step 1: Create the Android Studio Wrapper Project

Create a new Android Library module in Android Studio.

#### build.gradle.kts (Module)

```kotlin
plugins {
    id("com.android.library")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.example.mybinding"
    compileSdk = 34

    defaultConfig {
        minSdk = 21
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }
}

dependencies {
    // The native library you're wrapping
    implementation("com.thirdparty:awesomelib:1.0.0")
    
    // Any other dependencies
    implementation("androidx.core:core-ktx:1.12.0")
}
```

#### settings.gradle.kts

```kotlin
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
        // Add custom repositories if needed
        maven { url = uri("https://jitpack.io") }
    }
}
```

### Step 2: Create the Wrapper Class

**Java Example:** `DotnetMyBinding.java`

```java
package com.example.mybinding;

import android.content.Context;
import android.util.Log;
import com.thirdparty.awesomelib.AwesomeLib;
import com.thirdparty.awesomelib.AwesomeCallback;

public class DotnetMyBinding {
    private static final String TAG = "DotnetMyBinding";
    private static AwesomeLib instance;

    // Initialize the library
    public static void initialize(Context context, boolean debug) {
        AwesomeLib.Builder builder = new AwesomeLib.Builder(context);
        if (debug) {
            builder.setLoggingEnabled(true);
        }
        instance = builder.build();
    }

    // Simple method wrapper
    public static String getData() {
        if (instance == null) {
            throw new IllegalStateException("Must call initialize() first");
        }
        return instance.getData();
    }

    // Callback-based method
    public static void fetchDataAsync(final DataCallback callback) {
        if (instance == null) {
            callback.onError("Not initialized");
            return;
        }
        
        instance.fetchData(new AwesomeCallback() {
            @Override
            public void onSuccess(String result) {
                callback.onSuccess(result);
            }

            @Override
            public void onError(Exception e) {
                callback.onError(e.getMessage());
            }
        });
    }

    // Callback interface for .NET
    public interface DataCallback {
        void onSuccess(String result);
        void onError(String error);
    }
}
```

**Kotlin Example:** `DotnetMyBinding.kt`

```kotlin
package com.example.mybinding

import android.content.Context
import com.thirdparty.awesomelib.AwesomeLib

object DotnetMyBinding {
    private var instance: AwesomeLib? = null

    @JvmStatic
    fun initialize(context: Context, debug: Boolean) {
        instance = AwesomeLib.Builder(context)
            .setLoggingEnabled(debug)
            .build()
    }

    @JvmStatic
    fun getData(): String {
        return instance?.getData() 
            ?: throw IllegalStateException("Must call initialize() first")
    }

    @JvmStatic
    fun fetchDataAsync(callback: DataCallback) {
        instance?.fetchData { result, error ->
            if (error != null) {
                callback.onError(error.message ?: "Unknown error")
            } else {
                callback.onSuccess(result ?: "")
            }
        } ?: callback.onError("Not initialized")
    }

    interface DataCallback {
        fun onSuccess(result: String)
        fun onError(error: String)
    }
}
```

**Key Points:**
- Use `@JvmStatic` in Kotlin for static methods
- Keep method signatures simple (primitives, String, common Android types)
- Use callback interfaces instead of coroutines/lambdas for async operations

### Step 3: Build the AAR

```bash
cd android/native
./gradlew :app:assembleRelease
```

The AAR will be at `app/build/outputs/aar/app-release.aar`.

### Step 4: Configure the Binding Project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-android</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
  </PropertyGroup>

  <!-- Your wrapper AAR -->
  <ItemGroup>
    <AndroidLibrary Include="../native/app/build/outputs/aar/app-release.aar" />
  </ItemGroup>

  <!-- Dependencies required by your wrapper -->
  <ItemGroup Condition="$(TargetFramework.Contains('android'))">
    <!-- The library you're wrapping - don't bind, just include -->
    <AndroidMavenLibrary 
      Include="com.thirdparty:awesomelib" 
      Version="1.0.0" 
      Bind="false" />
      
    <!-- NuGet packages for common dependencies -->
    <PackageReference Include="Xamarin.AndroidX.Core" Version="1.12.0.3" />
    <PackageReference Include="Xamarin.Kotlin.StdLib" Version="1.9.22" />
  </ItemGroup>

  <!-- Metadata transforms -->
  <ItemGroup>
    <TransformFile Include="Transforms\Metadata.xml" />
  </ItemGroup>
</Project>
```

### Step 5: Optional Metadata Customization

`Transforms/Metadata.xml`:

```xml
<metadata>
  <!-- Rename the namespace -->
  <attr 
    path="/api/package[@name='com.example.mybinding']" 
    name="managedName">MyBinding</attr>
</metadata>
```

### Step 6: Reference in Your App

Add to your MAUI app's `.csproj`:

```xml
<ItemGroup Condition="$(TargetFramework.Contains('android'))">
  <ProjectReference Include="..\android\MyBinding.Android.Binding\MyBinding.Android.Binding.csproj" />
</ItemGroup>
```

### Step 7: Use in C#

```csharp
#if ANDROID
using MyBinding;
using Android.Runtime;
#endif

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
#if ANDROID
        var context = Platform.CurrentActivity ?? Platform.AppContext;
        Com.Example.Mybinding.DotnetMyBinding.Initialize(context, true);
#endif
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
#if ANDROID
        // Sync method
        var data = Com.Example.Mybinding.DotnetMyBinding.GetData();
        
        // Async with callback
        Com.Example.Mybinding.DotnetMyBinding.FetchDataAsync(new MyDataCallback(
            onSuccess: result => MainThread.BeginInvokeOnMainThread(() => 
                DisplayAlert("Success", result, "OK")),
            onError: error => MainThread.BeginInvokeOnMainThread(() => 
                DisplayAlert("Error", error, "OK"))
        ));
#endif
    }
}

#if ANDROID
// Implement the callback interface
public class MyDataCallback : Java.Lang.Object, Com.Example.Mybinding.DotnetMyBinding.IDataCallback
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

---

## Handling Native Libraries (.so files)

Some Android libraries include native code (`.so` files). If you get `java.lang.UnsatisfiedLinkError`, you need to ensure the native libraries are loaded.

### Including .so Files

For AAR files, `.so` files are usually included automatically. For JAR files, you may need to add them manually:

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

---

## Troubleshooting Common Issues

### Build Errors

#### "Type is defined multiple times"
You have duplicate classes, often from including the same dependency twice (once via NuGet, once via AAR).

**Solution:** Check your dependencies for overlap. Use `Bind="false"` or remove redundant references.

```xml
<!-- If kotlin-stdlib is in both NuGet and your AAR -->
<AndroidLibrary Update="mylibrary.aar">
  <!-- Exclude the embedded kotlin from AAR -->
</AndroidLibrary>
```

#### "Java dependency 'X' is not satisfied"
Missing transitive dependency.

**Solution:** Add the dependency via NuGet, AndroidMavenLibrary, or AndroidIgnoredJavaDependency.

#### "Class 'X' does not implement interface member 'Y'"
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

### Runtime Errors

#### `Java.Lang.NoClassDefFoundError`
Missing dependency at runtime.

**Solution:** You ignored a required dependency. Remove it from `AndroidIgnoredJavaDependency` and properly satisfy it.

#### `Java.Lang.UnsatisfiedLinkError`
Native library (.so) not loaded.

**Solution:** Ensure .so files are included and call `JavaSystem.LoadLibrary()` if needed.

---

## Best Practices

### Dependency Management

1. **Prefer NuGet packages** - They handle transitive dependencies automatically
2. **Use Java Dependency Verification** - Upgrade to .NET 9+ for error XA4241/XA4242
3. **Don't bind what you don't need** - Use `Bind="false"` liberally
4. **Version alignment** - Match NuGet package versions with what your library expects

### Project Organization

1. **Separate binding projects** - One per major library
2. **Document dependencies** - Comment why each dependency is included
3. **Test on real devices** - Emulators may hide some linking issues

### For Slim Bindings

1. **Keep wrapper APIs simple** - Primitives, strings, common types
2. **Use callback interfaces** - Not lambdas or coroutines
3. **Test the native project first** - Ensure it works in pure Android before binding

---

## Quick Reference

### Build Items

| Item | Purpose |
|------|---------|
| `<AndroidLibrary>` | Include AAR/JAR in binding |
| `<AndroidMavenLibrary>` | Download and include from Maven |
| `<AndroidIgnoredJavaDependency>` | Ignore a dependency (compile-time only) |
| `<AndroidNativeLibrary>` | Include .so files |
| `<TransformFile>` | Metadata transform files |

### Common Attributes

| Attribute | Values | Purpose |
|-----------|--------|---------|
| `Bind` | `true`/`false` | Whether to generate C# bindings |
| `Pack` | `true`/`false` | Whether to include in NuGet package |
| `JavaArtifact` | `groupId:artifactId:version` | Declare what Maven artifact this satisfies |
| `Repository` | `Central`/`Google`/URL | Maven repository to download from |
| `VerifyDependencies` | `true`/`false` | Enable Java Dependency Verification |

### Common NuGet Packages

| Maven Artifact | NuGet Package |
|----------------|---------------|
| `androidx.core:core` | `Xamarin.AndroidX.Core` |
| `androidx.appcompat:appcompat` | `Xamarin.AndroidX.AppCompat` |
| `androidx.recyclerview:recyclerview` | `Xamarin.AndroidX.RecyclerView` |
| `androidx.fragment:fragment` | `Xamarin.AndroidX.Fragment` |
| `androidx.activity:activity` | `Xamarin.AndroidX.Activity` |
| `androidx.lifecycle:lifecycle-*` | `Xamarin.AndroidX.Lifecycle.*` |
| `org.jetbrains.kotlin:kotlin-stdlib` | `Xamarin.Kotlin.StdLib` |
| `org.jetbrains.kotlinx:kotlinx-coroutines-*` | `Xamarin.KotlinX.Coroutines.*` |
| `com.google.android.gms:play-services-base` | `Xamarin.GooglePlayServices.Base` |
| `com.google.android.material:material` | `Xamarin.Google.Android.Material` |
| `com.squareup.okhttp3:okhttp` | `Square.OkHttp3` |
| `com.google.code.gson:gson` | `GoogleGson` |

---

## Resources

- [Binding a Java Library (Microsoft Docs)](https://learn.microsoft.com/dotnet/android/binding-libs/binding-java-libs/binding-java-library)
- [Binding from Maven (Microsoft Docs)](https://learn.microsoft.com/dotnet/android/binding-libs/binding-java-libs/binding-java-maven-library)
- [Java Dependency Verification](https://learn.microsoft.com/dotnet/android/binding-libs/advanced-concepts/java-dependency-verification)
- [Resolving Java Dependencies](https://learn.microsoft.com/dotnet/android/binding-libs/advanced-concepts/resolving-java-dependencies)
- [Java Bindings Metadata](https://learn.microsoft.com/dotnet/android/binding-libs/customizing-bindings/java-bindings-metadata)
- [Maui.NativeLibraryInterop Repository](https://github.com/CommunityToolkit/Maui.NativeLibraryInterop)
- [Xamarin.AndroidX Repository](https://github.com/xamarin/AndroidX)