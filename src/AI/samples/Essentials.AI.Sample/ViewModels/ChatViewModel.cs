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
	readonly List<ChatMessage> _conversationHistory = [];
	CancellationTokenSource? _cts;

	public ChatViewModel(ChatService chatService)
	{
		_chatService = chatService;
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
		_cts?.Dispose();
		_cts = null;
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
		ChatMessage? textMessage = null; // Accumulates text deltas into one assistant message
		var seenCallIds = new HashSet<string>();
		var callIdToToolName = new Dictionary<string, string>(); // Cache tool names for result bubbles

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

							// Update UI (already on UI thread via await foreach)
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
							}
							else
							{
								assistantBubble.Text += textContent.Text;
							}
							break;

						case FunctionCallContent functionCall:
							if (!seenCallIds.Add($"call:{functionCall.CallId}"))
								break;

							// Cache tool name for the result bubble
							if (functionCall.CallId is not null)
								callIdToToolName[functionCall.CallId] = functionCall.Name;

							// Add to history immediately, preserving stream order
							_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, [functionCall]));
							textMessage = null; // Next text starts a new message

							// Update UI: always remove pre-tool bubble (thinking or partial text)
							if (assistantBubble is not null)
							{
								Messages.Remove(assistantBubble);
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
							break;

						case FunctionResultContent functionResult:
							if (!seenCallIds.Add($"result:{functionResult.CallId}"))
								break;

							// Add to history immediately, preserving stream order
							_conversationHistory.Add(new ChatMessage(ChatRole.Tool, [functionResult]));

							// Update UI — use cached tool name instead of raw CallId
							var resultText = functionResult.Result?.ToString() ?? "(no result)";
							var toolName = (functionResult.CallId is not null &&
								callIdToToolName.TryGetValue(functionResult.CallId, out var cachedName))
								? cachedName
								: "tool";

							Messages.Add(new ChatBubble
							{
								BubbleType = ChatBubbleType.ToolResult,
								Text = $"📋 {toolName} responded",
								ToolName = toolName,
								DetailText = resultText
							});
							break;
					}
				}
			}

			if (assistantBubble is { IsStreaming: true })
				assistantBubble.IsStreaming = false;
		}
		catch (OperationCanceledException)
		{
			// User cancelled
		}
		catch (Exception ex)
		{
			Messages.Add(new ChatBubble
			{
				BubbleType = ChatBubbleType.Assistant,
				Text = $"⚠️ Error: {ex.Message}"
			});
		}
		finally
		{
			// Clean up thinking bubble if it was never replaced
			if (assistantBubble is null && Messages.Contains(thinkingBubble))
				Messages.Remove(thinkingBubble);

			// Stop streaming indicator on any remaining bubble
			if (assistantBubble is { IsStreaming: true })
				assistantBubble.IsStreaming = false;

			IsSending = false;
			_cts?.Dispose();
			_cts = null;
		}
	}
}
