namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Provides a way to automatically size a CALayer to its super layer.
	/// </summary>
	public interface IAutoSizableCALayer
	{
		/// <summary>
		/// Automatically sizes the CALayer to its super layer.
		/// </summary>
		void AutoSizeToSuperLayer();
	}
}