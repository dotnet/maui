using System;

namespace Microsoft.Maui;

/// <summary>
/// Represents a <see cref="IView"/> that allows the user to select a date.
/// </summary>
public interface IDatePicker : IView, ITextStyle
{
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