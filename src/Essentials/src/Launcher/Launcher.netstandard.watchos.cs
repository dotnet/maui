using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Launcher']/Docs" />
	public partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(string uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> CanOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task OpenAsync(string uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task OpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task OpenAsync(OpenFileRequest request) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> TryOpenAsync(string uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<bool> TryOpenAsync(Uri uri) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
