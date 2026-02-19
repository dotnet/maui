using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient function calling tests.
/// Provides common tests for any IChatClient implementation.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientFunctionCallingTestsBase<T>
	where T : class, IChatClient, new()
{
	protected virtual IChatClient EnableFunctionCalling(T client) => client;

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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

		var client = EnableFunctionCalling(new T());
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

	[Fact]
	public async Task GetResponseAsync_FunctionWithEnumParameter_CallsToolCorrectly()
	{
		// This test verifies that enum parameters are properly preserved in the JSON schema
		// and that the AI can call the function with valid enum values.
		// This was a critical bug where enum constraints were being lost during schema parsing.

		bool functionWasCalled = false;
		PointOfInterestCategory? capturedCategory = null;
		string? capturedQuery = null;

		var findPointsOfInterestTool = AIFunctionFactory.Create(
			(PointOfInterestCategory pointOfInterest, string naturalLanguageQuery) =>
			{
				functionWasCalled = true;
				capturedCategory = pointOfInterest;
				capturedQuery = naturalLanguageQuery;

				// Return mock data based on category
				return pointOfInterest switch
				{
					PointOfInterestCategory.Hotel => "There are these hotel in Maui: Hotel 1, Hotel 2, Hotel 3",
					PointOfInterestCategory.Restaurant => "There are these restaurant in Maui: Restaurant 1, Restaurant 2, Restaurant 3",
					PointOfInterestCategory.Cafe => "There are these cafe in Maui: Cafe 1, Cafe 2, Cafe 3",
					_ => $"Found some {pointOfInterest} locations"
				};
			},
			name: "FindPointsOfInterest",
			description: "Finds points of interest for a landmark.");

		var client = EnableFunctionCalling(new T());
		var systemMessage = new ChatMessage(ChatRole.System,
			"Your job is to help find hotels and restaurants. " +
			"Always use the FindPointsOfInterest tool to find businesses.");

		var messages = new List<ChatMessage>
		{
			systemMessage,
			new(ChatRole.User, "Find hotels in Maui")
		};
		var options = new ChatOptions
		{
			Tools = [findPointsOfInterestTool]
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.True(functionWasCalled, "Function with enum parameter should have been called");
		Assert.NotNull(capturedCategory);
		Assert.NotNull(capturedQuery);

		// Verify the AI used a valid enum value
		Assert.True(Enum.IsDefined(typeof(PointOfInterestCategory), capturedCategory.Value),
			$"AI should use a valid enum value, but got: {capturedCategory}");

		// Verify the query is related to the user's request
		Assert.True(
			capturedQuery.Contains("hotel", StringComparison.OrdinalIgnoreCase) ||
			capturedQuery.Contains("maui", StringComparison.OrdinalIgnoreCase),
			$"The natural language query should relate to the user's request, but got: {capturedQuery}");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_FunctionWithEnumParameter_CallsToolCorrectly()
	{
		// This test verifies enum parameters work correctly in streaming scenarios

		bool functionWasCalled = false;
		PointOfInterestCategory? capturedCategory = null;
		string? capturedQuery = null;

		var findPointsOfInterestTool = AIFunctionFactory.Create(
			(PointOfInterestCategory pointOfInterest, string naturalLanguageQuery) =>
			{
				functionWasCalled = true;
				capturedCategory = pointOfInterest;
				capturedQuery = naturalLanguageQuery;

				return $"There are these {pointOfInterest} in Maui: Location 1, Location 2, Location 3";
			},
			name: "FindPointsOfInterest",
			description: "Finds points of interest for a landmark.");

		var client = EnableFunctionCalling(new T());
		var systemMessage = new ChatMessage(ChatRole.System,
			"Your job is to help find restaurants and cafes. " +
			"Always use the FindPointsOfInterest tool to find businesses.");

		var messages = new List<ChatMessage>
		{
			systemMessage,
			new(ChatRole.User, "Find restaurants in Maui")
		};
		var options = new ChatOptions
		{
			Tools = [findPointsOfInterestTool]
		};

		bool receivedUpdates = false;
		bool foundFunctionCall = false;

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			receivedUpdates = true;

			if (update.Contents.Any(c => c is FunctionCallContent))
			{
				foundFunctionCall = true;
			}
		}

		Assert.True(receivedUpdates, "Should receive streaming updates");
		Assert.True(foundFunctionCall, "Should receive FunctionCallContent in streaming updates");
		Assert.True(functionWasCalled, "Function with enum parameter should have been called during streaming");
		Assert.NotNull(capturedCategory);

		// Verify the AI used a valid enum value
		Assert.True(Enum.IsDefined(typeof(PointOfInterestCategory), capturedCategory.Value),
			$"AI should use a valid enum value, but got: {capturedCategory}");

		Assert.NotNull(capturedQuery);
		// Verify the query is related to the user's request
		Assert.True(
			capturedQuery.Contains("restaurant", StringComparison.OrdinalIgnoreCase) ||
			capturedQuery.Contains("maui", StringComparison.OrdinalIgnoreCase),
			$"The natural language query should relate to the user's request, but got: {capturedQuery}");
	}

	[Fact]
	public async Task GetResponseAsync_MultiTurnConversationWithToolCalling_SucceedsOnFollowUp()
	{
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Ask about weather (triggers tool call)
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle?")
		};

		var firstResponse = await client.GetResponseAsync(messages, options);
		Assert.NotNull(firstResponse);
		Assert.NotNull(firstResponse.Messages);
		Assert.True(firstResponse.Messages.Count > 0, "First response should have messages");

		// Verify tool calling occurred in the response
		bool hasFunctionContent = firstResponse.Messages
			.Any(m => m.Contents.Any(c => c is FunctionCallContent || c is FunctionResultContent));
		Assert.True(hasFunctionContent, "First response should contain function call/result content from tool calling");

		// Turn 2: Build conversation history with all messages from first turn, then add follow-up
		// This is the pattern that triggers the bug: FunctionCallContent/FunctionResultContent
		// in the history causes ToNative to throw "content type not supported"
		var followUpMessages = new List<ChatMessage>(firstResponse.Messages)
		{
			new(ChatRole.User, "What about in Portland?")
		};

		var secondResponse = await client.GetResponseAsync(followUpMessages, options);

		Assert.NotNull(secondResponse);
		Assert.NotNull(secondResponse.Messages);
		Assert.True(secondResponse.Messages.Count > 0, "Second response should have messages");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_MultiTurnConversationWithToolCalling_SucceedsOnFollowUp()
	{
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Ask about weather (triggers tool call)
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle?")
		};

		var firstResponse = await client.GetResponseAsync(messages, options);
		Assert.NotNull(firstResponse);
		Assert.NotNull(firstResponse.Messages);

		// Verify tool calling occurred
		bool hasFunctionContent = firstResponse.Messages
			.Any(m => m.Contents.Any(c => c is FunctionCallContent || c is FunctionResultContent));
		Assert.True(hasFunctionContent, "First response should contain function call/result content from tool calling");

		// Turn 2: Stream follow-up with full conversation history including tool call/result content
		var followUpMessages = new List<ChatMessage>(firstResponse.Messages)
		{
			new(ChatRole.User, "What about in Portland?")
		};

		bool receivedAnyUpdate = false;
		await foreach (var update in client.GetStreamingResponseAsync(followUpMessages, options))
		{
			receivedAnyUpdate = true;
			Assert.NotNull(update);
		}

		Assert.True(receivedAnyUpdate, "Should receive at least one streaming update for the follow-up");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_MultiTurnWithToolCalling_HistoryBuiltFromStreamedContent_SucceedsOnFollowUp()
	{
		// This test simulates the exact pattern ChatViewModel uses:
		// 1. Stream turn 1 → collect all AIContent from ChatResponseUpdate
		// 2. Build history by grouping content by role (FunctionCallContent → Assistant, FunctionResultContent → Tool, TextContent → Assistant)
		// 3. Send turn 2 with that manually-built history
		// This catches the bug where only TextContent was kept in history, dropping tool context.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Stream the first response and collect all content
		var turn1UserMessage = new ChatMessage(ChatRole.User, "What's the weather in Seattle?");
		var turn1Messages = new List<ChatMessage> { turn1UserMessage };

		var allContents = new List<AIContent>();
		await foreach (var update in client.GetStreamingResponseAsync(turn1Messages, options))
		{
			foreach (var content in update.Contents)
				allContents.Add(content);
		}

		// Verify we got tool content from streaming
		var functionCalls = allContents.OfType<FunctionCallContent>().ToArray();
		var functionResults = allContents.OfType<FunctionResultContent>().ToArray();
		var textContents = allContents.OfType<TextContent>().Where(t => !string.IsNullOrEmpty(t.Text)).ToArray();

		Assert.True(functionCalls.Length > 0, "Streaming should produce FunctionCallContent");
		Assert.True(functionResults.Length > 0, "Streaming should produce FunctionResultContent");
		Assert.True(textContents.Length > 0, "Streaming should produce TextContent");

		// Build history the way ChatViewModel does (WITH the fix):
		//   User → "What's the weather..."
		//   Assistant → [FunctionCallContent]
		//   Tool → [FunctionResultContent]
		//   Assistant → [TextContent]
		var history = new List<ChatMessage> { turn1UserMessage };
		history.Add(new ChatMessage(ChatRole.Assistant, functionCalls));
		history.Add(new ChatMessage(ChatRole.Tool, functionResults));
		history.Add(new ChatMessage(ChatRole.Assistant, textContents));

		// Turn 2: Follow-up using the manually-built history
		history.Add(new ChatMessage(ChatRole.User, "What about in Portland?"));

		var secondResponse = await client.GetResponseAsync(history, options);

		Assert.NotNull(secondResponse);
		Assert.NotNull(secondResponse.Messages);
		Assert.True(secondResponse.Messages.Count > 0, "Second response should have messages after streaming-based history");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_MultiTurnWithToolCalling_HistoryBuiltFromStreamedContent_ToolResultsPreservedInContext()
	{
		// This test verifies that tool results from a streamed turn 1 are available
		// in the model's context for turn 2 — proving the history is complete.
		// Uses a distinctive value (47°F) that the model can't hallucinate.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"The current weather in {location} is 47 degrees Fahrenheit and rainy",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var toolOptions = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Stream and collect all content
		var turn1UserMessage = new ChatMessage(ChatRole.User, "What's the weather in Seattle?");
		var turn1Messages = new List<ChatMessage> { turn1UserMessage };

		var allContents = new List<AIContent>();
		await foreach (var update in client.GetStreamingResponseAsync(turn1Messages, toolOptions))
		{
			foreach (var content in update.Contents)
				allContents.Add(content);
		}

		// Build history from streamed content (the ChatViewModel pattern)
		var functionCalls = allContents.OfType<FunctionCallContent>().ToArray();
		var functionResults = allContents.OfType<FunctionResultContent>().ToArray();
		var textContents = allContents.OfType<TextContent>().Where(t => !string.IsNullOrEmpty(t.Text)).ToArray();

		var history = new List<ChatMessage> { turn1UserMessage };
		if (functionCalls.Length > 0)
			history.Add(new ChatMessage(ChatRole.Assistant, functionCalls));
		if (functionResults.Length > 0)
			history.Add(new ChatMessage(ChatRole.Tool, functionResults));
		if (textContents.Length > 0)
			history.Add(new ChatMessage(ChatRole.Assistant, textContents));

		// Turn 2: Ask about the temperature WITHOUT tools — forces model to recall from context
		history.Add(new ChatMessage(ChatRole.User,
			"What was the exact temperature in Fahrenheit that the weather check returned for Seattle? Reply with just the number."));

		var secondResponse = await client.GetResponseAsync(history);
		Assert.NotNull(secondResponse);

		var responseText = secondResponse.Text ?? string.Empty;
		Assert.True(responseText.Contains("47", StringComparison.Ordinal),
			$"Follow-up should reference the tool result temperature (47°F) from streamed history, proving tool context is preserved. Got: '{responseText}'");
	}

	[Fact]
	public async Task GetResponseAsync_MultiTurnConversationWithToolCalling_ToolResultsPreservedInContext()
	{
		// Use a distinctive, unlikely-to-be-hallucinated temperature value
		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"The current weather in {location} is 47 degrees Fahrenheit and rainy",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var toolOptions = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Ask about weather WITH tools (triggers tool call + result)
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle?")
		};

		var firstResponse = await client.GetResponseAsync(messages, toolOptions);
		Assert.NotNull(firstResponse);

		// Turn 2: Ask about the temperature WITHOUT tools — forces the model to recall from context.
		// If tool results were dropped from the transcript, the model has no way to know "47".
		var followUpMessages = new List<ChatMessage>(firstResponse.Messages)
		{
			new(ChatRole.User, "What was the exact temperature in Fahrenheit that the weather check returned for Seattle? Reply with just the number.")
		};

		// No tools on follow-up — model must rely on conversation history
		var secondResponse = await client.GetResponseAsync(followUpMessages);
		Assert.NotNull(secondResponse);

		var responseText = secondResponse.Text ?? string.Empty;
		Assert.True(responseText.Contains("47", StringComparison.Ordinal),
			$"Follow-up response should reference the tool result temperature (47°F), proving tool results are preserved in context. Got: '{responseText}'");
	}
}

public enum PointOfInterestCategory
{
	Cafe,
	Campground,
	Hotel,
	Marina,
	Museum,
	NationalMonument,
	Restaurant
}
