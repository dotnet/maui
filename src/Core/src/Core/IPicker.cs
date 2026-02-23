#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View for selecting a text item from a list of data.
	/// </summary>
	public interface IPicker : IView, ITextStyle, ITextAlignment, IItemDelegate<string>
	{
		/// <summary>
		/// Gets or sets a value indicating whether the dropdown is currently open.
		/// </summary>
		/// <value>
		/// <c>true</c> if the dropdown is currently open and visible; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Setting this property programmatically will open or close the dropdown.
		/// Controls may also update this property when the dropdown is opened or closed through user interaction.
		/// </remarks>
#if NETSTANDARD2_0
		bool IsOpen { get; set; }
#else
		bool IsOpen { get => false; set { } }
#endif

		/// <summary>
		/// Gets the list of choices.
		/// </summary>
		IList<string> Items { get; }

		/// <summary>
		/// Gets the title for the Picker.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Gets the color for the Picker title.
		/// </summary>
		Color TitleColor { get; }

		/// <summary>
		/// Gets the index of the selected item of the picker.
		/// </summary>
		int SelectedIndex { get; set; }
	}
}