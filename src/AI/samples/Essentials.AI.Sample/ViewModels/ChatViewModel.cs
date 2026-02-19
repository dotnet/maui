using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.ViewModels;

public partial class ChatViewModel : ObservableObject
{
	readonly ChatService _chatService;
	readonly IDispatcher _dispatcher;
	readonly List<ChatMessage> _conversationHistory = [];
	CancellationTokenSource? _cts;

	public ChatViewModel(ChatService chatService, IDispatcher dispatcher)
	{
		_chatService = chatService;
		_dispatcher = dispatcher;
	}

	public ChatService ChatService => _chatService;

	public ObservableCollection<ChatBubble> Messages { get; } = [];

	[ObservableProperty]
	public partial string MessageText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial bool IsOverlayVisible { get; set; }

	[ObservableProperty]
	public partial bool IsSending { get; set; }

	public Command SendMessageCommand => field ??= new Command(
		async () => await SendMessageAsync(),
		() => !string.IsNullOrWhiteSpace(MessageText) && !IsSending);

	public Command ToggleOverlayCommand => field ??= new Command(
		() => IsOverlayVisible = !IsOverlayVisible);

	public Command NewChatCommand => field ??= new Command(NewChat);

	void NewChat()
	{
		_cts?.Cancel();
		Messages.Clear();
		_conversationHistory.Clear();
		MessageText = string.Empty;
		IsSending = false;
	}

	partial void OnMessageTextChanged(string value)
	{
		SendMessageCommand.ChangeCanExecute();
	}

	partial void OnIsSendingChanged(bool value)
	{
		SendMessageCommand.ChangeCanExecute();
	}

	async Task SendMessageAsync()
	{
		var userText = MessageText.Trim();
		if (string.IsNullOrEmpty(userText))
			return;

		IsSending = true;
		MessageText = string.Empty;

		// Add user bubble
		Messages.Add(new ChatBubble
		{
			BubbleType = ChatBubbleType.User,
			Text = userText
		});

		// Add to conversation history
		_conversationHistory.Add(new ChatMessage(ChatRole.User, userText));

		// Prepare streaming
		_cts = new CancellationTokenSource();
		ChatBubble? assistantBubble = null;
		var allContents = new List<AIContent>();
		var seenCallIds = new HashSet<string>();

		// Show "Thinking..." placeholder immediately
		var thinkingBubble = new ChatBubble
		{
			BubbleType = ChatBubbleType.Assistant,
			Text = "Thinking...",
			IsStreaming = true
		};
		Messages.Add(thinkingBubble);

		try
		{
			await foreach (var update in _chatService.GetStreamingResponseAsync(
				_conversationHistory, _cts.Token))
			{
				foreach (var content in update.Contents)
				{
					allContents.Add(content);

					switch (content)
					{
						case TextContent textContent when !string.IsNullOrEmpty(textContent.Text):
							_dispatcher.Dispatch(() =>
							{
								if (assistantBubble is null)
								{
									// First text: if thinking bubble is still in Messages, reuse it
									if (Messages.Contains(thinkingBubble))
									{
										thinkingBubble.Text = textContent.Text;
										assistantBubble = thinkingBubble;
									}
									else
									{
										// Post-tool-call: create a new assistant bubble
										assistantBubble = new ChatBubble
										{
											BubbleType = ChatBubbleType.Assistant,
											Text = textContent.Text,
											IsStreaming = true
										};
										Messages.Add(assistantBubble);
									}
									return;
								}
								assistantBubble.Text += textContent.Text;
							});
							break;

						case FunctionCallContent functionCall:
							if (!seenCallIds.Add($"call:{functionCall.CallId}"))
								break; // Skip duplicate
							_dispatcher.Dispatch(() =>
							{
								if (assistantBubble is not null)
								{
									// Remove empty/junk assistant bubbles created before the tool call
									var trimmed = assistantBubble.Text.Trim();
									if (string.IsNullOrEmpty(trimmed) || trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
										Messages.Remove(assistantBubble);
									else
										assistantBubble.IsStreaming = false;
									assistantBubble = null;
								}
								else
								{
									// Remove thinking bubble if tool call arrives before any text
									Messages.Remove(thinkingBubble);
								}

								var argsJson = functionCall.Arguments is not null
									? JsonSerializer.Serialize(functionCall.Arguments, new JsonSerializerOptions { WriteIndented = true })
									: "{}";

								Messages.Add(new ChatBubble
								{
									BubbleType = ChatBubbleType.ToolCall,
									Text = $"🔧 Called {functionCall.Name}",
									ToolName = functionCall.Name,
									DetailText = argsJson
								});
							});
							break;

						case FunctionResultContent functionResult:
							if (!seenCallIds.Add($"result:{functionResult.CallId}"))
								break; // Skip duplicate
							_dispatcher.Dispatch(() =>
							{
								var resultText = functionResult.Result?.ToString() ?? "(no result)";
								var toolName = functionResult.CallId ?? "tool";

								Messages.Add(new ChatBubble
								{
									BubbleType = ChatBubbleType.ToolResult,
									Text = $"📋 {toolName} responded",
									ToolName = toolName,
									DetailText = resultText
								});
							});
							break;
					}
				}
			}

			_dispatcher.Dispatch(() =>
			{
				if (assistantBubble is { IsStreaming: true })
					assistantBubble.IsStreaming = false;
			});

			// Add full assistant response to history for multi-turn context
			// Include tool interactions so the model's transcript preserves the complete chain:
			//   Assistant → [FunctionCallContent]  (tool call decisions)
			//   Tool      → [FunctionResultContent] (tool results)
			//   Assistant → [TextContent]           (final text response)
			var functionCalls = allContents.OfType<FunctionCallContent>().ToArray();
			if (functionCalls.Length > 0)
				_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, functionCalls));

			var functionResults = allContents.OfType<FunctionResultContent>().ToArray();
			if (functionResults.Length > 0)
				_conversationHistory.Add(new ChatMessage(ChatRole.Tool, functionResults));

			var textContents = allContents.OfType<TextContent>().Where(t => !string.IsNullOrEmpty(t.Text)).ToArray();
			if (textContents.Length > 0)
				_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, textContents));
		}
		catch (OperationCanceledException)
		{
			// User cancelled
		}
		catch (Exception ex)
		{
			_dispatcher.Dispatch(() =>
			{
				Messages.Add(new ChatBubble
				{
					BubbleType = ChatBubbleType.Assistant,
					Text = $"⚠️ Error: {ex.Message}"
				});
			});
		}
		finally
		{
			IsSending = false;
			_cts = null;
		}
	}
}
