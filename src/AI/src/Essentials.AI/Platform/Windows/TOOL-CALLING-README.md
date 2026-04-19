# Prompt-Based Tool Calling for Phi Silica

## Background

The Windows Copilot Runtime exposes **Phi Silica** — Microsoft's NPU-optimized small language model — through the `Microsoft.Windows.AI.Text.LanguageModel` WinRT API. This API is text-in/text-out: you send a string prompt and receive a string response (with optional streaming via `Progress` callbacks). There is **no native tool/function calling API** at the WinRT level.

However, the underlying model (**Phi-4-mini-instruct**) _does_ support tool calling at the model level. The Hugging Face tokenizer config reveals dedicated special tokens:

| Token | Purpose |
|-------|---------|
| `<\|tool\|>` / `<\|/tool\|>` | Wrap tool definitions in system messages |
| `<\|tool_call\|>` / `<\|/tool_call\|>` | Model emits these when making a tool call |
| `<\|tool_response\|>` | Tool results fed back to the model |

The official Phi-4-mini-instruct [model card](https://huggingface.co/microsoft/Phi-4-mini-instruct) documents this format:

```
<|system|>You are a helpful assistant.<|tool|>[{"name":"get_weather","description":"...","parameters":{...}}]<|/tool|><|end|>
<|user|>What's the weather in Paris?<|end|>
<|assistant|><|tool_call|>{"name":"get_weather","arguments":{"city":"Paris"}}<|/tool_call|><|end|>
```

## The Problem

The Windows `LanguageModel` API **strips special tokens** from the output text. When the model internally generates `<|tool_call|>{"name":"GetWeather",...}<|/tool_call|>`, the API delivers the text _without_ the pipe-delimited tokens. Through experimentation, we discovered that the model outputs `<tool_call>` (plain XML-style tags without pipes) in its text response when prompted correctly.

Additionally, the `Microsoft.Extensions.AI` framework uses `FunctionInvokingChatClient` (FICC) as a middleware that:
1. Sends a request to the inner `IChatClient`
2. Checks the response for `FunctionCallContent` objects
3. If found, invokes the corresponding function and loops back with the result
4. If `InformationalOnly == true` on a `FunctionCallContent`, FICC skips it (the server already handled it)

Since Phi Silica returns plain text (not structured `FunctionCallContent`), we need a translation layer.

## Solution: PromptBasedToolCallingClient

`PromptBasedToolCallingClient` is a `DelegatingChatClient` that sits between FICC and `PhiSilicaChatClient`:

```
User Code
  ↓
FunctionInvokingChatClient (FICC)  — detects FunctionCallContent, invokes functions, loops
  ↓
PromptBasedToolCallingClient       — injects tool prompt, parses <tool_call> from text
  ↓
PhiSilicaChatClient                — sends to Phi Silica LanguageModel API
```

### What It Does

1. **Intercepts requests with tools** — When `ChatOptions.Tools` contains `AIFunction` tools, rewrites the request
2. **Injects tool definitions** into a system prompt describing available tools and the expected JSON format
3. **Nulls out `options.Tools`** so the inner `PhiSilicaChatClient` doesn't see them (it doesn't support tools natively)
4. **Parses model output** — Scans the assembled text for `<tool_call>{...}</tool_call>` patterns
5. **Converts to `FunctionCallContent`** — Replaces text tool calls with proper M.E.AI content types so FICC can invoke them

### Prompt Format

The system prompt injected by `PromptBasedToolCallingClient`:

```
You are a helpful assistant with access to the following tools:

[{"name":"GetWeather","description":"Gets the weather","parameters":{...}}]

When the user asks a question that requires using a tool, you MUST respond with ONLY
a tool call in this EXACT format (no other text):

<tool_call>{"name": "ToolName", "arguments": {"param": "value"}}</tool_call>

Rules:
- Respond with ONLY the <tool_call> block when calling a tool, no other text.
- Use the exact function name and parameter names from the tool definitions.
- The arguments must be valid JSON matching the parameter schema.
- After receiving a tool result, use it to formulate your final response.
- If the user's question can be answered without tools, respond normally.
```

## Key Issues Discovered & Solved

### Issue 1: Special tokens stripped by WinRT API

**Discovery**: The Phi-4 tokenizer has `<|tool_call|>` as a special token, but the Windows `LanguageModel` API strips these from output text.

**Solution**: Use plain XML-style `<tool_call>` tags (without pipes) in the prompt. The model reliably reproduces these in its output since they're treated as regular text, not special tokens.

### Issue 2: Streaming fragmentation

**Discovery**: During streaming, the model sends text in small chunks. A tool call like `<tool_call>{"name":"GetWeather"}` arrives as:
```
[0] Text: <
[1] Text: tool_call>
[2] Text: {"name
[3] Text: ": "GetWeather",
[4] Text: "arguments": {"location": "Seattle"}}
[5] Text: </tool_call>
```

**Solution**: The streaming path buffers all `ChatResponseUpdate` chunks and assembles the full text before attempting to parse tool calls. If tool calls are found, a single update with `FunctionCallContent` is yielded instead of the text fragments.

### Issue 3: options.Tools nulled before check

**Discovery**: `RewriteIfNeeded()` nulls out `options.Tools` (so the inner client doesn't see them), but the code checked `options.Tools` _after_ rewriting to decide whether to parse tool calls. This meant `ParseToolCallsFromResponse` was **never called**.

**Solution**: Capture `bool hadTools = options?.Tools is { Count: > 0 }` _before_ calling `RewriteIfNeeded()`, then use `hadTools` for the parsing decision.

### Issue 4: JSON with nested braces

**Discovery**: The regex `\{.+?\}` (non-greedy) matched the first `}` it found, truncating JSON like `{"name":"fn","arguments":{"x":1}}` at the inner closing brace.

**Solution**: Use greedy matching in the regex, plus a balanced-brace JSON extractor as fallback that correctly handles nested objects, strings with escaped characters, etc.

### Issue 5: FunctionCallContent/FunctionResultContent in conversation history

**Discovery**: Multi-turn conversations with tool calling include `FunctionCallContent` and `FunctionResultContent` in the message history. `PhiSilicaChatClient.ConvertToPrompt()` threw `ArgumentException` on these types.

**Solution**: Convert tool-related content to text representations using Phi-4 token format:
- `FunctionCallContent` → `<|tool_call|>{"name":"...","arguments":{...}}<|/tool_call|>`
- `FunctionResultContent` → `<|tool_response|>result_text<|end|>`

### Issue 6: AOT/Trimming compatibility

**Discovery**: The `Essentials.AI` library is marked `IsAotCompatible`. Using `JsonSerializer.Serialize<T>()` triggers IL2026/IL3050 errors.

**Solution**: Suppress warnings with `#pragma warning disable IL3050, IL2026` around JSON serialization calls, matching the pattern used in `AppleIntelligenceChatClient`.

### Issue 7: Unpackaged apps can't access Phi Silica

**Discovery**: Running the device test app as an unpackaged exe (`WindowsPackageType=None`) results in `UnauthorizedAccessException` because the `systemAIModels` restricted capability in `Package.appxmanifest` only applies to packaged (MSIX) apps.

**Solution**: Run tests as a packaged MSIX app by:
1. Setting `SelfContained=true` and `WindowsAppSDKSelfContained=true` in the csproj to bundle all dependencies
2. Registering the package via `Add-AppxPackage -Register AppxManifest.xml`
3. Launching via `Start-Process "shell:AppsFolder\$pfn!App" -ArgumentList "resultsFile"`

## Test Results

Starting point: **1/95 tests passing** (only `SmokeTests.TestInfrastructureWorks`)

Final results: **87/95 tests passing** including **22/27 function calling tests**

| Suite | Score |
|-------|-------|
| CancellationTests | 8/8 ✅ |
| OptionsTests | 8/8 ✅ |
| GetServiceTests | 5/5 ✅ |
| InstantiationTests | 4/4 ✅ |
| StreamingTests | 5/5 ✅ |
| ResponseTests | 1/1 ✅ |
| MessagesTests | 12/12 ✅ |
| ValidationTests | 7/7 ✅ |
| SmokeTests | 1/1 ✅ |
| FunctionCallingTests | 22/27 ⚠️ |
| JsonSchemaTests | 14/17 ⚠️ |

## References

- [Phi-4-mini-instruct model card](https://huggingface.co/microsoft/Phi-4-mini-instruct) — tool calling format documentation
- [Phi-4-mini-instruct tokenizer_config.json](https://huggingface.co/microsoft/Phi-4-mini-instruct/raw/main/tokenizer_config.json) — special token definitions
- [Microsoft.Extensions.AI FunctionInvokingChatClient](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.functioninvokingchatclient) — how FICC detects and invokes tool calls
- [Phi Silica documentation](https://learn.microsoft.com/windows/ai/apis/phi-silica) — Windows AI API overview
- [LanguageModel API reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.windows.ai.text.languagemodel) — WinRT API details
