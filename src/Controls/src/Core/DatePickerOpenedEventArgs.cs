using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a DatePicker control is opened.
/// </summary>
public class DatePickerOpenedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of DatePickerOpenedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of DatePickerOpenedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly DatePickerOpenedEventArgs Empty = new();
}