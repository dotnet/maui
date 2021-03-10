namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that provides a toggled value.
	/// </summary>
	public interface ISwitch : IView
	{
		/// <summary>
		/// Gets or sets a Boolean value that indicates whether this Switch is toggled.
		/// </summary>
		bool IsToggled { get; set; }

		/// <summary>
		/// Gets the Switch Track Color.
		/// </summary>
		Color TrackColor { get; }

		/// <summary>
		/// Gets the Switch Thumb Color.
		/// </summary>
		Color ThumbColor { get; }
	}
}