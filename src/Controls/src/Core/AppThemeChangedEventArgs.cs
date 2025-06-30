#nullable disable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AppThemeChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.AppThemeChangedEventArgs']/Docs/*" />
	public class AppThemeChangedEventArgs : EventArgs
	{
		/// <param name="appTheme">The appTheme parameter.</param>
		public AppThemeChangedEventArgs(AppTheme appTheme) =>
			RequestedTheme = appTheme;

		/// <include file="../../docs/Microsoft.Maui.Controls/AppThemeChangedEventArgs.xml" path="//Member[@MemberName='RequestedTheme']/Docs/*" />
		public AppTheme RequestedTheme { get; }
	}
}