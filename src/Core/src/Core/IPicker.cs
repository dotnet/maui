#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View for selecting a text item from a list of data.
	/// </summary>
	public interface IPicker : IView, ITextStyle, ITextAlignment
	{
		/// <summary>
		/// Gets the title for the Picker.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets the count for the number of items in the picker
		/// </summary>
		int GetCount();

		/// <summary>
		/// Gets the Display String for the index
		/// </summary>
		string DisplayFor(int index);

		/// <summary>
		/// Gets the index of the selected item of the picker.
		/// </summary>
		int SelectedIndex { get; set; }
	}
}