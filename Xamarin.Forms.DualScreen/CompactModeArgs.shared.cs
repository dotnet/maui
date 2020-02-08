using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.DualScreen
{
	public class CompactModeArgs : EventArgs
	{
		public CompactModeArgs(Func<Task> close, bool success)
		{
			if(close == null)
			{
				close = () => Task.Delay(0);
			}

			Close = close;
			Success = success;
		}

		public Func<Task> Close { get; }
		public bool Success { get; }
	}
}
