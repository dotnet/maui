using Azure.AI.OpenAI;
using Azure;
using OpenAI.Chat;
using UITest.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace UITest.Appium.AI
{
	public static class HelperExtensions
	{
		static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

		const string FindElementImagePrompt = @"
		You are a software tool designed to analyze mobile application screenshots. The user will provide you with a screenshot and then ask for the coordinates of an element in the screenshot. Your job is to interpret the user's description of an element, find the bounding box of the element in the screenshot, and return that bounding box. You should return responses in JSON object format. More specific instructions:
		- Use computer vision techniques including OCR to look for text or visual elements specified by the user.
		- Don't include any text or conversation.
		- Do not include any explanations of your choices.
		- Reply simply with the bounding box in JSON format with the following keys: x, y, width, and height.
		- Uses pixels as the average unit.
		- Include also the content text, if the element contains it. Use the following key in JSON: text.";
		
		const string VerifyScreenshotPrompt = @"
		You are a software tool designed to analyze mobile application screenshots. The user will provide you with two screenshots and then ask to compare them. Your job is to analyze the screenshots, and return a boolean indicating whether they are equal or not. You should return responses in JSON object format. More specific instructions:
		- Use computer vision techniques including OCR to look for differences, even if they are small.
		- Don't include any text or conversation.
		- Do not include any explanations of your choices.
		- Reply simply with the boolean in JSON format with the following keys: equals.";

		public static async Task ClickWithAI(this IApp app, string prompt)
		{
			var element = await FindElementWithAI(app, prompt);
			element?.Click();
		}

		public static async Task DoubleClickWithAI(this IApp app, string prompt)
		{
			var elementToDoubleClick = await FindElementWithAI(app, prompt);

			if (elementToDoubleClick is not null)
			{
				app.CommandExecutor.Execute("doubleClick", new Dictionary<string, object>
				{
					{ "element", elementToDoubleClick },
				});
			}
		}

		public static async Task TapWithAI(this IApp app, string prompt)
		{
			var element = await FindElementWithAI(app, prompt);
			element?.Tap();
		}

		public static async Task DoubleTapWithAI(this IApp app, string prompt)
		{
			var elementToDoubleTap = await FindElementWithAI(app, prompt);

			if (elementToDoubleTap is not null)
			{
				app.CommandExecutor.Execute("doubleTap", new Dictionary<string, object>
				{
					{ "element", elementToDoubleTap },
				});
			}
		}

		public static async Task RightClickWithAI(this IApp app, string prompt)
		{
			var element = await FindElementWithAI(app, prompt);
			element?.Command.Execute("click", new Dictionary<string, object>()
			{
				{ "element", element },
				{ "button", "right" }
			});
		}

		public static async Task PressDownWithAI(this IApp app, string prompt)
		{
			var element = await FindElementWithAI(app, prompt);
			element?.Command.Execute("pressDown", new Dictionary<string, object>()
			{
				{ "element", element }
			});
		}

		public static async Task EnterTextWithAI(this IApp app, string prompt, string text)
		{
			var element = await FindElementWithAI(app, prompt);

			if (element is not null)
			{
				element.SendKeys(text);
				app.DismissKeyboard();
			}
		}

		public static async Task ClearTextWithAI(this IApp app, string prompt)
		{
			var element = await FindElementWithAI(app, prompt);
			element?.Clear();
		}
		public static async Task LongPressWithAI(this IApp app, string prompt)
		{
			var elementToLongPress = await FindElementWithAI(app, prompt);

			if (elementToLongPress is not null)
			{
				app.CommandExecutor.Execute("longPress", new Dictionary<string, object>
				{
					{ "element", elementToLongPress },
				});
			}
		}

		public static async Task TouchAndHoldWithAI(this IApp app, string prompt)
		{
			var elementToTouchAndHold = await FindElementWithAI(app, prompt);
		
			if (elementToTouchAndHold is not null)
			{
				app.CommandExecutor.Execute("touchAndHold", new Dictionary<string, object>
				{
					{ "element", elementToTouchAndHold },
				});
			}
		}

		public static async Task PinchToZoomInWithAI(this IApp app, string prompt, TimeSpan? duration = null)
		{
			var elementToPinchToZoomIn = await FindElementWithAI(app, prompt);

			if (elementToPinchToZoomIn is not null)
			{
				app.CommandExecutor.Execute("pinchToZoomIn", new Dictionary<string, object>
				{
					{ "element", elementToPinchToZoomIn },
					{ "duration", duration ?? TimeSpan.FromSeconds(1) }
				});
			}
		}

		public static async Task PinchToZoomOutWithAI(this IApp app, string prompt, TimeSpan? duration = null)
		{
			var elementToPinchToZoomOut = await FindElementWithAI(app, prompt);

			if (elementToPinchToZoomOut is not null)
			{
				app.CommandExecutor.Execute("pinchToZoomOut", new Dictionary<string, object>
				{
					{ "element", elementToPinchToZoomOut },
					{ "duration", duration ?? TimeSpan.FromSeconds(1) }
				});
			}
		}

		public static async Task SwipeLeftToRightWithAI(this IApp app, string prompt, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = await FindElementWithAI(app, prompt);

			if (elementToSwipe is not null)
			{
				app.CommandExecutor.Execute("swipeLeftToRight", new Dictionary<string, object>
				{
					{ "element", elementToSwipe },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task SwipeRightToLeftWithAI(this IApp app, string prompt, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToSwipe = await FindElementWithAI(app, prompt);

			if (elementToSwipe is not null)
			{
				app.CommandExecutor.Execute("swipeRightToLeft", new Dictionary<string, object>
				{
					{ "element", elementToSwipe },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task ScrollLeftWithAI(this IApp app, string prompt, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = await FindElementWithAI(app, prompt);

			if (elementToScroll is not null)
			{
				app.CommandExecutor.Execute("scrollLeft", new Dictionary<string, object>
				{
					{ "element", elementToScroll },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task ScrollDownWithAI(this IApp app, string prompt, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = await FindElementWithAI(app, prompt);

			if (elementToScroll is not null)
			{
				app.CommandExecutor.Execute("scrollDown", new Dictionary<string, object>
				{
					{ "element", elementToScroll },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task ScrollRightWithAI(this IApp app, string prompt, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = await FindElementWithAI(app, prompt);

			if (elementToScroll is not null)
			{
				app.CommandExecutor.Execute("scrollRight", new Dictionary<string, object>
				{
					{ "element", elementToScroll },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task ScrollUpWithAI(this IApp app, string prompt, ScrollStrategy strategy = ScrollStrategy.Auto, double swipePercentage = 0.67, int swipeSpeed = 500, bool withInertia = true)
		{
			var elementToScroll = await FindElementWithAI(app, prompt);

			if (elementToScroll is not null)
			{
				app.CommandExecutor.Execute("scrollUp", new Dictionary<string, object>
				{
					{ "element", elementToScroll },
					{ "strategy", strategy },
					{ "swipePercentage", swipePercentage },
					{ "swipeSpeed", swipeSpeed },
					{ "withInertia", withInertia }
				});
			}
		}

		public static async Task SetSliderValueWithAI(this IApp app, string prompt, double value, double minimum = 0d, double maximum = 1d)
		{
			var element = await FindElementWithAI(app, prompt);

			if (element is not null)
			{
				app.CommandExecutor.Execute("setSliderValue", new Dictionary<string, object>
				{
					{ "element", element },
					{ "value", value },
					{ "minimum", minimum },
					{ "maximum", maximum },
				});
			}
		}

		public static async Task<IUIElement> WaitForElementWithAI(this IApp app, string prompt, string timeoutMessage = "Timed out waiting for element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Task<IUIElement?> result() => app.FindElementWithAI(prompt);
			var task = WaitForAtLeastOne(result, timeoutMessage, timeout, retryFrequency);
			var results = await task;

			return results;
		}

		public static async Task WaitForNoElementWithAI(this IApp app, string prompt, string timeoutMessage = "Timed out waiting for no element...", TimeSpan? timeout = null, TimeSpan? retryFrequency = null, TimeSpan? postTimeout = null)
		{
			Task<IUIElement?> result() => app.FindElementWithAI(prompt);
			await WaitForNone(result, timeoutMessage, timeout, retryFrequency);
		}

		public static async Task<bool> VerifyScreenshotWithAI(this IApp app, BinaryData snapshot, BinaryData referenceSnapshot, string? prompt = null)
		{
			string endpoint = "AZURE_OPENAI_ENDPOINT";
			string key = "AZURE_OPENAI_API_KEY";
			string modelName = "AZURE_OPENAI_MODEL_NAME";

			AzureKeyCredential credential = new AzureKeyCredential(key);
			AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
			ChatClient chatClient = azureClient.GetChatClient(modelName);

			var messages = new List<ChatMessage>
			{
				new SystemChatMessage(VerifyScreenshotPrompt),
				new UserChatMessage(
					ChatMessageContentPart.CreateTextPart(prompt ?? "Compare the images"),
					ChatMessageContentPart.CreateImagePart(snapshot, "image/png"),
					ChatMessageContentPart.CreateImagePart(referenceSnapshot, "image/png"))
			};

			var options = new ChatCompletionOptions
			{
				Temperature = (float)0.7,
				MaxOutputTokenCount = 800,
				FrequencyPenalty = 0,
				PresencePenalty = 0,
			};


			ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

			if (completion.Content != null && completion.Content.Count > 0)
			{
				string result = completion.Content[0].Text;
				VerifyScreenshotData? verifyScreenshotData = JsonSerializer.Deserialize<VerifyScreenshotData>(SanitizeResponse(result));

				if (verifyScreenshotData is not null)
					return verifyScreenshotData.AreEquals;
			}

			return false;
		}
		
		static Task<IUIElement> WaitForAtLeastOne(Func<Task<IUIElement?>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null,
			TimeSpan? retryFrequency = null)
		{
			var results = Wait(query, i => i != null, timeoutMessage, timeout, retryFrequency);

			return results;
		}

		static async Task WaitForNone(Func<Task<IUIElement?>> query,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			await Wait(query, i => i == null, timeoutMessage, timeout, retryFrequency);
		}

		static async Task<IUIElement> Wait(Func<Task<IUIElement?>> query,
			Func<IUIElement?, bool> satisfactory,
			string? timeoutMessage = null,
			TimeSpan? timeout = null, TimeSpan? retryFrequency = null)
		{
			timeout ??= DefaultTimeout;
			retryFrequency ??= TimeSpan.FromMilliseconds(500);
			timeoutMessage ??= "Timed out on query.";

			DateTime start = DateTime.Now;

			Task<IUIElement?> task = query();

			var result = await task;

			while (!satisfactory(result))
			{
				long elapsed = DateTime.Now.Subtract(start).Ticks;
				if (elapsed >= timeout.Value.Ticks)
				{
					Debug.WriteLine($">>>>> {elapsed} ticks elapsed, timeout value is {timeout.Value.Ticks}");

					throw new TimeoutException(timeoutMessage);
				}

				Task.Delay(retryFrequency.Value.Milliseconds).Wait();
			
				task = query();
				result = await task;
			}

			return result!;
		}

		static async Task<IUIElement?> FindElementWithAI(this IApp app, string prompt)
		{
			var screenshot = app.Screenshot() ?? throw new InvalidOperationException("Failed to get screenshot");
			var screenshotBytes = BinaryData.FromBytes(screenshot);
			var element = await FindElementWithAI(app, screenshotBytes, prompt);

			return element;
		}

		static async Task<IUIElement?> FindElementWithAI(this IApp app, BinaryData data, string prompt)
		{
			string endpoint = "AZURE_OPENAI_ENDPOINT";
			string key = "AZURE_OPENAI_API_KEY";
			string modelName = "AZURE_OPENAI_MODEL_NAME";

			AzureKeyCredential credential = new AzureKeyCredential(key);
			AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
			ChatClient chatClient = azureClient.GetChatClient(modelName);

			var messages = new List<ChatMessage>
			{
				new SystemChatMessage(FindElementImagePrompt),
				new UserChatMessage(
					ChatMessageContentPart.CreateTextPart(prompt),
					ChatMessageContentPart.CreateImagePart(data, "image/png"))
			};

			var options = new ChatCompletionOptions
			{
				Temperature = (float)0.7,
				MaxOutputTokenCount = 800,
				FrequencyPenalty = 0,
				PresencePenalty = 0,
			};


			ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

			if (completion.Content != null && completion.Content.Count > 0)
			{
				string result = completion.Content[0].Text;
				IUElementData? elementData = JsonSerializer.Deserialize<IUElementData>(SanitizeResponse(result));

				if (elementData is not null)
					return app.FindElement(elementData);
			}

			return null;
		}

		internal static IUIElement? FindElement(this IApp app, IUElementData elementData)
		{
			if (app is not AppiumApp appiumApp)
				return null;

			IUIElement? result = null;

			if (elementData is not null)
			{
				result = AppiumQuery.ByXPath("//*[@x='" + elementData.X + "' and @y='" + elementData.Y + "']").FindElement(appiumApp);
				if (result is null && elementData.Text is not null)
				{
					string text = elementData.Text;
					// Android (text), iOS (label), Windows (Name)
					result = AppiumQuery.ByXPath("//*[@text='" + text + "' or @label='" + text + "' or @Name='" + text + "']").FindElement(appiumApp);
				}
			}

			return result;
		}

		static string SanitizeResponse(string input)
		{
			string output = input;

			int start = output.IndexOf("{");
			output = output.Substring(start);

			int end = output.LastIndexOf('}');
			output = output.Substring(0, end + 1);

			return output;
		}
	}
}

class IUElementData
{
	[JsonPropertyName("x")]
	public int X { get; set; }
	[JsonPropertyName("y")]
	public int Y { get; set; }
	[JsonPropertyName("width")]
	public int Width { get; set; }
	[JsonPropertyName("height")]
	public int Height { get; set; }
	[JsonPropertyName("text")]
	public string? Text { get; set; }
}

class VerifyScreenshotData
{
	[JsonPropertyName("equals")]
	public bool AreEquals { get; set; }
}