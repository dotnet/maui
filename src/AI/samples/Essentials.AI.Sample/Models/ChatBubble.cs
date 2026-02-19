using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Models;

public enum ChatBubbleType
{
	User,
	Assistant,
	ToolCall,
	ToolResult
}

public class ChatBubble : INotifyPropertyChanged
{
	string _text = string.Empty;
	bool _isStreaming;
	bool _isExpanded;

	public ChatBubble()
	{
		ToggleExpandedCommand = new Command(() => IsExpanded = !IsExpanded);
	}

	public ChatBubbleType BubbleType { get; init; }

	public string Text
	{
		get => _text;
		set => SetProperty(ref _text, value);
	}

	public bool IsStreaming
	{
		get => _isStreaming;
		set => SetProperty(ref _isStreaming, value);
	}

	public bool IsExpanded
	{
		get => _isExpanded;
		set => SetProperty(ref _isExpanded, value);
	}

	public string? ToolName { get; init; }

	public string? DetailText { get; init; }

	public ICommand ToggleExpandedCommand { get; }

	public event PropertyChangedEventHandler? PropertyChanged;

	void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
