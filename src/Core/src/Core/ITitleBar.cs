using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Title bar control
	/// </summary>
	public interface ITitleBar : IView, IContentView
	{
		/// <summary>
		/// Gets a list of elements that should prevent dragging in the title
		/// bar region and instead handle input directly
		/// </summary>
		IList<IView> PassthroughElements { get; }

		/// <summary>
		/// Gets the title text of the title bar. The title usually specifies
		/// the name of the application or indicates the purpose of the window
		/// </summary>
		string? Title { get; }

		/// <summary>
		/// Gets the subtitle text of the title bar. The subtitle usually specifies
		/// the secondary information about the application or window
		/// </summary>
		string? Subtitle { get; }
	}
}
