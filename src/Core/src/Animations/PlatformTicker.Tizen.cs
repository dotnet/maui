using System.Threading;
using Tizen.Applications;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
		readonly Timer _timer;
		readonly SynchronizationContext? _context;
		bool _isRunning;

		/// <inheritdoc/>
		public override bool IsRunning => _isRunning;

		public PlatformTicker()
		{
			if (SynchronizationContext.Current == null)
			{
				TizenSynchronizationContext.Initialize();
			}

			_context = SynchronizationContext.Current;
			_timer = new Timer((object? o) => HandleElapsed(o), this, Timeout.Infinite, Timeout.Infinite);
		}

		/// <inheritdoc/>
		public override void Start()
		{
			_timer.Change(16, 16);
			_isRunning = true;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			_timer.Change(-1, -1);
			_isRunning = false;
		}

		void HandleElapsed(object? state)
		{
			_context?.Post((o) => Fire?.Invoke(), null);
		}
	}
}