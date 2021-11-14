using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public class ShellNavigatingEventArgs : DeferrableEventArgs
	{
		public ShellNavigatingEventArgs(ShellNavigationState current, ShellNavigationState target, ShellNavigationSource source, bool canCancel)
			: base(canCancel)
		{
			this.Current = current;
			this.Target = target;
			this.Source = source;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Current']/Docs" />
		public ShellNavigationState Current { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Target']/Docs" />
		public ShellNavigationState Target { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigatingEventArgs.xml" path="//Member[@MemberName='Source']/Docs" />
		public ShellNavigationSource Source { get; }
	}
}