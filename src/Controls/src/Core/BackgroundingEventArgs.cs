#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public class BackgroundingEventArgs : EventArgs
	{
		public BackgroundingEventArgs(IPersistedState state)
		{
			State = state;
		}

		public IPersistedState State { get; set; }
	}
}