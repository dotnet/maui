using System;

namespace Microsoft.Maui;

/// <summary>
/// Represents a <see cref="IView"/> that allows the user to select a time.
/// </summary>
public interface ITimePicker : IView, ITextStyle
{
	/// <summary>
	/// The format of the time to display to the user.
	/// </summary>
	string Format { get; }

	/// <summary>
	/// Gets or sets the selected time.
	/// </summary>
	TimeSpan? Time { get; set; }
}