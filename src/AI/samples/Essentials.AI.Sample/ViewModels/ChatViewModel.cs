using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Maui.Controls.Sample.ViewModels;

public partial class ChatViewModel : ObservableObject
{
	readonly ChatService _chatService;
	readonly IDispatcher _dispatcher;
	readonly ILogger<ChatViewModel> _logger;
	readonly List<ChatMessage> _conversationHistory = [];
	CancellationTokenSource? _cts;

	public ChatViewModel(ChatService chatService, IDispatcher dispatcher, ILogger<ChatViewModel> logger)
	{
		_chatService = chatService;
		_dispatcher = dispatcher;
		_logger = logger;
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

		_logger.LogDebug($"[Chat] === Sending {_conversationHistory.Count} messages ===");
		foreach (var msg in _conversationHistory)
		{
			var contentSummary = string.Join(", ", msg.Contents.Select(c => c switch
			{
				TextContent tc => $"Text({tc.Text?.Length ?? 0} chars)",
				FunctionCallContent fc => $"Call({fc.Name}, id={fc.CallId})",
				FunctionResultContent fr => $"Result(id={fr.CallId})",
				_ => c.GetType().Name
			}));
			_logger.LogDebug($"[Chat]   {msg.Role}: [{contentSummary}]");
		}

		// Prepare streaming
		_cts = new CancellationTokenSource();
		ChatBubble? assistantBubble = null;
		ChatMessage? textMessage = null; // Accumulates text deltas into one assistant message
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
					_logger.LogDebug($"[Chat] ← {content switch
					{
						TextContent tc => $"Text: \"{tc.Text?[..Math.Min(tc.Text?.Length ?? 0, 80)]}\"",
						FunctionCallContent fc => $"FunctionCall: {fc.Name} (id={fc.CallId})",
						FunctionResultContent fr => $"FunctionResult: (id={fr.CallId}) {fr.Result?.ToString()?[..Math.Min(fr.Result?.ToString()?.Length ?? 0, 80)]}",
						_ => content.GetType().Name
					}}");
					switch (content)
					{
						case TextContent textContent when !string.IsNullOrEmpty(textContent.Text):
							// Add to history: accumulate all text deltas into one assistant message
							if (textMessage is null)
							{
								textMessage = new ChatMessage(ChatRole.Assistant, [textContent]);
								_conversationHistory.Add(textMessage);
							}
							else
							{
								textMessage.Contents.Add(textContent);
							}

							// Update UI
							_dispatcher.Dispatch(() =>
							{
								if (assistantBubble is null)
								{
									if (Messages.Contains(thinkingBubble))
									{
										thinkingBubble.Text = textContent.Text;
										assistantBubble = thinkingBubble;
									}
									else
									{
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
								break;

							// Add to history immediately, preserving stream order
							_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, [functionCall]));
							textMessage = null; // Next text starts a new message

							// Update UI
							_dispatcher.Dispatch(() =>
							{
								if (assistantBubble is not null)
								{
									var trimmed = assistantBubble.Text.Trim();
									if (string.IsNullOrEmpty(trimmed))
										Messages.Remove(assistantBubble);
									else
										assistantBubble.IsStreaming = false;
									assistantBubble = null;
								}
								else
								{
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
								break;

							// Add to history immediately, preserving stream order
							_conversationHistory.Add(new ChatMessage(ChatRole.Tool, [functionResult]));

							// Update UI
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

			_logger.LogDebug($"[Chat] === Stream complete. History now has {_conversationHistory.Count} messages ===");
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
