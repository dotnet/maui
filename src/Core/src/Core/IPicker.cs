using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View for selecting a text item from a list of data.
	/// </summary>
	public interface IPicker : IView
	{
		/// <summary>
		/// Gets the title for the Picker.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets or sets the internal list of items to template and display.
		/// </summary>
		IList<string> Items { get; }

		/// <summary>
		/// Gets or sets the source list of items to template and display.
		/// </summary>
		IList ItemsSource { get; }

		/// <summary>
		/// Gets the index of the selected item of the picker.
		/// </summary>
		int SelectedIndex { get; set; }

		/// <summary>
		/// Gets the selected item.
		/// </summary>
		object? SelectedItem { get; set; }

		/// <summary>
		/// Gets the character spacing.
		/// </summary>
		double CharacterSpacing { get; set; }
	}
}