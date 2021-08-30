namespace Microsoft.Maui
{
	/// <summary>
	/// Define how the Shape outline is painted on Layouts.
	/// </summary>
	public interface IBorderStroke
	{
		/// <summary>
		/// Gets the data that specifies how the Shape outline is painted.
		/// </summary>
		BorderStroke? BorderStroke { get; }
	}
}