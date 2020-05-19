using System;

namespace System.Maui
{
	public class ShellNavigatingEventArgs : EventArgs
	{
		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
		{
			Current = current;
			Target = target;
			Source = source;
			CanCancel = canCancel;
		}

		public ShellNavigationState Current { get; }

		public ShellNavigationState Target { get; }

		public ShellNavigationSource Source { get; }

		public bool CanCancel { get; }

		public bool Cancel()
		{
			if (!CanCancel)
				return false;
			Cancelled = true;
			return true;
		}

		public bool Cancelled { get; private set; }
	}
}