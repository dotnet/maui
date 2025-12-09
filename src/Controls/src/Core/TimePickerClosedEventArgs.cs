using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a TimePicker control is closed.
/// </summary>
public class TimePickerClosedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of TimePickerClosedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of TimePickerClosedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly TimePickerClosedEventArgs Empty = new();
}