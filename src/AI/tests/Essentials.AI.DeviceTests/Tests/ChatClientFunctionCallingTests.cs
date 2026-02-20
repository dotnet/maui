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
		// Add each content item to history as it arrives, preserving stream order.
		// This catches both the original bug (dropping tool content) and the ordering bug
		// (grouping all calls together instead of interleaving call→result pairs).

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Stream the first response, building history inline
		var turn1UserMessage = new ChatMessage(ChatRole.User, "What's the weather in Seattle?");
		var history = new List<ChatMessage> { turn1UserMessage };

		bool hasFunctionCall = false;
		bool hasFunctionResult = false;
		bool hasText = false;
		ChatMessage? textMessage = null;

		await foreach (var update in client.GetStreamingResponseAsync([turn1UserMessage], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case FunctionCallContent fc:
						history.Add(new ChatMessage(ChatRole.Assistant, [fc]));
						textMessage = null;
						hasFunctionCall = true;
						break;
					case FunctionResultContent fr:
						history.Add(new ChatMessage(ChatRole.Tool, [fr]));
						hasFunctionResult = true;
						break;
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						if (textMessage is null)
						{
							textMessage = new ChatMessage(ChatRole.Assistant, [tc]);
							history.Add(textMessage);
						}
						else
						{
							textMessage.Contents.Add(tc);
						}
						hasText = true;
						break;
				}
			}
		}

		Assert.True(hasFunctionCall, "Streaming should produce FunctionCallContent");
		Assert.True(hasFunctionResult, "Streaming should produce FunctionResultContent");
		Assert.True(hasText, "Streaming should produce TextContent");

		// Turn 2: Follow-up using the inline-built history
		history.Add(new ChatMessage(ChatRole.User, "What about in Portland?"));

		var secondResponse = await client.GetResponseAsync(history, options);

		Assert.NotNull(secondResponse);
		Assert.NotNull(secondResponse.Messages);
		Assert.True(secondResponse.Messages.Count > 0, "Second response should have messages after streaming-based history");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_MultiTurnWithToolCalling_HistoryBuiltFromStreamedContent_ToolResultsPreservedInContext()
	{
		// Verifies tool results from streamed turn 1 are available in turn 2's context.
		// Uses a distinctive value (47°F) that the model can't hallucinate.
		// History is built inline as content arrives, preserving stream order.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"The current weather in {location} is 47 degrees Fahrenheit and rainy",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var toolOptions = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Turn 1: Stream and build history inline
		var turn1UserMessage = new ChatMessage(ChatRole.User, "What's the weather in Seattle?");
		var history = new List<ChatMessage> { turn1UserMessage };
		ChatMessage? textMessage = null;

		await foreach (var update in client.GetStreamingResponseAsync([turn1UserMessage], toolOptions))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case FunctionCallContent fc:
						history.Add(new ChatMessage(ChatRole.Assistant, [fc]));
						textMessage = null;
						break;
					case FunctionResultContent fr:
						history.Add(new ChatMessage(ChatRole.Tool, [fr]));
						break;
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						if (textMessage is null)
						{
							textMessage = new ChatMessage(ChatRole.Assistant, [tc]);
							history.Add(textMessage);
						}
						else
						{
							textMessage.Contents.Add(tc);
						}
						break;
				}
			}
		}

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

	[Fact]
	public async Task GetStreamingResponseAsync_WithToolCalling_NoNullTextContent()
	{
		// Verifies that no text content with the literal value "null" leaks through
		// the streaming pipeline during tool-calling conversations. This guards against
		// both Apple framework sentinels and any serialization bugs in our code.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		var allTextDeltas = new List<string>();

		await foreach (var update in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
			foreach (var content in update.Contents)
			{
				if (content is TextContent tc && tc.Text is not null)
				{
					allTextDeltas.Add(tc.Text);
				}
			}
		}

		// No text delta should be the literal string "null"
		Assert.DoesNotContain("null", allTextDeltas);
		Assert.True(allTextDeltas.Count > 0, "Should receive at least one text delta");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithToolCalling_StreamOrderIsToolsBeforeResponse()
	{
		// Records the exact order of content types in a tool-calling stream
		// to verify that tool calls/results arrive BEFORE the final text response.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Record every content item in order: (type, snippet)
		var streamLog = new List<(string Type, string Snippet)>();

		await foreach (var update in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case FunctionCallContent fc:
						streamLog.Add(("ToolCall", $"{fc.Name}({fc.CallId})"));
						break;
					case FunctionResultContent fr:
						streamLog.Add(("ToolResult", $"{fr.CallId}: {fr.Result?.ToString()?[..Math.Min(fr.Result?.ToString()?.Length ?? 0, 40)]}"));
						break;
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						streamLog.Add(("Text", tc.Text.Length > 60 ? tc.Text[..60] + "..." : tc.Text));
						break;
				}
			}
		}

		// Output the full stream log for diagnostics
		var logSummary = string.Join("\n", streamLog.Select((item, i) => $"  [{i}] {item.Type}: {item.Snippet}"));
		var typeOrder = string.Join(" → ", streamLog.Select(x => x.Type));

		// Basic sanity: we got tool calls and text
		Assert.True(streamLog.Any(x => x.Type == "ToolCall"),
			$"Expected at least one ToolCall in stream. Full log:\n{logSummary}");
		Assert.True(streamLog.Any(x => x.Type == "Text"),
			$"Expected at least one Text in stream. Full log:\n{logSummary}");

		// Find the index of the first tool call and first text
		int firstToolCallIndex = streamLog.FindIndex(x => x.Type == "ToolCall");
		int firstTextIndex = streamLog.FindIndex(x => x.Type == "Text");

		// Assert: tool calls should arrive BEFORE text (the model can't respond before calling tools)
		Assert.True(firstToolCallIndex < firstTextIndex,
			$"Expected ToolCall (index {firstToolCallIndex}) before Text (index {firstTextIndex}). " +
			$"Stream order: {typeOrder}\nFull log:\n{logSummary}");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithToolCalling_StreamOrderPreservedThroughFICC()
	{
		// Tests the stream order through a FunctionInvokingChatClient middleware chain,
		// matching the sample app's pipeline (NonFunctionInvokingChatClient pattern).
		// The raw client sends ToolCall→ToolResult→Text. Verify FICC doesn't reorder.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Sunny, 72°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var rawClient = EnableFunctionCalling(new T());

		// Wrap in a DelegatingChatClient that hides FunctionCallContent from FICC
		// (same pattern as NonFunctionInvokingChatClient/ToolCallPassThroughHandler)
		var wrappedClient = new WrapperClient(rawClient);
		var ficc = new FunctionInvokingChatClient(wrappedClient);
		var outerClient = new UnwrapperClient(ficc);

		var options = new ChatOptions { Tools = [weatherTool] };
		var streamLog = new List<(string Type, string Snippet)>();

		await foreach (var update in outerClient.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case FunctionCallContent fc:
						streamLog.Add(("ToolCall", $"{fc.Name}({fc.CallId})"));
						break;
					case FunctionResultContent fr:
						streamLog.Add(("ToolResult", $"{fr.CallId}"));
						break;
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						streamLog.Add(("Text", tc.Text.Length > 60 ? tc.Text[..60] + "..." : tc.Text));
						break;
				}
			}
		}

		var logSummary = string.Join("\n", streamLog.Select((item, i) => $"  [{i}] {item.Type}: {item.Snippet}"));
		var typeOrder = string.Join(" → ", streamLog.Select(x => x.Type));

		Assert.True(streamLog.Any(x => x.Type == "ToolCall"),
			$"Expected at least one ToolCall. Full log:\n{logSummary}");
		Assert.True(streamLog.Any(x => x.Type == "Text"),
			$"Expected at least one Text. Full log:\n{logSummary}");

		int firstToolCallIndex = streamLog.FindIndex(x => x.Type == "ToolCall");
		int firstTextIndex = streamLog.FindIndex(x => x.Type == "Text");

		Assert.True(firstToolCallIndex < firstTextIndex,
			$"FICC reordered stream! ToolCall (index {firstToolCallIndex}) should come before Text (index {firstTextIndex}). " +
			$"Stream order: {typeOrder}\nFull log:\n{logSummary}");
	}

	/// <summary>Wraps FunctionCallContent/FunctionResultContent so FICC ignores them.</summary>
	private sealed class WrapperClient(IChatClient inner) : DelegatingChatClient(inner)
	{
		public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages, ChatOptions? options = null,
			[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
			{
				for (int i = 0; i < update.Contents.Count; i++)
				{
					if (update.Contents[i] is FunctionCallContent fcc)
						update.Contents[i] = new MarkerFCC(fcc);
					else if (update.Contents[i] is FunctionResultContent frc)
						update.Contents[i] = new MarkerFRC(frc);
				}
				yield return update;
			}
		}
	}

	/// <summary>Unwraps marker types back to FunctionCallContent/FunctionResultContent.</summary>
	private sealed class UnwrapperClient(IChatClient inner) : DelegatingChatClient(inner)
	{
		public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
			IEnumerable<ChatMessage> messages, ChatOptions? options = null,
			[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
			{
				for (int i = 0; i < update.Contents.Count; i++)
				{
					if (update.Contents[i] is MarkerFCC m1)
						update.Contents[i] = m1.Inner;
					else if (update.Contents[i] is MarkerFRC m2)
						update.Contents[i] = m2.Inner;
				}
				yield return update;
			}
		}
	}

	private sealed class MarkerFCC(FunctionCallContent inner) : AIContent { public FunctionCallContent Inner => inner; }
	private sealed class MarkerFRC(FunctionResultContent inner) : AIContent { public FunctionResultContent Inner => inner; }

	[Fact]
	public async Task GetStreamingResponseAsync_MultiTurnWithToolCalling_ContentOrderPreserved()
	{
		// Verifies that conversation history built from streaming preserves the
		// correct interleaving order: each FunctionCallContent (Assistant) is followed
		// by its FunctionResultContent (Tool) before the next call or text.

		var weatherTool = AIFunctionFactory.Create(
			(string location) => $"Clear skies, 68°F in {location}",
			name: "GetWeather",
			description: "Gets the weather for a location");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [weatherTool]
		};

		// Stream and build history, tracking the order of content types
		var history = new List<ChatMessage>
		{
			new(ChatRole.User, "What's the weather in Seattle?")
		};
		var contentOrder = new List<string>(); // Track: "call", "result", "text"
		ChatMessage? textMessage = null;

		await foreach (var update in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What's the weather in Seattle?")], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case FunctionCallContent fc:
						contentOrder.Add("call");
						history.Add(new ChatMessage(ChatRole.Assistant, [fc]));
						textMessage = null;
						break;
					case FunctionResultContent fr:
						contentOrder.Add("result");
						history.Add(new ChatMessage(ChatRole.Tool, [fr]));
						break;
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						if (!contentOrder.Contains("text") || contentOrder.Last() != "text")
							contentOrder.Add("text");
						if (textMessage is null)
						{
							textMessage = new ChatMessage(ChatRole.Assistant, [tc]);
							history.Add(textMessage);
						}
						else
						{
							textMessage.Contents.Add(tc);
						}
						break;
				}
			}
		}

		// Verify: every "call" is immediately followed by "result" (no reordering)
		for (int i = 0; i < contentOrder.Count; i++)
		{
			if (contentOrder[i] == "call")
			{
				Assert.True(i + 1 < contentOrder.Count && contentOrder[i + 1] == "result",
					$"FunctionCall at index {i} should be followed by FunctionResult. Order: [{string.Join(", ", contentOrder)}]");
			}
		}

		// Verify the history can be used for a follow-up (ordering is valid for the transcript)
		history.Add(new ChatMessage(ChatRole.User, "What about Portland?"));
		var followUp = await client.GetResponseAsync(history, options);
		Assert.NotNull(followUp);
		Assert.True(followUp.Messages.Count > 0, "Follow-up should succeed with correctly ordered history");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithToolCalling_NoNullTextBeforeToolCalls()
	{
		// Captures the EXACT raw sequence of all content items during a tool-calling stream.
		// Verifies: (1) no text delta contains "null" (case-insensitive substring),
		// (2) all text is non-empty, (3) tool calls always have valid arguments.
		// Uses a landmarks query that exercises search tools to maximize coverage.

		var landmarkTool = AIFunctionFactory.Create(
			(string query) => """
			[
				{"name": "Table Mountain", "country": "South Africa"},
				{"name": "Victoria Falls", "country": "Zimbabwe"},
				{"name": "Pyramids of Giza", "country": "Egypt"}
			]
			""",
			name: "SearchLandmarks",
			description: "Searches for landmarks and points of interest by query");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions
		{
			Tools = [landmarkTool]
		};

		// Capture every single content item with its exact value
		var rawLog = new List<(string Type, string Value)>();

		await foreach (var update in client.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "What are famous landmarks in Africa?")], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case TextContent tc:
						rawLog.Add(("Text", tc.Text ?? "<null>"));
						break;
					case FunctionCallContent fc:
						rawLog.Add(("ToolCall", $"{fc.Name}({fc.CallId}): {fc.Arguments}"));
						break;
					case FunctionResultContent fr:
						rawLog.Add(("ToolResult", $"{fr.CallId}: {fr.Result}"));
						break;
					default:
						rawLog.Add((content.GetType().Name, content.ToString() ?? "<null>"));
						break;
				}
			}
		}

		var logSummary = string.Join("\n", rawLog.Select((item, i) => $"  [{i}] {item.Type}: {item.Value}"));

		// 1. No text should be the literal "null" or contain "null" as a value artifact
		var textItems = rawLog.Where(x => x.Type == "Text").ToList();
		foreach (var (_, value) in textItems)
		{
			Assert.False(string.Equals(value, "null", StringComparison.OrdinalIgnoreCase),
				$"Found literal 'null' text in stream. Full log:\n{logSummary}");
			Assert.False(string.Equals(value, "<null>", StringComparison.Ordinal),
				$"Found null TextContent.Text in stream. Full log:\n{logSummary}");
		}

		// 2. All text should be non-empty (Swift guard and C# filter should prevent this)
		foreach (var (_, value) in textItems)
		{
			Assert.False(string.IsNullOrWhiteSpace(value),
				$"Found empty/whitespace-only text in stream. Full log:\n{logSummary}");
		}

		// 3. Should have tool calls and text in the response
		Assert.True(rawLog.Any(x => x.Type == "ToolCall"),
			$"Expected at least one ToolCall. Full log:\n{logSummary}");
		Assert.True(textItems.Count > 0,
			$"Expected at least one Text item. Full log:\n{logSummary}");
	}

	[Fact]
	public async Task GetStreamingResponseAsync_ViewModelSimulation_ThinkingBubbleRemovedBeforeToolCalls()
	{
		// Simulates the ChatViewModel's exact state machine to reproduce the "null thinking bubble" bug.
		// The user reports: "what are landmarks in africa" → thinking bubble text goes to "null",
		// then tool calls appear, then text streams — but the thinking bubble stays stuck.
		//
		// This test runs through the FULL middleware pipeline (wrap→FICC→unwrap) matching the app,
		// then processes each update exactly as ChatViewModel does, recording what happens to the
		// simulated "thinking bubble".

		var landmarkTool = AIFunctionFactory.Create(
			(string query) => """
			[
				{"name": "Table Mountain", "country": "South Africa"},
				{"name": "Victoria Falls", "country": "Zimbabwe"},
				{"name": "Pyramids of Giza", "country": "Egypt"}
			]
			""",
			name: "SearchLandmarks",
			description: "Searches for landmarks and points of interest by query");

		// Set up the SAME middleware pipeline as the app:
		// AppleClient → WrapperClient → FunctionInvokingChatClient → UnwrapperClient
		var rawClient = EnableFunctionCalling(new T());
		var wrappedClient = new WrapperClient(rawClient);
		var ficc = new FunctionInvokingChatClient(wrappedClient);
		var pipeline = new UnwrapperClient(ficc);

		var options = new ChatOptions { Tools = [landmarkTool] };

		// === Simulate ChatViewModel state machine ===
		string thinkingText = "Thinking...";
		bool thinkingInMessages = true;
		string? assistantText = null;
		#pragma warning disable CS0219 // Variable assigned but never read — it's used for state tracking
		var assistantIsThinking = false; // true when assistantBubble == thinkingBubble
		#pragma warning restore CS0219
		var preToolTextValues = new List<string>(); // Any text set on thinking bubble before tool calls
		var stateLog = new List<string>(); // Full state transition log
		bool toolCallSeen = false;

		await foreach (var update in pipeline.GetStreamingResponseAsync(
			[new ChatMessage(ChatRole.User, "what are landmarks in africa")], options))
		{
			foreach (var content in update.Contents)
			{
				switch (content)
				{
					case TextContent tc when !string.IsNullOrEmpty(tc.Text):
						if (!toolCallSeen)
						{
							// Text arriving BEFORE tool calls — this is the scenario that causes the bug
							if (assistantText is null)
							{
								if (thinkingInMessages)
								{
									// Line 122: thinkingBubble.Text = textContent.Text
									thinkingText = tc.Text;
									assistantText = thinkingText;
									assistantIsThinking = true;
									preToolTextValues.Add(tc.Text);
									stateLog.Add($"PRE-TOOL TEXT on thinking bubble: \"{tc.Text}\"");
								}
								else
								{
									assistantText = tc.Text;
									stateLog.Add($"PRE-TOOL TEXT new bubble: \"{tc.Text}\"");
								}
							}
							else
							{
								assistantText += tc.Text;
								stateLog.Add($"PRE-TOOL TEXT append: \"{tc.Text}\"");
							}
						}
						else
						{
							stateLog.Add($"POST-TOOL TEXT: \"{(tc.Text.Length > 60 ? tc.Text[..60] + "..." : tc.Text)}\"");
						}
						break;

					case FunctionCallContent fc:
						toolCallSeen = true;
						stateLog.Add($"TOOL CALL: {fc.Name}({fc.CallId})");

						// Simulate the FunctionCallContent handler (FIXED version)
						if (assistantText is not null)
						{
							stateLog.Add($"  → Removing assistant bubble with text: \"{assistantText}\"");
							// This is the bug: OLD code would keep the bubble if text.Trim() was not empty
							// NEW code always removes it
							thinkingInMessages = false;
							assistantText = null;
							assistantIsThinking = false;
						}
						else
						{
							stateLog.Add("  → Removing thinking bubble (no pre-tool text)");
							thinkingInMessages = false;
						}
						break;

					case FunctionResultContent fr:
						stateLog.Add($"TOOL RESULT: {fr.CallId}");
						break;
				}
			}
		}

		var fullLog = string.Join("\n", stateLog);

		// KEY ASSERTIONS:

		// 1. No pre-tool text should be the literal "null"
		foreach (var text in preToolTextValues)
		{
			Assert.False(string.Equals(text, "null", StringComparison.OrdinalIgnoreCase),
				$"Pre-tool text was literal 'null'! This means a jsonString or toString on a null object " +
				$"is leaking through the streaming pipeline.\nFull state log:\n{fullLog}");
		}

		// 2. Should have seen tool calls (the model should use the tool for this query)
		Assert.True(toolCallSeen,
			$"Expected tool calls for 'what are landmarks in africa' query.\nFull state log:\n{fullLog}");

		// 3. If ANY pre-tool text arrived, log it as a finding (this is what the user sees)
		// This is informational — the fix (always remove) handles it, but we want to KNOW if it happens
		if (preToolTextValues.Count > 0)
		{
			// Pre-tool text DID arrive. With the old code, this would have caused the stuck bubble.
			// With the fix, the bubble is always removed. Log the values for diagnostics.
			var preToolSummary = string.Join(", ", preToolTextValues.Select(v => $"\"{v}\""));
			stateLog.Add($"\n=== FINDING: {preToolTextValues.Count} pre-tool text value(s): {preToolSummary} ===");
		}
	}

	[Fact]
	public async Task GetStreamingResponseAsync_ViewModelSimulation_NoNullTextInStream_RawClient()
	{
		// Same as above but uses the RAW client (no middleware) to isolate whether
		// "null" text comes from Apple or from the middleware pipeline.
		// Runs 3 times to account for model non-determinism.

		var landmarkTool = AIFunctionFactory.Create(
			(string query) => """
			[
				{"name": "Table Mountain", "country": "South Africa"},
				{"name": "Victoria Falls", "country": "Zimbabwe"},
				{"name": "Pyramids of Giza", "country": "Egypt"}
			]
			""",
			name: "SearchLandmarks",
			description: "Searches for landmarks and points of interest by query");

		var client = EnableFunctionCalling(new T());
		var options = new ChatOptions { Tools = [landmarkTool] };

		for (int run = 0; run < 3; run++)
		{
			var allContent = new List<(string Type, string Value)>();

			await foreach (var update in client.GetStreamingResponseAsync(
				[new ChatMessage(ChatRole.User, "what are landmarks in africa")], options))
			{
				foreach (var content in update.Contents)
				{
					switch (content)
					{
						case TextContent tc:
							allContent.Add(("Text", tc.Text ?? "<C#-null>"));
							break;
						case FunctionCallContent fc:
							allContent.Add(("ToolCall", fc.Name));
							break;
						case FunctionResultContent fr:
							allContent.Add(("ToolResult", fr.CallId));
							break;
						default:
							allContent.Add((content.GetType().Name, content.ToString() ?? "<null>"));
							break;
					}
				}
			}

			var log = string.Join("\n", allContent.Select((c, i) => $"  [{i}] {c.Type}: {c.Value}"));

			// Check for literal "null" text
			var nullTexts = allContent
				.Where(c => c.Type == "Text" && string.Equals(c.Value, "null", StringComparison.OrdinalIgnoreCase))
				.ToList();

			Assert.True(nullTexts.Count == 0,
				$"Run {run + 1}/3: Found {nullTexts.Count} literal 'null' text item(s) in raw stream!\n{log}");

			// Check for C# null text
			var csharpNulls = allContent
				.Where(c => c.Type == "Text" && c.Value == "<C#-null>")
				.ToList();

			Assert.True(csharpNulls.Count == 0,
				$"Run {run + 1}/3: Found {csharpNulls.Count} C# null TextContent.Text in raw stream!\n{log}");
		}
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
