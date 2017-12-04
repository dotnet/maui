using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Interface of the controls which can measure their size taking into
	/// account the available area.
	/// </summary>
	public interface IMeasurable
	{
		/// <summary>
		/// Measures the size of the control in order to fit it into the
		/// available area.
		/// </summary>
		/// <param name="availableWidth">Available width.</param>
		/// <param name="availableHeight">Available height.</param>
		/// <returns>Size of the control that fits the available area.</returns>
		ESize Measure(int availableWidth, int availableHeight);
	}
}
