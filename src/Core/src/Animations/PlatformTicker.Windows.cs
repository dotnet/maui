using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
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