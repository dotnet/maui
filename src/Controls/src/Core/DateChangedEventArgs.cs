using System;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Event arguments for <see cref="DatePicker.DateSelected" /> event.
/// </summary>
public class DateChangedEventArgs : EventArgs
{
	/// <summary>
	/// Creates a new <see cref="DateChangedEventArgs" /> object that represents a change from <paramref name="oldDate" /> to <paramref name="newDate" />
	/// </summary>
	/// <param name="oldDate">The old date value.</param>
	/// <param name="newDate">The new date value.</param>
	public DateChangedEventArgs(DateTime? oldDate, DateTime? newDate)
	{
		OldDate = oldDate;
		NewDate = newDate;
	}

	/// <summary>
	/// The date that the user entered.
	/// </summary>
	public DateTime? NewDate { get; private set; }

	/// <summary>
	/// The date that was on the element at the time that the user selected it.
	/// </summary>
	public DateTime? OldDate { get; private set; }
}