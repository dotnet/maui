using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
		bool _isRunning;

		/// <inheritdoc/>
		public override bool IsRunning => _isRunning;

		/// <inheritdoc/>
		public override void Start()
		{
			if (_isRunning)
			{
				return;
			}

			_isRunning = true;
			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			if (!_isRunning)
			{
				return;
			}

			_isRunning = false;
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		void RenderingFrameEventHandler(object? sender, object? args)
		{
			Fire?.Invoke();
		}
	}
}