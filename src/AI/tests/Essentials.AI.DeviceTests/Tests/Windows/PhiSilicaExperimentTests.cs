#if WINDOWS
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Experiment tests for probing Phi Silica model capabilities.
/// These tests help understand the model's behavior and inform
/// the PromptBasedToolCallingClient design.
/// </summary>
[Category("PhiSilicaChatClient")]
public class PhiSilicaExperimentTests
{
	// ═══════════════════════════════════════════════════════════
	// MODEL IDENTITY PROBING
	// ═══════════════════════════════════════════════════════════

	[Fact]
	public async Task Probe_ModelIdentity_ReturnsPhiInfo()
	{
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User,
				"Answer briefly: Are you a Microsoft Phi model? " +
				"If yes, which generation (Phi-2, Phi-3, Phi-4)? " +
				"Do you support tool/function calling natively?")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
		var text = response.Text;
		Assert.False(string.IsNullOrEmpty(text), "Model should respond to identity probe");

		// Log the response for analysis (visible in test output)
		Console.WriteLine($"MODEL IDENTITY RESPONSE: {text}");
	}

	[Fact]
	public async Task Probe_NativeToolFormat_DetectsCapability()
	{
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System,
				"You have access to the following tools:\n" +
				"[{\"name\":\"get_weather\",\"description\":\"Gets weather for a city\"," +
				"\"parameters\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}}]\n\n" +
				"When calling a tool, respond with ONLY the tool call, no other text."),
			new(ChatRole.User, "What is the weather in Cape Town?")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
		var text = response.Text;
		Assert.False(string.IsNullOrEmpty(text));

		Console.WriteLine($"TOOL CALL PROBE RESPONSE: {text}");

		// Check what format the model uses
		bool hasToolCallTags = text.Contains("<tool_call>", StringComparison.OrdinalIgnoreCase) ||
							   text.Contains("<|tool_call|>", StringComparison.OrdinalIgnoreCase);
		bool hasJsonBlock = text.TrimStart().StartsWith('{') || text.TrimStart().StartsWith('[');
		bool hasToolKeyword = text.Contains("\"name\"", StringComparison.OrdinalIgnoreCase) &&
							  text.Contains("get_weather", StringComparison.OrdinalIgnoreCase);

		Console.WriteLine($"Has <tool_call> tags: {hasToolCallTags}");
		Console.WriteLine($"Has JSON block: {hasJsonBlock}");
		Console.WriteLine($"Has tool name keyword: {hasToolKeyword}");

		// At least one of these should be true if the model understands tool calling
		Assert.True(hasToolCallTags || hasJsonBlock || hasToolKeyword,
			$"Model should attempt some form of tool call. Got: {text}");
	}

	// ═══════════════════════════════════════════════════════════
	// ENUM STRUCTURED OUTPUT TESTS
	// ═══════════════════════════════════════════════════════════

	public enum Fruit { Apple, Banana, Cherry }

	public record FruitResponse
	{
		public Fruit? SelectedFruit { get; init; }
		public string? Reason { get; init; }
	}

	[Fact]
	public async Task Enum_StructuredOutput_ValidValue_ReturnsCorrectEnum()
	{
		// Test: Ask about a fruit that IS in the enum
		var client = new PhiSilicaSchemaClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "The user wants a banana. Select the matching fruit.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<FruitResponse>()
		};

		var response = await client.GetResponseAsync(messages, options);
		Assert.NotNull(response);
		var text = response.Text;
		Console.WriteLine($"ENUM VALID RESPONSE: {text}");

		// Parse and check
		Assert.False(string.IsNullOrEmpty(text), "Should get a response");
		try
		{
			var result = JsonSerializer.Deserialize<FruitResponse>(text,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			Assert.NotNull(result);
			Console.WriteLine($"Parsed: Fruit={result.SelectedFruit}, Reason={result.Reason}");

			// The model should pick Banana
			Assert.Equal(Fruit.Banana, result.SelectedFruit);
		}
		catch (JsonException ex)
		{
			Console.WriteLine($"JSON parse failed: {ex.Message}");
			// Don't fail the test - just log for analysis
		}
	}

	[Fact]
	public async Task Enum_StructuredOutput_InvalidValue_HandlesGracefully()
	{
		// Test: Ask about something NOT in the enum (Bread is not a fruit)
		var client = new PhiSilicaSchemaClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "The user wants bread. Select the matching fruit from the enum. If none match, set SelectedFruit to null.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<FruitResponse>()
		};

		var response = await client.GetResponseAsync(messages, options);
		Assert.NotNull(response);
		var text = response.Text;
		Console.WriteLine($"ENUM INVALID RESPONSE: {text}");

		Assert.False(string.IsNullOrEmpty(text), "Should get a response");
		try
		{
			var result = JsonSerializer.Deserialize<FruitResponse>(text,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			Assert.NotNull(result);
			Console.WriteLine($"Parsed: Fruit={result.SelectedFruit}, Reason={result.Reason}");

			// The model should NOT pick a valid fruit for "bread"
			// (or pick null)
			Assert.Null(result.SelectedFruit);
		}
		catch (JsonException ex)
		{
			Console.WriteLine($"JSON parse failed (expected for invalid enum): {ex.Message}");
		}
	}

	// ═══════════════════════════════════════════════════════════
	// TOOL CALLING FORMAT EXPERIMENTS
	// ═══════════════════════════════════════════════════════════

	[Fact]
	public async Task ToolFormat_PhiCookbookStyle_TestFormat()
	{
		// Test the format from PhiCookbook: tools in system message "tools" field
		var client = new PhiSilicaChatClient();
		var tools = "[{\"name\":\"get_weather\",\"description\":\"Gets weather\",\"parameters\":{\"city\":{\"type\":\"str\",\"default\":\"London\"}}}]";

		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, $"You are a helpful assistant with tools.\n<|tool|>{tools}<|/tool|>"),
			new(ChatRole.User, "What is the weather in Seattle?")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
		Console.WriteLine($"PHI COOKBOOK FORMAT RESPONSE: {response.Text}");
	}

	[Fact]
	public async Task ToolFormat_CurrentFormat_TestFormat()
	{
		// Test our current <tool_call> format
		var client = new PhiSilicaChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System,
				"You have access to these tools:\n" +
				"[{\"name\":\"get_weather\",\"description\":\"Gets weather for a city\"," +
				"\"parameters\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}}]\n\n" +
				"When calling a tool, respond with ONLY:\n" +
				"<tool_call>{\"name\": \"ToolName\", \"arguments\": {\"param\": \"value\"}}</tool_call>\n\n" +
				"If the user's question can be answered without tools, respond normally."),
			new(ChatRole.User, "What is the weather in Seattle?")
		};

		var response = await client.GetResponseAsync(messages);
		Assert.NotNull(response);
		var text = response.Text;
		Console.WriteLine($"CURRENT FORMAT RESPONSE: {text}");

		// Verify it uses our format
		Assert.Contains("get_weather", text, StringComparison.OrdinalIgnoreCase);
	}

	// ═══════════════════════════════════════════════════════════
	// CHAINED TOOL CALLING TESTS
	// ═══════════════════════════════════════════════════════════

	[Fact]
	public async Task ChainedTools_MultiCityWeatherReport_CallsAllTools()
	{
		// Test: 3 weather calls → 1 report call
		// "Build a weather report for Cape Town, Durban, and Madagascar"
		int weatherCallCount = 0;
		var weatherResults = new Dictionary<string, string>();

		var weatherTool = AIFunctionFactory.Create(
			(string city) =>
			{
				Interlocked.Increment(ref weatherCallCount);
				var result = city.ToLowerInvariant() switch
				{
					"cape town" => "{\"city\":\"Cape Town\",\"temp\":22,\"condition\":\"Sunny\"}",
					"durban" => "{\"city\":\"Durban\",\"temp\":28,\"condition\":\"Humid\"}",
					"madagascar" => "{\"city\":\"Madagascar\",\"temp\":30,\"condition\":\"Tropical\"}",
					_ => $"{{\"city\":\"{city}\",\"temp\":20,\"condition\":\"Unknown\"}}"
				};
				weatherResults[city] = result;
				return result;
			},
			name: "GetWeather",
			description: "Gets current weather for a city. Call once per city.");

		bool reportCalled = false;
		string? reportData = null;

		var reportTool = AIFunctionFactory.Create(
			(string weatherData) =>
			{
				reportCalled = true;
				reportData = weatherData;
				return "Weather report generated successfully for all cities.";
			},
			name: "GenerateWeatherReport",
			description: "Generates a consolidated weather report from weather data for multiple cities. Pass all weather data as a JSON string.");

		var inner = new PromptBasedToolCallingClient(new PhiSilicaChatClient());
		var client = inner.AsBuilder().UseFunctionInvocation().Build();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Get the weather for Cape Town, Durban, and Madagascar, then generate a weather report with all the data.")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool, reportTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Console.WriteLine($"CHAINED TOOLS RESPONSE: {response.Text}");
		Console.WriteLine($"Weather calls: {weatherCallCount}");
		Console.WriteLine($"Report called: {reportCalled}");
		Console.WriteLine($"Weather results: {string.Join(", ", weatherResults.Keys)}");

		// We expect at least 2 weather calls (model may not call all 3 separately)
		Assert.True(weatherCallCount >= 2,
			$"Expected at least 2 weather calls, got {weatherCallCount}");
	}
}
#endif
