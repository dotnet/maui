using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that provides a toggled value.
	/// </summary>
	public interface IRadioButton : IView
	{
		/// <summary>
		/// Gets or sets a Boolean value that indicates whether this RadioButton is checked.
		/// </summary>
		bool IsChecked { get; set; }
	}
}