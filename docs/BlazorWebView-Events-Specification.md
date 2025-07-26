# BlazorWebView Initialization Events Specification

## Overview

This document provides a comprehensive specification for the `BlazorWebViewInitializing` and `BlazorWebViewInitialized` events in the BlazorWebView control, based on analysis of all GitHub issues and user feedback. This specification serves as a reference for implementing similar functionality in HybridWebView and understanding the complete requirements for webview initialization events.

## Event Definitions

### BlazorWebViewInitializing Event

**Purpose**: Raised before the web view is initialized. On some platforms this enables customizing the web view configuration.

**Event Args**: `BlazorWebViewInitializingEventArgs`

**Platform Support**:
- ✅ Windows (WebView2)
- ✅ iOS/MacCatalyst (WKWebView)
- ❌ Android (Not supported)
- ❌ Tizen (Not supported)

### BlazorWebViewInitialized Event

**Purpose**: Raised after the web view is initialized but before any component has been rendered. The event arguments provide the instance of the platform-specific web view control.

**Event Args**: `BlazorWebViewInitializedEventArgs`

**Platform Support**:
- ✅ Windows (WebView2)
- ✅ iOS/MacCatalyst (WKWebView)
- ✅ Android (WebView)
- ✅ Tizen (WebView)

## Event Arguments Details

### BlazorWebViewInitializingEventArgs

Platform-specific properties available for configuration:

#### Windows (WebView2)
```csharp
public class BlazorWebViewInitializingEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the browser executable folder path for the WebView2Control.
    /// </summary>
    public string BrowserExecutableFolder { get; set; }

    /// <summary>
    /// Gets or sets the user data folder path for the WebView2Control.
    /// </summary>
    public string UserDataFolder { get; set; }

    /// <summary>
    /// Gets or sets the environment options for the WebView2Control.
    /// </summary>
    public CoreWebView2EnvironmentOptions EnvironmentOptions { get; set; }
}
```

#### iOS/MacCatalyst (WKWebView)
```csharp
public class BlazorWebViewInitializingEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the web view WKWebViewConfiguration.
    /// </summary>
    public WKWebViewConfiguration Configuration { get; set; }
}
```

### BlazorWebViewInitializedEventArgs

Platform-specific properties available for access:

#### Windows (WebView2)
```csharp
public class BlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the WebView2Control instance that was initialized.
    /// </summary>
    public WebView2Control WebView { get; internal set; }
}
```

#### Android (WebView)
```csharp
public class BlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the AWebView instance that was initialized.
    /// </summary>
    public AWebView WebView { get; internal set; }
}
```

#### iOS/MacCatalyst (WKWebView)
```csharp
public class BlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the WKWebView instance that was initialized.
    /// </summary>
    public WKWebView WebView { get; internal set; }
}
```

#### Tizen (WebView)
```csharp
public class BlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the TWebView instance that was initialized.
    /// </summary>
    public TWebView WebView { get; internal set; }
}
```

## Use Cases and Examples

### 1. WebView2 Data Directory Control (Windows)

**Problem**: Control where WebView2 stores user data to avoid permission issues in restricted directories.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitializing += (sender, e) =>
{
    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    e.UserDataFolder = Path.Combine(appDataPath, "MyApp", "WebView2Data");
    Directory.CreateDirectory(e.UserDataFolder);
};
```

**GitHub Issues**: [#10772](https://github.com/dotnet/maui/issues/10772), [#11144](https://github.com/dotnet/maui/issues/11144), [#26746](https://github.com/dotnet/maui/issues/26746)

### 2. WebView2 Runtime Version Control (Windows)

**Problem**: Need to use a specific WebView2 runtime version for compatibility or testing.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitializing += (sender, e) =>
{
    e.BrowserExecutableFolder = @"C:\MyApp\WebView2Runtime\Microsoft.WebView2.FixedVersionRuntime.113.0.1774.57.x64";
};
```

**GitHub Issues**: [#15304](https://github.com/dotnet/maui/issues/15304)

### 3. Browser Security Settings (Windows)

**Problem**: Need to disable web security for development or specific use cases.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitializing += (sender, e) =>
{
    e.EnvironmentOptions = new CoreWebView2EnvironmentOptions
    {
        AdditionalBrowserArguments = "--disable-web-security --disable-features=VizDisplayCompositor"
    };
};
```

**GitHub Issues**: [#14433](https://github.com/dotnet/maui/issues/14433), [#20969](https://github.com/dotnet/maui/issues/20969)

### 4. WebView2 Settings Configuration (Windows)

**Problem**: Need to control zoom, keyboard shortcuts, external drops, and other WebView2 settings.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitialized += (sender, e) =>
{
#if WINDOWS
    var settings = e.WebView.CoreWebView2.Settings;
    settings.IsZoomControlEnabled = false;
    settings.AreBrowserAcceleratorKeysEnabled = false;
    e.WebView.AllowExternalDrop = false;
    settings.AreDevToolsEnabled = false;
#endif
};
```

**GitHub Issues**: [#19419](https://github.com/dotnet/maui/issues/19419), [#8478](https://github.com/dotnet/maui/issues/8478), [#7569](https://github.com/dotnet/maui/issues/7569)

### 5. iOS Video Playback Configuration

**Problem**: Configure WKWebView for inline video playback and media handling.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitializing += (sender, e) =>
{
#if IOS || MACCATALYST
    e.Configuration.AllowsInlineMediaPlayback = true;
    e.Configuration.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
#endif
};
```

**GitHub Issues**: [#16013](https://github.com/dotnet/maui/issues/16013)

### 6. Transparent Background Configuration

**Problem**: Need transparent WebView background for overlay effects.

**Solution**:
```csharp
// iOS/MacCatalyst
blazorWebView.BlazorWebViewInitializing += (sender, e) =>
{
#if IOS || MACCATALYST
    e.Configuration.WebpagePreferences.AllowsContentJavaScript = true;
    // Additional transparency settings
#endif
};

// Windows - requires BlazorWebViewInitialized
blazorWebView.BlazorWebViewInitialized += (sender, e) =>
{
#if WINDOWS
    e.WebView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
#endif
};
```

**GitHub Issues**: [#14185](https://github.com/dotnet/maui/issues/14185), [#14407](https://github.com/dotnet/maui/issues/14407)

### 7. Android Mixed Content Configuration

**Problem**: Allow HTTP content in HTTPS context for local development.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitialized += (sender, e) =>
{
#if ANDROID
    e.WebView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
#endif
};
```

**GitHub Issues**: [#17219](https://github.com/dotnet/maui/issues/17219), [#10141](https://github.com/dotnet/maui/issues/10141)

### 8. Development and Debugging

**Problem**: Enable debugging tools and inspect WebView content.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitialized += (sender, e) =>
{
#if WINDOWS
    e.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
    e.WebView.CoreWebView2.DocumentTitleChanged += (s, args) => 
    {
        System.Diagnostics.Debug.WriteLine($"Title changed: {e.WebView.CoreWebView2.DocumentTitle}");
    };
#elif IOS || MACCATALYST
    e.WebView.SetValueForKey(NSNumber.FromBoolean(true), new NSString("inspectable"));
#endif
};
```

**GitHub Issues**: [#7706](https://github.com/dotnet/maui/issues/7706)

### 9. Process Failure Handling

**Problem**: Handle WebView2 process crashes gracefully.

**Solution**:
```csharp
blazorWebView.BlazorWebViewInitialized += (sender, e) =>
{
#if WINDOWS
    e.WebView.CoreWebView2.ProcessFailed += (s, args) =>
    {
        // Log the failure
        System.Diagnostics.Debug.WriteLine($"WebView2 process failed: {args.Reason}");
        
        // Optionally restart or recover
        if (args.Reason == Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedKind.BrowserProcessExited)
        {
            // Handle browser process restart
        }
    };
#endif
};
```

**GitHub Issues**: [#6481](https://github.com/dotnet/maui/issues/6481)

## Known Issues and Limitations

### 1. Threading Issues with Disposal

**Issue**: The `DisposeAsync` implementation uses `ConfigureAwait(false)` which can cause WebView disposal on the wrong thread.

**Impact**: COM exceptions when disposing BlazorWebView in WPF applications.

**Workaround**: Use `ConfigureAwait(true)` or dispose on UI thread.

**Status**: Acknowledged, fix planned for .NET 10

**GitHub Issues**: [#26746](https://github.com/dotnet/maui/issues/26746)

### 2. BlazorWebViewInitializing Not Working on Windows MAUI

**Issue**: The BlazorWebViewInitializing event sometimes doesn't fire or settings are ignored on Windows MAUI applications.

**Impact**: Cannot configure WebView2 settings like UserDataFolder.

**Workaround**: Set environment variables before WebView creation:
```csharp
Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", "C:\\path\\to\\folder");
Environment.SetEnvironmentVariable("WEBVIEW2_BROWSER_EXECUTABLE_FOLDER", "C:\\path\\to\\runtime");
```

**Status**: Ongoing investigation

**GitHub Issues**: [#15304](https://github.com/dotnet/maui/issues/15304), [#11144](https://github.com/dotnet/maui/issues/11144)

### 3. Platform-Specific Limitations

#### Windows
- Some WebView2 command line arguments are ignored for security (e.g., `--user-data-dir`)
- AllowExternalDrop defaults to true (security concern)

#### iOS/MacCatalyst
- Cannot intercept HTTPS requests for security reasons
- Limited configuration options compared to Windows

#### Android
- No BlazorWebViewInitializing event support
- Limited to post-initialization configuration only

### 4. Application Hanging Issues

**Issue**: Applications sometimes hang on startup with white/blank screen.

**Impact**: Poor developer experience, apps may need restart.

**Possible Causes**: 
- Timing issues during initialization
- Hot reload interference
- Exception handling in initialization events

**Workarounds**: 
- Disable CLR exception handling during debugging
- Restart debugging session

**Status**: Ongoing investigation

**GitHub Issues**: [#15533](https://github.com/dotnet/maui/issues/15533)

## Platform Support Matrix

| Feature | Windows | iOS/MacCatalyst | Android | Tizen |
|---------|---------|-----------------|---------|-------|
| BlazorWebViewInitializing | ✅ | ✅ | ❌ | ❌ |
| BlazorWebViewInitialized | ✅ | ✅ | ✅ | ✅ |
| Runtime Path Control | ✅ | ❌ | ❌ | ❌ |
| Data Folder Control | ✅ | ❌ | ❌ | ❌ |
| Browser Arguments | ✅ (limited) | ❌ | ❌ | ❌ |
| Security Settings | ✅ | ✅ (limited) | ✅ (limited) | ❓ |
| Debugging Tools | ✅ | ✅ | ✅ | ❓ |
| Request Interception | ✅ | ❌ (HTTPS) | ✅ | ❓ |
| Transparent Background | ✅ | ✅ | ✅ | ❓ |

## Test Suite Requirements

Based on the issues and use cases identified, a comprehensive test suite should cover:

### Functional Tests

1. **Event Firing Tests**
   - Verify BlazorWebViewInitializing fires before initialization
   - Verify BlazorWebViewInitialized fires after initialization but before rendering
   - Test event firing order and timing

2. **Configuration Tests**
   - Test UserDataFolder setting (Windows)
   - Test BrowserExecutableFolder setting (Windows)
   - Test EnvironmentOptions configuration (Windows)
   - Test WKWebViewConfiguration setting (iOS/MacCatalyst)

3. **Platform WebView Access Tests**
   - Test WebView property accessibility in BlazorWebViewInitialized
   - Test platform-specific settings application
   - Test WebView lifecycle management

### Integration Tests

1. **Cross-Platform Consistency**
   - Test behavior consistency across platforms
   - Test feature availability per platform
   - Test graceful degradation on unsupported platforms

2. **Threading Tests**
   - Test event firing on correct thread
   - Test disposal on correct thread
   - Test ConfigureAwait behavior

3. **Error Handling Tests**
   - Test invalid configuration handling
   - Test missing directory creation
   - Test process failure scenarios

### Performance Tests

1. **Initialization Performance**
   - Measure event overhead
   - Test startup time impact
   - Memory usage during initialization

2. **Reliability Tests**
   - Test repeated initialization/disposal cycles
   - Test under memory pressure
   - Test with complex configurations

### Regression Tests

1. **Known Issue Prevention**
   - Test for threading issues in disposal
   - Test for event not firing scenarios
   - Test for configuration ignored scenarios

2. **Security Tests**
   - Test AllowExternalDrop default behavior
   - Test browser security argument restrictions
   - Test data folder permission scenarios

## Implementation Guidelines for HybridWebView

Based on the BlazorWebView experience, implementing similar events in HybridWebView should consider:

### 1. Event Design
- Follow same naming pattern: `HybridWebViewInitializing` and `HybridWebViewInitialized`
- Use similar event args structure with platform-specific properties
- Ensure events fire at correct times in lifecycle

### 2. Platform Abstractions
- Create platform-specific event args classes
- Use conditional compilation for platform-specific properties
- Document platform support clearly

### 3. Threading Considerations
- Fire events on UI thread
- Use `ConfigureAwait(true)` in async disposal
- Ensure thread-safe access to WebView properties

### 4. Error Handling
- Provide clear error messages for invalid configurations
- Handle missing directories gracefully
- Document platform limitations

### 5. Documentation
- Provide platform-specific examples
- Document known limitations
- Include troubleshooting guides

## Related GitHub Issues

### Primary Implementation Issues
- [#26767](https://github.com/dotnet/maui/issues/26767) - HybridWebView needs initialization events
- [#26746](https://github.com/dotnet/maui/issues/26746) - Threading issue in disposal
- [#15304](https://github.com/dotnet/maui/issues/15304) - BlazorWebViewInitializing not working on Windows MAUI
- [#11144](https://github.com/dotnet/maui/issues/11144) - UserDataFolder setting not working

### Use Case and Feature Requests
- [#10772](https://github.com/dotnet/maui/issues/10772) - Cannot update WebView2 UserDataFolder
- [#11382](https://github.com/dotnet/maui/issues/11382) - Intercepting requests from BlazorWebView
- [#14433](https://github.com/dotnet/maui/issues/14433) - Disable web security in BlazorWebView
- [#16013](https://github.com/dotnet/maui/issues/16013) - Video playsinline attribute not working
- [#7569](https://github.com/dotnet/maui/issues/7569) - Unable to disable zooming
- [#8478](https://github.com/dotnet/maui/issues/8478) - AllowExternalDrop security issue

### Platform-Specific Issues
- [#7706](https://github.com/dotnet/maui/issues/7706) - Safari developer tools not available (macOS)
- [#17219](https://github.com/dotnet/maui/issues/17219) - Mixed content issues (Android)
- [#14185](https://github.com/dotnet/maui/issues/14185), [#14407](https://github.com/dotnet/maui/issues/14407) - Transparent background

### Stability Issues
- [#15533](https://github.com/dotnet/maui/issues/15533) - Application hanging with white screen
- [#6481](https://github.com/dotnet/maui/issues/6481) - Process failure handling

## Conclusion

The BlazorWebView initialization events provide crucial functionality for configuring platform-specific webview settings. While they have proven invaluable for many use cases, several issues remain around timing, threading, and platform consistency. Implementing similar functionality in HybridWebView should address these known issues while maintaining the valuable customization capabilities that developers rely on.

The comprehensive test suite and implementation guidelines outlined in this specification should help ensure a robust and reliable implementation that addresses the lessons learned from the BlazorWebView experience.