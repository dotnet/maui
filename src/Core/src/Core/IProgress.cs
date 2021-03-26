namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that show progress as a horizontal bar that is filled to a percentage 
	/// represented by a float value.
	/// </summary>
	public interface IProgress : IView
	{
		/// <summary>
		/// Is a value that represents the current progress as a value from 0 to 1. 
		/// Progress values less than 0 will be clamped to 0, values greater than 1 will be clamped to 1.
		/// </summary>
		double Progress { get; }

		/// <summary>
		/// Get the color of the progress bar.
		/// </summary>
		Color ProgressColor { get; }
	}
}