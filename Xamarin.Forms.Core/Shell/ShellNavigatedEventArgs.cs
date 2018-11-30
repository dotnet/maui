using System;

namespace Xamarin.Forms
{
	public class ShellNavigatedEventArgs : EventArgs
	{
		public ShellNavigatedEventArgs(ShellNavigationState previous, ShellNavigationState current, ShellNavigationSource source)
		{
			Previous = previous;
			Current = current;
			Source = source;
		}

		public ShellNavigationState Current { get; }
		public ShellNavigationState Previous { get; }
		public ShellNavigationSource Source { get; }
	}
}