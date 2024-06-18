using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Titlebar for the Window
	/// </summary>
	public interface ITitleBar : IView
	{
		IList<IView> PassthroughElements { get; }

		string? Title { get; }

		string? Subtitle { get; }
	}
}
