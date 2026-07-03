using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maui.Controls.Sample.ViewModels;

public enum ChatBubbleType
{
	User,
	Assistant,
	ToolCall,
	ToolResult
}

public partial class ChatBubbleViewModel : ObservableObject
{
	public ChatBubbleType BubbleType { get; init; }

	[ObservableProperty]
	public partial string Text { get; set; } = string.Empty;

	[ObservableProperty]
	public partial bool IsStreaming { get; set; }

	[ObservableProperty]
	public partial bool IsExpanded { get; set; }

	public string? ToolName { get; init; }

	public string? DetailText { get; init; }

	[RelayCommand]
	void ToggleExpanded() => IsExpanded = !IsExpanded;
}
