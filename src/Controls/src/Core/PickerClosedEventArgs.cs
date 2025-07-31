using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a Picker control is closed.
/// </summary>
public class PickerClosedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of PickerClosedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of PickerClosedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly PickerClosedEventArgs Empty = new();
}