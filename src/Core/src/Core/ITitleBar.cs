namespace Microsoft.Maui
{
	/// <summary>
	/// Titlebar for the Window
	/// </summary>
	public interface ITitleBar : IView
	{
		IView? Content { get; }

		IView? LeadingContent { get; }
		IView? TrailingContent { get; }

		string? Title { get; }

		string? Subtitle { get; }
	}
}
