using Microsoft.Extensions.AI;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.ViewModels;

public class ChatViewModel(IChatClient chatClient) : BindableObject
{
	private readonly List<ChatMessage> _chatHistory = [];

	public ObservableCollection<MessageViewModel> Messages { get; } = [
		new() {
        	Text = "Hello! I'm an AI assistant. How can I help you today?",
			IsUser = false
    	}
	];

	public string? MessageText
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				OnPropertyChanged();
			}
		}
	}

	public Command SendMessageCommand =>
		field ?? new Command(async () => await SendMessageAsync(), () => !string.IsNullOrWhiteSpace(MessageText));

	private async Task SendMessageAsync()
	{
		if (string.IsNullOrWhiteSpace(MessageText))
			return;

		var userMessage = MessageText;
		MessageText = string.Empty;

		// Add user message
		Messages.Add(new MessageViewModel
		{
			Text = userMessage,
			IsUser = true
		});

		_chatHistory.Add(new ChatMessage(ChatRole.User, userMessage));

		try
		{
			// Get AI response
			var response = await chatClient.GetResponseAsync(_chatHistory, cancellationToken: default);
			
			// Extract the assistant message from the response
			var assistantMessageText = response.ToString() ?? "(No response)";
			var assistantMessage = new ChatMessage(ChatRole.Assistant, assistantMessageText);
			_chatHistory.Add(assistantMessage);

			// Add AI response
			Messages.Add(new MessageViewModel
			{
				Text = assistantMessageText,
				IsUser = false
			});
		}
		catch (Exception ex)
		{
			Messages.Add(new MessageViewModel
			{
				Text = $"Error: {ex.Message}",
				IsUser = false
			});
		}
	}
}
