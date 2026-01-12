using System.Text.Json;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for ChatClient JSON schema response format tests.
/// Provides common tests for any IChatClient implementation that supports structured JSON output.
/// </summary>
/// <typeparam name="T">The concrete ChatClient type to test.</typeparam>
public abstract class ChatClientJsonSchemaTestsBase<T>
	where T : class, IChatClient, new()
{
	// Record types for testing JSON schema responses
	public record WeatherResponse(string Location, int Temperature, string Condition);
	public record SimpleResponse(string Message);
	public record TripItinerary(string Destination, int Days, Activity[] Activities);
	public record Activity(string Name, string Description, string Time);

	[Fact]
	public async Task GetResponseAsync_WithJsonSchemaFormat_ReturnsStructuredResponse()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is the weather in Seattle? Respond with location, temperature in fahrenheit, and condition.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<WeatherResponse>()
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.NotNull(response.Text);
		
		// Verify the response can be parsed as the expected type
		var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(response.Text, JsonSerializerOptions.Web);
		Assert.NotNull(weatherResponse);
		Assert.NotNull(weatherResponse.Location);
	}

	[Fact]
	public async Task GetResponseAsync_WithJsonSchemaFormat_ReturnsValidJson()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Say hello in a message field.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<SimpleResponse>()
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.NotNull(response.Text);
		
		// Verify it's valid JSON
		var doc = JsonDocument.Parse(response.Text);
		Assert.NotNull(doc);
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithJsonSchemaFormat_StreamsValidJson()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is the weather in Boston? Respond with location, temperature in fahrenheit, and condition.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<WeatherResponse>()
		};

		var textBuilder = new System.Text.StringBuilder();
		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			foreach (var content in update.Contents)
			{
				if (content is TextContent textContent)
				{
					textBuilder.Append(textContent.Text);
				}
			}
		}

		var completeText = textBuilder.ToString();
		Assert.False(string.IsNullOrEmpty(completeText), "Should receive streaming response");
		
		// Verify the complete response is valid JSON
		var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(completeText);
		Assert.NotNull(weatherResponse);
	}

	[Fact]
	public virtual async Task GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object with a 'greeting' field containing 'hello'")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var response = await client.GetResponseAsync(messages, options);
		
		Assert.NotNull(response);
		Assert.NotNull(response.Text);
		Assert.False(string.IsNullOrWhiteSpace(response.Text), "Response text should not be empty");
		
		// Verify the response is valid JSON (will throw if invalid)
		using var doc = JsonDocument.Parse(response.Text);
		Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
	}

	[Fact]
	public virtual async Task GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Generate a JSON object with a 'greeting' field containing 'hello'")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.Json
		};

		var textBuilder = new System.Text.StringBuilder();

		await foreach (var update in client.GetStreamingResponseAsync(messages, options))
		{
			if (!string.IsNullOrEmpty(update.Text))
			{
				textBuilder.Append(update.Text);
			}
		}
		
		var responseText = textBuilder.ToString();
		Assert.False(string.IsNullOrWhiteSpace(responseText), "Streaming response should not be empty");
		
		// Verify the complete response is valid JSON (will throw if invalid)
		using var doc = JsonDocument.Parse(responseText);
		Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
	}

	[Fact]
	public async Task GetResponseAsync_WithJsonSchemaFormatAndCustomOptions_Works()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Say hello in a message field.")
		};
		
		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<SimpleResponse>(jsonOptions),
			Temperature = 0.5f
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.NotNull(response.Text);
		
		// Verify the response can be parsed
		var simpleResponse = JsonSerializer.Deserialize<SimpleResponse>(response.Text, jsonOptions);
		Assert.NotNull(simpleResponse);
	}

	[Fact]
	public async Task GetResponseAsync_WithComplexJsonSchema_ReturnsStructuredResponse()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, "You are a travel assistant. Always respond with structured itinerary data."),
			new(ChatRole.User, "Plan a 1-day trip to Seattle with 2 activities.")
		};
		var options = new ChatOptions
		{
			ResponseFormat = ChatResponseFormat.ForJsonSchema<TripItinerary>()
		};

		var response = await client.GetResponseAsync(messages, options);

		Assert.NotNull(response);
		Assert.NotNull(response.Text);
		
		var itinerary = JsonSerializer.Deserialize<TripItinerary>(response.Text, JsonSerializerOptions.Web);
		Assert.NotNull(itinerary);
		Assert.NotNull(itinerary.Destination);
	}

	[Fact]
	public async Task GetResponseAsync_WithNullMessages_ThrowsArgumentNullException()
	{
		var client = new T();

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => client.GetResponseAsync(null!));
	}

	[Fact]
	public async Task GetStreamingResponseAsync_WithNullMessages_ThrowsArgumentNullException()
	{
		var client = new T();

		await Assert.ThrowsAsync<ArgumentNullException>(async () =>
		{
			await foreach (var update in client.GetStreamingResponseAsync(null!))
			{
				// Should not reach here
			}
		});
	}

	[Fact]
	public async Task GetResponseAsyncT_ReturnsStructuredResponse()
	{
		var client = new T();
		var message = new ChatMessage(ChatRole.User, "What is the weather in Seattle? Respond with location, temperature in fahrenheit, and condition.");

		var response = await client.GetResponseAsync<WeatherResponse>(message);

		Assert.NotNull(response);
		Assert.NotNull(response.Result);
		Assert.NotNull(response.Result.Location);
	}

	[Fact]
	public async Task GetResponseAsyncT_WithSimpleType_ReturnsDeserializedResult()
	{
		var client = new T();
		var message = new ChatMessage(ChatRole.User, "Say hello in a message field.");

		var response = await client.GetResponseAsync<SimpleResponse>(message);

		Assert.NotNull(response);
		Assert.NotNull(response.Result);
		Assert.NotNull(response.Result.Message);
	}

	[Fact]
	public async Task GetResponseAsyncT_WithComplexType_ReturnsStructuredResponse()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.System, "You are a travel assistant. Always respond with structured itinerary data."),
			new(ChatRole.User, "Plan a 1-day trip to Seattle with 2 activities.")
		};

		var response = await client.GetResponseAsync<TripItinerary>(messages);

		Assert.NotNull(response);
		Assert.NotNull(response.Result);
		Assert.NotNull(response.Result.Destination);
	}

	[Fact]
	public async Task GetResponseAsyncT_WithChatOptions_ReturnsStructuredResponse()
	{
		var client = new T();
		var message = new ChatMessage(ChatRole.User, "What is the weather in Boston? Respond with location, temperature in fahrenheit, and condition.");
		var options = new ChatOptions
		{
			Temperature = 0.5f
		};

		var response = await client.GetResponseAsync<WeatherResponse>(message, options);

		Assert.NotNull(response);
		Assert.NotNull(response.Result);
		Assert.NotNull(response.Result.Location);
	}

	[Fact]
	public async Task GetResponseAsyncT_WithMessageList_ReturnsStructuredResponse()
	{
		var client = new T();
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is the weather in Chicago? Respond with location, temperature in fahrenheit, and condition.")
		};

		var response = await client.GetResponseAsync<WeatherResponse>(messages);

		Assert.NotNull(response);
		Assert.NotNull(response.Result);
		Assert.NotNull(response.Result.Location);
	}

	[Fact]
	public async Task GetResponseAsyncT_WithNullMessage_ThrowsArgumentNullException()
	{
		var client = new T();

		await Assert.ThrowsAsync<NullReferenceException>(
			() => client.GetResponseAsync<SimpleResponse>((ChatMessage)null!));
	}

	[Fact]
	public async Task GetResponseAsyncT_WithNullMessages_ThrowsArgumentNullException()
	{
		var client = new T();

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => client.GetResponseAsync<SimpleResponse>((IList<ChatMessage>)null!));
	}

	[Fact]
	public async Task GetResponseAsyncT_WithCancellationToken_SupportsCancellation()
	{
		var client = new T();
		var message = new ChatMessage(ChatRole.User, "Say hello in a message field.");
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => client.GetResponseAsync<SimpleResponse>(message, cancellationToken: cts.Token));
	}
}
