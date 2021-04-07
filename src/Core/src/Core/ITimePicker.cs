using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that allows the user to select a time.
	/// </summary>
	public interface ITimePicker : IView, ITextStyle
	{
		/// <summary>
		/// The format of the time to display to the user.
		/// </summary>
		string Format { get; }

		/// <summary>
		/// Gets the displayed time.
		/// </summary>
		TimeSpan Time { get; set; }
	}
}