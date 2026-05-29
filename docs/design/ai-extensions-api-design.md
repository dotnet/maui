# DI Extension Methods API Design Proposal

## Overview
This document proposes API designs for registering AI services in .NET MAUI applications using dependency injection. The goal is to make it trivial to add local, on-device AI capabilities while maintaining flexibility for advanced scenarios.

## Goals
- Make it trivial to add AI to a MAUI app (ideally 1-2 lines in `MauiProgram.cs`)
- Support progressive disclosure (simple by default, powerful when needed)
- Follow .NET conventions and patterns developers already know
- Work with both Microsoft DI and third-party containers
- Enable platform-agnostic code while allowing platform-specific overrides

## API Design Options

### Option 1: Simple Auto-Detection

#### API
```csharp
// Automatically registers the platform-specific client
services.AddAIChatClient();

// With options
services.AddAIChatClient(options => {
    options.DefaultTemperature = 0.7;
    options.EnableStreaming = true;
});

// All services
services.AddAI();
```

#### Example Usage
```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services.AddAIChatClient();
    
    return builder.Build();
}

// ViewModel
public class ChatViewModel
{
    private readonly IChatClient _chatClient;
    
    public ChatViewModel(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    
    public async Task SendMessage(string text)
    {
        var response = await _chatClient.CompleteAsync(text);
        return response.Message.Text;
    }
}
```

#### Pros
- Simplest possible API - just works
- Minimal cognitive load for developers
- Matches patterns from `AddHttpClient()`, `AddDbContext()`

#### Cons
- "Magic" platform detection could be confusing
- Less control for advanced scenarios
- Harder to test with mocks

---

### Option 2: Explicit Platform Registration

#### API
```csharp
// Explicit per platform
services.AddAppleIntelligenceChatClient();
services.AddMLKitChatClient();
services.AddWindowsAIChatClient();

// Platform-agnostic wrapper
services.AddPlatformChatClient();
```

#### Example Usage
```csharp
// MauiProgram.cs - Explicit
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    #if IOS || MACCATALYST
    builder.Services.AddAppleIntelligenceChatClient();
    #elif ANDROID
    builder.Services.AddMLKitChatClient();
    #elif WINDOWS
    builder.Services.AddWindowsAIChatClient();
    #endif
    
    return builder.Build();
}

// MauiProgram.cs - Platform-agnostic
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services.AddPlatformChatClient();
    
    return builder.Build();
}
```

#### Pros
- Clear and explicit - no magic
- Easy to test with platform-specific mocks
- Great for debugging and understanding

#### Cons
- More verbose for common case
- Requires platform `#if` directives in user code
- Multiple ways to do the same thing

---

### Option 3: Builder Pattern

#### API
```csharp
services.AddMauiAI(ai => {
    ai.AddChatClient();
    ai.AddEmbeddingGenerator();
    ai.ConfigureChatOptions(opts => {
        opts.Temperature = 0.7;
    });
});
```

#### Example Usage
```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services.AddMauiAI(ai => {
        ai.AddChatClient()
          .WithStreaming()
          .WithMultimodal();
          
        ai.AddEmbeddingGenerator()
          .WithCaching(1000);
    });
    
    return builder.Build();
}
```

#### Pros
- Fluent and discoverable
- Groups related configuration
- Extensible for future services

#### Cons
- More complex API surface
- Need to maintain builder interface
- Could be overkill for simple scenarios

---

### Option 4: Chainable Service Collection

#### API
```csharp
services.AddMauiAI()
    .ConfigureChatClient(opts => opts.EnableStreaming = true)
    .ConfigureEmbeddings(opts => opts.CacheResults = true);
```

#### Example Usage
```csharp
// MauiProgram.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services
        .AddMauiAI()
        .ConfigureChatClient(opts => {
            opts.DefaultTemperature = 0.7;
            opts.EnableStreaming = true;
        });
    
    return builder.Build();
}
```

#### Pros
- Clean chaining pattern
- Familiar from Entity Framework, Identity
- Good separation of concerns

#### Cons
- Requires returning custom interface
- Adds complexity to implementation

---

### Option 5: Multiple Entry Points

#### API
```csharp
// Everything (chat + embeddings + future services)
services.AddAI();

// Just chat
services.AddAIChatClient();

// Just embeddings
services.AddAIEmbeddingGenerator();

// With options
services.AddAI(options => {
    options.IncludeChatClient = true;
    options.IncludeEmbeddingGenerator = false;
    options.Chat.DefaultTemperature = 0.7;
});
```

#### Example Usage
```csharp
// MauiProgram.cs - Simple (recommended for most apps)
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    // One line - gets you chat + embeddings
    builder.Services.AddAI();
    
    return builder.Build();
}

// MauiProgram.cs - Selective (just chat)
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    // Just what you need
    builder.Services.AddAIChatClient();
    
    return builder.Build();
}

// MauiProgram.cs - Configured
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services.AddAI(options => {
        options.IncludeChatClient = true;
        options.IncludeEmbeddingGenerator = false;
        options.Chat.DefaultTemperature = 0.7;
        options.Chat.EnableStreaming = true;
    });
    
    return builder.Build();
}

// Debug vs Release (local AI for dev, cloud for prod)
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    #if DEBUG
    // Use free local AI for dev/test
    builder.Services.AddAIChatClient();
    #else
    // Use Azure OpenAI for production
    builder.Services.AddSingleton<IChatClient>(sp => 
        new AzureOpenAIChatClient(endpoint, apiKey));
    #endif
    
    return builder.Build();
}
```

#### Pros
- Progressive disclosure - simple cases are simple
- Flexible - pick what you need
- Matches .NET ecosystem patterns
- Easy to understand intent from method name

#### Cons
- Multiple ways to do same thing (could be confusing)
- Need to document when to use which method

---

## Design Questions

### 1. Platform Detection
**How should extension methods detect the current platform?**

**Option A: Compile-time (multi-targeting)**
```csharp
#if IOS || MACCATALYST
services.AddSingleton<IChatClient, AppleIntelligenceChatClient>();
#elif ANDROID
services.AddSingleton<IChatClient, MLKitChatClient>();
#elif WINDOWS
services.AddSingleton<IChatClient, WindowsAIChatClient>();
#endif
```
- Pro: Type-safe, compile-time checked
- Con: Visible in user code (but inside extension method, not user code)

**Option B: Runtime (DeviceInfo.Platform)**
```csharp
if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
    services.AddSingleton<IChatClient, AppleIntelligenceChatClient>();
else if (DeviceInfo.Platform == DevicePlatform.Android)
    services.AddSingleton<IChatClient, MLKitChatClient>();
// etc.
```
- Pro: Hidden from user, cleaner API
- Con: Runtime detection, potential for errors

**Recommendation:** Option A (compile-time) - safer and more predictable

### 2. Service Lifetime
**Should AI services be Singleton, Scoped, or Transient?**

**Recommendation: Singleton**
- `IChatClient` is stateless
- Can be safely shared across app
- Matches Azure OpenAI client pattern
- Better performance (no repeated instantiation)

```csharp
services.AddSingleton<IChatClient, AppleIntelligenceChatClient>();
```

### 3. Multiple Registrations
**What happens if user calls `AddAI()` twice?**

**Recommendation: Last one wins** (standard DI behavior)
```csharp
services.AddAI(); // Registers platform client
services.AddSingleton<IChatClient>(new MyCustomClient()); // Replaces it
```

This is consistent with how other .NET DI extensions work.

### 4. Configuration Options Structure
**How should options be structured?**

```csharp
public class MauiAIOptions
{
    // What to include
    public bool IncludeChatClient { get; set; } = true;
    public bool IncludeEmbeddingGenerator { get; set; } = true;
    
    // Chat-specific options
    public ChatClientOptions Chat { get; set; } = new();
    
    // Embedding-specific options
    public EmbeddingGeneratorOptions Embeddings { get; set; } = new();
}

public class ChatClientOptions
{
    public double DefaultTemperature { get; set; } = 1.0;
    public bool EnableStreaming { get; set; } = true;
    public int MaxTokens { get; set; } = 2048;
}

public class EmbeddingGeneratorOptions
{
    public bool CacheResults { get; set; } = true;
    public int CacheSize { get; set; } = 1000;
}
```

## Usage Scenarios

### Scenario 1: Simplest Possible
```csharp
// MauiProgram.cs
builder.Services.AddAI();

// ViewModel
public ChatViewModel(IChatClient chat) { ... }
```

### Scenario 2: Just Chat (No Embeddings)
```csharp
builder.Services.AddAIChatClient();
```

### Scenario 3: Custom Configuration
```csharp
builder.Services.AddAI(options => {
    options.Chat.DefaultTemperature = 0.7;
    options.Chat.MaxTokens = 4096;
});
```

### Scenario 4: Dev vs Prod (Local vs Cloud)
```csharp
#if DEBUG
builder.Services.AddAIChatClient(); // Free local AI
#else
builder.Services.AddSingleton<IChatClient>(new AzureOpenAIChatClient(...)); // Cloud
#endif
```

### Scenario 5: Testing with Mocks
```csharp
// In test project
var services = new ServiceCollection();
services.AddSingleton<IChatClient>(mockChatClient);
var provider = services.BuildServiceProvider();
var viewModel = new ChatViewModel(provider.GetRequiredService<IChatClient>());
```

### Scenario 6: Hybrid (Local + Cloud Fallback)
```csharp
// Register local client
builder.Services.AddAIChatClient();

// Wrap with fallback client (future work)
builder.Services.Decorate<IChatClient>((inner, sp) => 
    new OfflineFallbackChatClient(
        cloudClient: new AzureOpenAIChatClient(...),
        localClient: inner,
        connectivity: sp.GetRequiredService<IConnectivity>()));
```

## Open Questions for Discussion

1. **Which API style should we use?**
   - Recommendation: Option 5 (Multiple Entry Points)
   
2. **Should platform detection be compile-time or runtime?**
   - Recommendation: Compile-time for type safety

3. **Should we expose platform-specific methods (e.g., `AddAppleIntelligenceChatClient()`)?**
   - Could be useful for testing scenarios
   - Adds API surface area

4. **What's the package structure?**
   - Core extensions in `Microsoft.Maui.AI`
   - Implementations in platform packages (`Microsoft.Maui.AI.iOS`, etc.)

5. **Should we validate options at registration time or first use?**
   - Registration time = fail fast
   - First use = more flexible but harder to debug

6. **How do we handle forward compatibility when new abstractions are added?**
   - `AddAI()` should automatically include new services
   - Provide opt-out via options

7. **Should we support named clients (similar to `AddHttpClient("name")`)?**
   - Useful if app needs multiple configurations
   - Adds complexity

## Recommendation

**Use Option 5: Multiple Entry Points** because:
- ✅ Matches .NET ecosystem patterns (`AddHttpClient`, `AddDbContext`)
- ✅ Simple cases are one line: `services.AddAI()`
- ✅ Advanced cases are clear: `services.AddAI(options => ...)`
- ✅ Selective cases are explicit: `services.AddAIChatClient()`
- ✅ Easy to document and understand
- ✅ Supports progressive disclosure

**Implementation details:**
- Use compile-time platform detection (Option A)
- Default to Singleton lifetime
- Package core extensions in `Microsoft.Maui.AI`
- Validate options at registration time where possible

## Next Steps

1. Get team feedback on API style preference
2. Finalize options class structure
3. Decide on platform detection approach
4. Create implementation issue with tasks
5. Write API documentation
6. Create usage samples
