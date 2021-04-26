namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that contain a single child, with some framing options.
	/// </summary>
	public interface IFrame : IBorder
	{
		/// <summary>
		/// The child of the Frame.
		/// </summary>
		IView? Content { get; }

		/// <summary>
		/// Gets a flag indicating if the Frame has a shadow displayed.
		/// </summary>
		bool HasShadow { get; }
	}
}