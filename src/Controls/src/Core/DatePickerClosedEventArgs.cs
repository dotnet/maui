using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a DatePicker control is closed.
/// </summary>
public class DatePickerClosedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of DatePickerClosedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of DatePickerClosedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly DatePickerClosedEventArgs Empty = new();
}