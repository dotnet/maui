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
			return IsPlatformViewInitialized = true;
		}

		/// <summary>
		/// Deinitializes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		void DeinitializePlatformDependencies()
		{
			IsPlatformViewInitialized = false;
		}
	}
}