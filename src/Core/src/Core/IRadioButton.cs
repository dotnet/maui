using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that provides a toggled value.
	/// </summary>
	public interface IRadioButton : IView, ITextStyle, IContentView
	{
		/// <summary>
		/// Gets or sets a Boolean value that indicates whether this RadioButton is checked.
		/// </summary>
		bool IsChecked { get; set; }

		/// <summary>
		/// Defines the border stroke color.
		/// </summary>
		Color BorderColor { get; }

		/// <summary>
		///  Defines the width of the RadioButton border.
		/// </summary>
		double BorderWidth { get; }

		/// <summary>
		/// Defines the corner radius of the RadioButton.
		/// </summary>
		int CornerRadius { get; }
	}
}