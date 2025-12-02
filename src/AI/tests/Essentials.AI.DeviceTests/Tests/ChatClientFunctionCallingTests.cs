using Microsoft.Extensions.AI;
using Xunit;

#if IOS || MACCATALYST
using PlatformChatClient = Microsoft.Maui.Essentials.AI.AppleIntelligenceChatClient;
#elif ANDROID
using PlatformChatClient = Microsoft.Maui.Essentials.AI.MLKitGenAIChatClient;
#elif WINDOWS
using PlatformChatClient = Microsoft.Maui.Essentials.AI.WindowsAIChatClient;
#endif

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class ChatClientFunctionCallingTests
{
	[Fact]
	public async Task GetResponseAsync_CallsFunctionAndReturnsResult()
	{
		bool functionWasCalled = false;
		string? capturedLocation = null;

		var weatherTool = AIFunctionFactory.Create(
			(string location) =>
			{
				functionWasCalled = true;
				capturedLocation = location;
				return $"Sunny, 72°F in {location}";
			},
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.True(functionWasCalled, "Function should have been called");
		Assert.NotNull(capturedLocation);
		Assert.Contains("Seattle", capturedLocation, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetResponseAsync_HandlesMultipleFunctionCalls()
	{
		int weatherCallCount = 0;
		int timeCallCount = 0;

		var weatherTool = AIFunctionFactory.Create(
			(string location) =>
			{
				weatherCallCount++;
				return $"Sunny, 72°F in {location}";
			},
			name: "GetWeather",
			description: "Gets the weather for a location");

		var timeTool = AIFunctionFactory.Create(
			(string timezone) =>
			{
				timeCallCount++;
				return $"Current time in {timezone} is 10:30 AM";
			},
			name: "GetTime",
			description: "Gets the current time for a timezone");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle and what time is it in PST?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool, timeTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.True(weatherCallCount > 0 || timeCallCount > 0, "At least one function should have been called");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_CallsFunctionAndStreamsUpdates()
	{
		bool functionWasCalled = false;
		string? capturedLocation = null;

		var weatherTool = AIFunctionFactory.Create(
			(string location) =>
			{
				functionWasCalled = true;
				capturedLocation = location;
				return $"Sunny, 72°F in {location}";
			},
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Boston?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		bool receivedAnyUpdate = false;
		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			receivedAnyUpdate = true;
			Assert.NotNull(update);
		}

		Assert.True(receivedAnyUpdate, "Should receive at least one streaming update");
		Assert.True(functionWasCalled, "Function should have been called during streaming");
		Assert.NotNull(capturedLocation);
		Assert.Contains("Boston", capturedLocation, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_StreamsToolCallContent()
	{
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Chicago?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		bool foundToolCallContent = false;
		var updates = new List<ChatResponseUpdate>();

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			updates.Add(update);

			if (update.Contents.Any(c => c is FunctionCallContent))
			{
				foundToolCallContent = true;
			}
		}

		Assert.True(updates.Count > 0, "Should receive streaming updates");
		Assert.True(foundToolCallContent, "Should receive at least one update with FunctionCallContent");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_StreamsToolResultContent()
	{
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Denver?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		bool foundToolResultContent = false;
		var updates = new List<ChatResponseUpdate>();

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			updates.Add(update);

			if (update.Contents.Any(c => c is FunctionResultContent))
			{
				foundToolResultContent = true;
			}
		}

		Assert.True(updates.Count > 0, "Should receive streaming updates");
		Assert.True(foundToolResultContent, "Should receive at least one update with FunctionResultContent");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_StreamsToolCallBeforeToolResult()
	{
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Miami?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		int? toolCallIndex = null;
		int? toolResultIndex = null;
		int currentIndex = 0;

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			if (update.Contents.Any(c => c is FunctionCallContent) && toolCallIndex == null)
			{
				toolCallIndex = currentIndex;
			}

			if (update.Contents.Any(c => c is FunctionResultContent) && toolResultIndex == null)
			{
				toolResultIndex = currentIndex;
			}

			currentIndex++;
		}

		if (toolCallIndex.HasValue && toolResultIndex.HasValue)
		{
			Assert.True(toolCallIndex < toolResultIndex,
				"FunctionCallContent should be streamed before FunctionResultContent");
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_HandlesMultipleFunctionCalls()
	{
		int weatherCallCount = 0;
		int timeCallCount = 0;

		var weatherTool = AIFunctionFactory.Create(
			(string location) =>
			{
				weatherCallCount++;
				return $"Sunny, 72°F in {location}";
			},
			name: "GetWeather",
			description: "Gets the weather for a location");

		var timeTool = AIFunctionFactory.Create(
			(string timezone) =>
			{
				timeCallCount++;
				return $"Current time in {timezone} is 10:30 AM";
			},
			name: "GetTime",
			description: "Gets the current time for a timezone");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in New York and what time is it in EST?")
		};
		var options = new ChatOptions
		{
			Tools = [weatherTool, timeTool]
		};

		bool receivedUpdates = false;
		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			receivedUpdates = true;
		}

		Assert.True(receivedUpdates, "Should receive streaming updates");
		Assert.True(weatherCallCount > 0 || timeCallCount > 0, "At least one function should have been called");
	}

	[Fact]
	public async Task GetResponseAsync_FunctionWithComplexParameters()
	{
		bool functionWasCalled = false;

		var searchTool = AIFunctionFactory.Create(
			(string query, int maxResults, bool includeImages) =>
			{
				functionWasCalled = true;
				return $"Found {maxResults} results for '{query}' (images: {includeImages})";
			},
			name: "Search",
			description: "Searches for information");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Search for 'dotnet maui' with 10 results including images")
		};
		var options = new ChatOptions
		{
			Tools = [searchTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.True(functionWasCalled, "Function with complex parameters should be called");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_FunctionWithComplexParameters()
	{
		bool functionWasCalled = false;

		var searchTool = AIFunctionFactory.Create(
			(string query, int maxResults, bool includeImages) =>
			{
				functionWasCalled = true;
				return $"Found {maxResults} results for '{query}' (images: {includeImages})";
			},
			name: "Search",
			description: "Searches for information");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Search for 'xamarin forms' with 5 results without images")
		};
		var options = new ChatOptions
		{
			Tools = [searchTool]
		};

		bool receivedUpdates = false;
		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			receivedUpdates = true;
		}

		Assert.True(receivedUpdates, "Should receive streaming updates");
		Assert.True(functionWasCalled, "Function with complex parameters should be called during streaming");
	}

	[Fact]
	public async Task GetResponseAsync_ChainedFunctionCalls_TimeAndWeather()
	{
		int timeCallCount = 0;
		int weatherCallCount = 0;
		string? capturedDate = null;

		var timeTool = AIFunctionFactory.Create(
			() =>
			{
				timeCallCount++;
				return "2025-12-02 12:00:00";
			},
			name: "GetCurrentTime",
			description: "Gets the current date and time. No parameters needed.");

		var weatherTool = AIFunctionFactory.Create(
			(string date) =>
			{
				weatherCallCount++;
				capturedDate = date;
				return $"{{\"date\":\"{date}\",\"condition\":\"sunny\",\"temperature\":72,\"humidity\":45}}";
			},
			name: "GetWeather",
			description: "Gets the weather forecast for a specific date. Requires the date in YYYY-MM-DD format.");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather like today?")
		};
		var options = new ChatOptions
		{
			Tools = [timeTool, weatherTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		
		// Verify both functions were called
		Assert.True(timeCallCount > 0, "GetCurrentTime should have been called");
		Assert.True(weatherCallCount > 0, "GetWeather should have been called");
		
		// Verify the date was passed from time to weather
		Assert.NotNull(capturedDate);
		Assert.Contains("2025-12-02", capturedDate, StringComparison.OrdinalIgnoreCase);
		
		// Verify transcript entries contain all the messages
		Assert.NotNull(response.Messages);
		Assert.True(response.Messages.Count >= 5, 
			$"Expected at least 5 messages (user, function call, tool result, function call, tool result, assistant), but got {response.Messages.Count}");
		
		// Verify we have function call content
		bool hasFunctionCall = response.Messages
			.Any(m => m.Contents.Any(c => c is FunctionCallContent));
		Assert.True(hasFunctionCall, "Response should contain FunctionCallContent");
		
		// Verify we have function result content
		bool hasFunctionResult = response.Messages
			.Any(m => m.Contents.Any(c => c is FunctionResultContent));
		Assert.True(hasFunctionResult, "Response should contain FunctionResultContent");
		
		// Verify the time tool result contains the static time
		var timeResults = response.Messages
			.Where(m => m.Contents.Any(c => 
				c is FunctionResultContent frc &&
				frc.Result?.ToString()?.Contains("2025-12-02 12:00:00", StringComparison.OrdinalIgnoreCase) == true))
			.ToList();
		Assert.NotEmpty(timeResults);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_ChainedFunctionCalls_TimeAndWeather()
	{
		int timeCallCount = 0;
		int weatherCallCount = 0;
		string? capturedDate = null;

		var timeTool = AIFunctionFactory.Create(
			() =>
			{
				timeCallCount++;
				return "2025-12-02 12:00:00";
			},
			name: "GetCurrentTime",
			description: "Gets the current date and time. No parameters needed.");

		var weatherTool = AIFunctionFactory.Create(
			(string date) =>
			{
				weatherCallCount++;
				capturedDate = date;
				return $"{{\"date\":\"{date}\",\"condition\":\"cloudy\",\"temperature\":68,\"humidity\":55}}";
			},
			name: "GetWeather",
			description: "Gets the weather forecast for a specific date. Requires the date in YYYY-MM-DD format.");

		var client = new PlatformChatClient();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather like today?")
		};
		var options = new ChatOptions
		{
			Tools = [timeTool, weatherTool]
		};

		bool foundTimeCall = false;
		bool foundWeatherCall = false;
		bool foundToolResult = false;
		bool foundStaticTime = false;
		var updates = new List<ChatResponseUpdate>();

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			updates.Add(update);

			foreach (var content in update.Contents)
			{
				if (content is FunctionCallContent functionCall)
				{
					if (functionCall.Name == "GetCurrentTime")
						foundTimeCall = true;
					else if (functionCall.Name == "GetWeather")
						foundWeatherCall = true;
				}
				else if (content is FunctionResultContent functionResult)
				{
					foundToolResult = true;
					if (functionResult.Result?.ToString()?.Contains("2025-12-02 12:00:00", StringComparison.OrdinalIgnoreCase) == true)
						foundStaticTime = true;
				}
			}
		}

		Assert.True(updates.Count > 0, "Should receive streaming updates");
		Assert.True(timeCallCount > 0, "GetCurrentTime should have been called");
		Assert.True(weatherCallCount > 0, "GetWeather should have been called");
		Assert.True(foundTimeCall, "Should stream FunctionCallContent for GetCurrentTime");
		Assert.True(foundWeatherCall, "Should stream FunctionCallContent for GetWeather");
		Assert.True(foundToolResult, "Should stream FunctionResultContent");
		Assert.True(foundStaticTime, "Should find the static time '2025-12-02 12:00:00' in function results");
	}
}
