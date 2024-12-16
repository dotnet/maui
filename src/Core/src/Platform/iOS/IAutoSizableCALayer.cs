namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Provides a way to automatically size a CALayer to its super layer.
	/// </summary>
	// TODO: When we're good with this solution, we should make this public with NET10.
	// We may also evaluate to make CALayerAutosizeObserver public to offer a solution for third-party developers.
	internal interface IAutoSizableCALayer
	{
		/// <summary>
		/// Automatically sizes the CALayer to its super layer.
		/// </summary>
		void AutoSizeToSuperLayer();
	}
}