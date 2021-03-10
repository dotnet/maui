namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays an animation to show that the application is engaged
	/// in a lengthy activity.
	/// </summary>
	public interface IActivityIndicator : IView
	{
		/// <summary>
		/// Gets a value that indicates whether the ActivityIndicator should be visible and animating,
		/// or hidden.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Gets a Color value that defines the display color.
		/// </summary>
		Color Color { get; }
	}
}