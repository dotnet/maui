using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that allows the user to select a date.
	/// </summary>
	public interface IDatePicker : IView
	{
		/// <summary>
		/// Gets the format of the date to display to the user. 
		/// </summary>
		string Format { get; set; }

		/// <summary>
		/// Gets the displayed date.
		/// </summary>
		DateTime Date { get; set; }

		/// <summary>
		/// Gets the minimum DateTime selectable.
		/// </summary>
		DateTime MinimumDate { get; }

		/// <summary>
		/// Gets the maximum DateTime selectable.
		/// </summary>
		DateTime MaximumDate { get; }
	}
}