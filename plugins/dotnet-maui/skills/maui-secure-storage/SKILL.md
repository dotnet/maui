---
name: maui-secure-storage
description: >
  Add secure storage to .NET MAUI apps using SecureStorage.Default.
  Covers SetAsync, GetAsync, Remove, RemoveAll, platform setup
  (Android backup rules, iOS Keychain entitlements, Windows limits),
  common pitfalls, and a DI wrapper service for testability.
  USE FOR: "secure storage", "SecureStorage", "store token securely", "Keychain",
  "Android Keystore", "save secret", "encrypted storage", "store credentials",
  "sensitive data storage".
  DO NOT USE FOR: general file storage (use maui-file-handling), SQLite databases
  (use maui-sqlite-database), or authentication flows (use maui-authentication).
---

# Secure Storage in .NET MAUI

## API Surface

Use `SecureStorage.Default` (implements `ISecureStorage`):

```csharp
// Store
await SecureStorage.Default.SetAsync("auth_token", token);

// Retrieve (returns null if not found)
string? token = await SecureStorage.Default.GetAsync("auth_token");

// Remove single key
bool removed = SecureStorage.Default.Remove("auth_token");

// Remove all
SecureStorage.Default.RemoveAll();
```

All values are **strings only**. Serialize complex data to JSON first.

## Platform Setup

### Android — Handle Auto Backup

Auto Backup can restore encrypted preferences to a new device where the encryption key is invalid, causing unrecoverable exceptions. Choose one approach:

**Option A — Disable Auto Backup entirely:**

In `Platforms/Android/AndroidManifest.xml`:
```xml
<application android:allowBackup="false" ...>
```

**Option B — Exclude secure storage from backup:**

1. Create `Platforms/Android/Resources/xml/auto_backup_rules.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<full-backup-content>
  <exclude domain="sharedpref"
           path="${applicationId}.microsoft.maui.essentials.preferences.xml" />
</full-backup-content>
```

2. Reference it in `AndroidManifest.xml`:
```xml
<application android:fullBackupContent="@xml/auto_backup_rules" ...>
```

### iOS / Mac Catalyst — Enable Keychain

In `Platforms/iOS/Entitlements.plist` (and `Platforms/MacCatalyst/Entitlements.plist`):
```xml
<dict>
  <key>keychain-access-groups</key>
  <array>
    <string>$(AppIdentifierPrefix)com.yourcompany.yourapp</string>
  </array>
</dict>
```

> **Simulator only:** Add the keychain access group matching your bundle ID. Remove it before building for physical devices or App Store submission — it is not needed there and can cause signing issues.

### Windows

No setup required. Limits:
- Key name: max **255 characters**
- Value: max **8 KB** per setting
- Composite storage: max **64 KB** total

## Gotchas and Best Practices

1. **Small text only.** Do not store large blobs. Store tokens, passwords, short secrets.
2. **Wrap in try/catch.** On Android, corrupted values from backup restoration throw exceptions. Always handle failures gracefully:
   ```csharp
   try
   {
       var value = await SecureStorage.Default.GetAsync("key");
   }
   catch (Exception)
   {
       SecureStorage.Default.RemoveAll();
   }
   ```
3. **iOS iCloud Keychain sync.** Values may sync across devices via iCloud Keychain if the user has it enabled. This is platform behavior, not controllable from MAUI.
4. **iOS uninstall does not clear Keychain.** Unlike Android, uninstalling an iOS app does **not** remove its Keychain entries. Values persist and are available if the app is reinstalled.
5. **No sensitive logging.** Never log secret values retrieved from secure storage.

## DI Wrapper Service for Testability

### Define the interface

```csharp
public interface ISecureStorageService
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    bool Remove(string key);
    void RemoveAll();
}
```

### Implement against SecureStorage.Default

```csharp
public class SecureStorageService : ISecureStorageService
{
    public Task SetAsync(string key, string value)
        => SecureStorage.Default.SetAsync(key, value);

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch (Exception)
        {
            // Corrupted value — clear and return null
            SecureStorage.Default.RemoveAll();
            return null;
        }
    }

    public bool Remove(string key)
        => SecureStorage.Default.Remove(key);

    public void RemoveAll()
        => SecureStorage.Default.RemoveAll();
}
```

### Register in MauiProgram.cs

```csharp
builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
```

### Inject into view models

```csharp
public class LoginViewModel
{
    private readonly ISecureStorageService _secure;

    public LoginViewModel(ISecureStorageService secure)
    {
        _secure = secure;
    }

    public async Task SaveTokenAsync(string token)
    {
        await _secure.SetAsync("auth_token", token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _secure.GetAsync("auth_token");
    }
}
```

### Mock in tests

```csharp
var mock = new Mock<ISecureStorageService>();
mock.Setup(s => s.GetAsync("auth_token"))
    .ReturnsAsync("test-token-value");

var vm = new LoginViewModel(mock.Object);
```
