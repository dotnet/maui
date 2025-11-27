namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Provides a way to automatically size a CALayer to its super layer.
	/// </summary>
	// We may also evaluate to make CALayerAutosizeObserver public to offer a solution for third-party developers.
	public interface IAutoSizableCALayer
	{
		/// <summary>
		/// Automatically sizes the CALayer to its super layer.
		/// </summary>
		void AutoSizeToSuperLayer();
	}
}