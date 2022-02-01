using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AppThemeChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.AppThemeChangedEventArgs']/Docs" />
	public class AppThemeChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/AppThemeChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public AppThemeChangedEventArgs(OSAppTheme appTheme) =>
			RequestedTheme = appTheme;

		/// <include file="../../docs/Microsoft.Maui.Controls/AppThemeChangedEventArgs.xml" path="//Member[@MemberName='RequestedTheme']/Docs" />
		public OSAppTheme RequestedTheme { get; }
	}
}