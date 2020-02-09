using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.DualScreen
{
	public class CompactModeArgs : EventArgs
	{
		Func<Task> _close;
		public CompactModeArgs(Func<Task> close, bool success)
		{
			if(close == null)
			{
				close = () => Task.Delay(0);
			}

			_close = close;
			Success = success;
		}

		public Task CloseAsync() => _close();
		public bool Success { get; }
	}
}
