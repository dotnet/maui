using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a top-level menu in a MenuBar view.
	/// </summary>
	public interface IMenuBarItem : IList<IMenuElement>, IElement
	{
		string Text { get; }

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }
	}
}
