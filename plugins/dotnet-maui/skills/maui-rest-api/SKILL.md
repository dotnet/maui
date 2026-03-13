---
name: maui-rest-api
description: >
  Guidance for consuming REST APIs in .NET MAUI apps. Covers HttpClient setup
  with System.Text.Json, DI registration, service interface/implementation
  pattern, full CRUD operations (GET, POST, PUT, DELETE), error handling,
  platform-specific clear-text traffic configuration, and async/await best practices.
  USE FOR: "REST API", "HttpClient", "call API", "GET request", "POST request",
  "API service", "JSON deserialization", "CRUD operations", "clear-text traffic",
  "consume API MAUI".
  DO NOT USE FOR: Aspire service discovery (use maui-aspire), authentication token handling
  (use maui-authentication), or local database storage (use maui-sqlite-database).
---

# Consuming REST APIs in .NET MAUI

## HttpClient & JSON setup

Always configure a shared `JsonSerializerOptions` with camel-case naming:

```csharp
private static readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};
```

## DI registration

Register `HttpClient` as a singleton or use `IHttpClientFactory`. Set `BaseAddress` once:

```csharp
// MauiProgram.cs
builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri("https://api.example.com")
});
builder.Services.AddSingleton<IMyApiService, MyApiService>();
```

For more control, use the factory pattern:

```csharp
builder.Services.AddHttpClient<IMyApiService, MyApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});
```

## Service interface + implementation

Define a clean interface for each API resource:

```csharp
public interface IMyApiService
{
    Task<List<Item>> GetItemsAsync();
    Task<Item?> GetItemAsync(int id);
    Task<Item?> CreateItemAsync(Item item);
    Task<bool> UpdateItemAsync(Item item);
    Task<bool> DeleteItemAsync(int id);
}
```

Implement the interface, injecting `HttpClient`:

```csharp
public class MyApiService : IMyApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public MyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
```

## CRUD operations

### GET (list)

```csharp
    public async Task<List<Item>> GetItemsAsync()
    {
        var response = await _httpClient.GetAsync("api/items");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Item>>(content, _jsonOptions) ?? [];
    }
```

### GET (single)

```csharp
    public async Task<Item?> GetItemAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/items/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Item>(content, _jsonOptions);
    }
```

### POST (create)

```csharp
    public async Task<Item?> CreateItemAsync(Item item)
    {
        var json = JsonSerializer.Serialize(item, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/items", content);
        if (!response.IsSuccessStatusCode)
            return null;
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Item>(responseBody, _jsonOptions);
    }
```

### PUT (update)

```csharp
    public async Task<bool> UpdateItemAsync(Item item)
    {
        var json = JsonSerializer.Serialize(item, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/items/{item.Id}", content);
        return response.IsSuccessStatusCode;
    }
```

### DELETE

```csharp
    public async Task<bool> DeleteItemAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/items/{id}");
        return response.IsSuccessStatusCode;
    }
}
```

## Error handling

Check `IsSuccessStatusCode` or call `EnsureSuccessStatusCode()` depending on the scenario:

- Use `EnsureSuccessStatusCode()` when failure is unexpected (throws `HttpRequestException`).
- Use `IsSuccessStatusCode` when you need to branch on specific status codes.

Wrap calls in try/catch for network-level failures:

```csharp
try
{
    var items = await _apiService.GetItemsAsync();
}
catch (HttpRequestException ex)
{
    // Network error or non-success status code
}
catch (JsonException ex)
{
    // Deserialization failure
}
```

## Common HTTP response codes

| Code | Meaning              | Typical use                          |
|------|----------------------|--------------------------------------|
| 200  | OK                   | Successful GET or PUT                |
| 201  | Created              | Successful POST (resource created)   |
| 204  | No Content           | Successful DELETE or PUT (no body)   |
| 400  | Bad Request          | Validation error in request body     |
| 404  | Not Found            | Resource does not exist              |
| 409  | Conflict             | Duplicate or state conflict          |

## Platform-specific: local development with HTTP clear-text

Emulators and simulators block clear-text HTTP by default. When targeting a local dev server over `http://`:

**Android** — add a network security config in `Platforms/Android/Resources/xml/network_security_config.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
  <domain-config cleartextTrafficPermitted="true">
    <domain includeSubdomains="true">10.0.2.2</domain>
  </domain-config>
</network-security-config>
```

Reference it in `AndroidManifest.xml`:

```xml
<application android:networkSecurityConfig="@xml/network_security_config" ... />
```

**iOS / Mac Catalyst** — add an `NSAppTransportSecurity` exception in `Info.plist`:

```xml
<key>NSAppTransportSecurity</key>
<dict>
  <key>NSAllowsLocalNetworking</key>
  <true/>
</dict>
```

> **Note:** Android emulators reach the host machine at `10.0.2.2`. iOS simulators use `localhost` directly.

## Rules

- Always use `async/await`; never block with `.Result` or `.Wait()`.
- Register `HttpClient` once (singleton or factory); do not create per-request instances.
- Set `BaseAddress` in DI; use relative URIs in service methods.
- Apply `JsonSerializerOptions` consistently with `CamelCase` naming policy.
- Check `IsSuccessStatusCode` before deserializing response bodies.
- Wrap API calls in try/catch for `HttpRequestException` and `JsonException`.
- Use the service interface pattern so view models depend on abstractions.
