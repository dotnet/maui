using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellNavigatedEventArgs']/Docs" />
	public class ShellNavigatedEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ShellNavigatedEventArgs(ShellNavigationState previous, ShellNavigationState current, ShellNavigationSource source)
		{
			Previous = previous;
			Current = current;
			Source = source;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatedEventArgs.xml" path="//Member[@MemberName='Current']/Docs" />
		public ShellNavigationState Current { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatedEventArgs.xml" path="//Member[@MemberName='Previous']/Docs" />
		public ShellNavigationState Previous { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatedEventArgs.xml" path="//Member[@MemberName='Source']/Docs" />
		public ShellNavigationSource Source { get; }
	}
}