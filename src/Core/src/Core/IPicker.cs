#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View for selecting a text item from a list of data.
	/// </summary>
	public interface IPicker : IView, ITextStyle, ITextAlignment, IItemDelegate<string>
	{
		/// <summary>
		/// Gets the title for the Picker.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets the index of the selected item of the picker.
		/// </summary>
		int SelectedIndex { get; set; }
	}
}