namespace Maui.Controls.Sample.ViewModels;

public class MessageViewModel
{
	public string Text { get; set; } = string.Empty;

	public bool IsUser { get; set; }

	public Color BackgroundColor => IsUser 
		? Color.FromArgb("#007AFF") 
		: Color.FromArgb("#E5E5EA");

	public Color TextColor => IsUser 
		? Colors.White 
		: Colors.Black;

	public LayoutOptions HorizontalAlignment => IsUser 
		? LayoutOptions.End 
		: LayoutOptions.Start;
}
