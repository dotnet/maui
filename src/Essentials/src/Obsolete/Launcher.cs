#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Launcher']/Docs" />
	public static partial class Launcher
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='CanOpenAsync'][1]/Docs" />
		public static Task<bool> CanOpenAsync(string uri)
			=> Current.CanOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='CanOpenAsync'][2]/Docs" />
		public static Task<bool> CanOpenAsync(Uri uri)
			=> Current.CanOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task<bool> OpenAsync(string uri)
			=> Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task<bool> OpenAsync(Uri uri)
			=> Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task<bool> OpenAsync(OpenFileRequest request)
			=> Current.OpenAsync(request);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='TryOpenAsync'][1]/Docs" />
		public static Task<bool> TryOpenAsync(string uri)
			=> Current.TryOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='TryOpenAsync'][2]/Docs" />
		public static Task<bool> TryOpenAsync(Uri uri)
			=> Current.TryOpenAsync(uri);

		static ILauncher Current => ApplicationModel.Launcher.Default;
	}
}
