using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View which allows the user to select a binary choice.
	/// </summary>
	public interface ICheckBox : IView
	{
		/// <summary>
		/// Gets a value that indicates whether the CheckBox is checked.
		/// </summary>
		bool IsChecked { get; set; }

		/// <summary>
		/// Gets the CheckBox Foreground Paint.
		/// </summary>
		Paint? Foreground { get; }
	}
}