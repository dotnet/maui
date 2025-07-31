using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides event data for the event that is raised when a Picker control is opened.
/// </summary>
public class PickerOpenedEventArgs : EventArgs
{
	/// <summary>
	/// Gets a reusable empty instance of PickerOpenedEventArgs.
	/// </summary>
	/// <value>
	/// A static readonly instance of PickerOpenedEventArgs that can be reused to avoid unnecessary object allocation.
	/// </value>
	internal new static readonly PickerOpenedEventArgs Empty = new();
}