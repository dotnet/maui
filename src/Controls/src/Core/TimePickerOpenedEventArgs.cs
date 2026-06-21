using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a TimePicker control is opened.
/// </summary>
public class TimePickerOpenedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of TimePickerOpenedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of TimePickerOpenedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly TimePickerOpenedEventArgs Empty = new();
}