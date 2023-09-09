using System.Threading.Tasks;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class AsyncTicker : Ticker
	{
		bool _running;

		public override bool IsRunning => _running; 

		public void SetEnabled(bool enabled)
		{
			SystemEnabled = enabled;

			if (!enabled)
			{
				Stop();
			}
		}

		public override async void Start()
		{
			if (SystemEnabled)
			{
				_running = true;
			}

			while (IsRunning && SystemEnabled)
			{
				Fire?.Invoke();
				await Task.Delay(16);
			}
		}

		public override void Stop()
		{
			_running = false;
		}
	}
}