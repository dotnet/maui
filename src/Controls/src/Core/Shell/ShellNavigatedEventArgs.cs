#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides data for the <see cref="Shell.Navigated"/> event.</summary>
	public class ShellNavigatedEventArgs : EventArgs
	{
		/// <summary>Initializes a new instance of <see cref="ShellNavigatedEventArgs"/>.</summary>
		/// <param name="previous">The navigation state before the navigation occurred.</param>
		/// <param name="current">The navigation state after the navigation occurred.</param>
		/// <param name="source">The source of the navigation event.</param>
		public ShellNavigatedEventArgs(ShellNavigationState previous, ShellNavigationState current, ShellNavigationSource source)
		{
			Previous = previous;
			Current = current;
			Source = source;
		}

		/// <summary>Gets the navigation state after the navigation occurred.</summary>
		public ShellNavigationState Current { get; }

		/// <summary>Gets the navigation state before the navigation occurred.</summary>
		public ShellNavigationState Previous { get; }

		/// <summary>Gets the source of the navigation event.</summary>
		public ShellNavigationSource Source { get; }
	}
}