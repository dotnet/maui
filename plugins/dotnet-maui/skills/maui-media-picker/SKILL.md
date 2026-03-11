---
name: maui-media-picker
description: >-
  Guidance for picking photos/videos, capturing from camera, multi-select (.NET 10),
  MediaPickerOptions, platform permissions, and FileResult handling in .NET MAUI.
  USE FOR: "pick photo", "capture photo", "take picture", "pick video", "camera capture",
  "MediaPicker", "photo gallery", "image picker", "multi-select photos", "MediaPickerOptions".
  DO NOT USE FOR: general file picking (use maui-file-handling),
  image display or optimization (use maui-performance), or camera streaming (use maui-platform-invoke).
---

# .NET MAUI Media Picker Skill

## Core API

Use `MediaPicker.Default` to pick or capture photos and videos. All methods must run on the **UI thread**.

### Single-select methods (all .NET versions)

| Method | Purpose |
|---|---|
| `MediaPicker.Default.PickPhotoAsync()` | Pick one photo from gallery |
| `MediaPicker.Default.PickVideoAsync()` | Pick one video from gallery |
| `MediaPicker.Default.CapturePhotoAsync()` | Capture a photo with the camera |
| `MediaPicker.Default.CaptureVideoAsync()` | Capture a video with the camera |

All return `Task<FileResult?>`. A **null** result means the user cancelled.

### Multi-select methods (.NET 10+)

| Method | Purpose |
|---|---|
| `MediaPicker.Default.PickPhotosAsync()` | Pick multiple photos |
| `MediaPicker.Default.PickVideosAsync()` | Pick multiple videos |

These return `Task<IEnumerable<FileResult>>`. An **empty list** means the user cancelled.

## MediaPickerOptions (.NET 10+)

Pass `MediaPickerOptions` to any pick/capture method to control behavior:

```csharp
var options = new MediaPickerOptions
{
    Title = "Select photos",        // Picker dialog title
    SelectionLimit = 5,             // Max items (0 = unlimited; multi-select only)
    MaximumWidth = 1024,            // Resize max width in pixels
    MaximumHeight = 1024,           // Resize max height in pixels
    CompressionQuality = 80,       // JPEG quality 0–100
    RotateImage = true,             // Auto-rotate per EXIF
    PreserveMetaData = true         // Keep EXIF/metadata
};

var photos = await MediaPicker.Default.PickPhotosAsync(options);
```

> **Platform note:** Android and Windows may not enforce `SelectionLimit`. Always validate the count in your code.

## FileResult usage

```csharp
var photo = await MediaPicker.Default.PickPhotoAsync();
if (photo is null)
    return; // user cancelled

// Read the stream
using var stream = await photo.OpenReadAsync();

// Useful properties
string fullPath    = photo.FullPath;
string fileName    = photo.FileName;
string contentType = photo.ContentType;
```

### Save to app storage

```csharp
async Task<string> SaveToAppDataAsync(FileResult fileResult)
{
    var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileResult.FileName);
    using var sourceStream = await fileResult.OpenReadAsync();
    using var targetStream = File.OpenWrite(targetPath);
    await sourceStream.CopyToAsync(targetStream);
    return targetPath;
}
```

## Platform permissions

### Android

Add to `Platforms/Android/AndroidManifest.xml`:

```xml
<!-- Camera capture -->
<uses-permission android:name="android.permission.CAMERA" />

<!-- Storage: API ≤ 32 (Android 12 and below) -->
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE"
                 android:maxSdkVersion="32" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE"
                 android:maxSdkVersion="32" />

<!-- Storage: API ≥ 33 (Android 13+) -->
<uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
<uses-permission android:name="android.permission.READ_MEDIA_VIDEO" />
<uses-permission android:name="android.permission.READ_MEDIA_AUDIO" />
```

Also add inside `<application>`:

```xml
<queries>
    <intent>
        <action android:name="android.media.action.IMAGE_CAPTURE" />
    </intent>
</queries>
```

### iOS / Mac Catalyst

Add to `Platforms/iOS/Info.plist` (and `Platforms/MacCatalyst/Info.plist`):

```xml
<key>NSCameraUsageDescription</key>
<string>This app needs camera access to take photos</string>
<key>NSMicrophoneUsageDescription</key>
<string>This app needs microphone access to record video</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs photo library access to pick media</string>
<key>NSPhotoLibraryAddUsageDescription</key>
<string>This app needs permission to save photos</string>
```

### Windows

No additional permissions required.

## Check availability before capture

```csharp
if (MediaPicker.Default.IsCaptureSupported)
{
    var photo = await MediaPicker.Default.CapturePhotoAsync();
    // ...
}
```

## Multi-select example (.NET 10+)

```csharp
var options = new MediaPickerOptions { SelectionLimit = 10 };
var results = await MediaPicker.Default.PickPhotosAsync(options);

if (!results.Any())
    return; // user cancelled

foreach (var file in results)
{
    using var stream = await file.OpenReadAsync();
    // process each selected photo
}
```

## Key rules

- Always call media picker methods on the **main/UI thread**.
- Single-select cancellation returns **null**; multi-select returns an **empty list**.
- Use `IsCaptureSupported` before calling capture methods.
- Android/Windows may ignore `SelectionLimit` — validate result count yourself.
- `OpenReadAsync()` gives a stream; dispose it when done.
- Save files to `FileSystem.AppDataDirectory` for persistent app-local storage.
