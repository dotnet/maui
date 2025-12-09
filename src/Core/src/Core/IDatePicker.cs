using System;

namespace Microsoft.Maui;

/// <summary>
/// Represents a <see cref="IView"/> that allows the user to select a date.
/// </summary>
public interface IDatePicker : IView, ITextStyle
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
	/// Gets the format of the date to display to the user. 
	/// </summary>
	string Format { get; set; }

	/// <summary>
	/// Gets the displayed date.
	/// </summary>
	DateTime? Date { get; set; }

	/// <summary>
	/// Gets the minimum selectable <see cref="DateTime"/>.
	/// </summary>
	DateTime? MinimumDate { get; }

	/// <summary>
	/// Gets the maximum selectable <see cref="DateTime"/>.
	/// </summary>
	DateTime? MaximumDate { get; }
}