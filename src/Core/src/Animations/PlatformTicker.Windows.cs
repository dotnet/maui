using Microsoft.UI.Xaml.Media;
using ViewManagement = Windows.UI.ViewManagement;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
		readonly ViewManagement.UISettings _uiSettings = new();

		// Always read the live system value so AnimationManager.Add() and OnFire() both
		// see the current "Show animations in Windows" (Ease of Access) setting without caching.
		/// <inheritdoc/>
		public override bool SystemEnabled => _uiSettings.AnimationsEnabled;

		/// <inheritdoc/>
		public override void Start()
		{
			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		void RenderingFrameEventHandler(object? sender, object? args)
		{
			Fire?.Invoke();
		}
	}
}