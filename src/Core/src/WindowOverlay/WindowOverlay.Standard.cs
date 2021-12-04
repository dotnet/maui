namespace Microsoft.Maui
{
	public partial class WindowOverlay
	{
		object? _graphicsView = null;

		/// <inheritdoc/>
		public void Invalidate()
		{
		}

		/// <inheritdoc/>
		public virtual bool Initialize()
		{
			return IsNativeViewInitialized = true;
		}

		/// <summary>
		/// Deinitializes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		void DeinitializeNativeDependencies()
		{
			IsNativeViewInitialized = false;
		}
	}
}