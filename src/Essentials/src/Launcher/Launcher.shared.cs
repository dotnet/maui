#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel
{
	public interface ILauncher
	{
		Task<bool> CanOpenAsync(Uri uri);

		Task<bool> OpenAsync(Uri uri);

		Task<bool> OpenAsync(OpenFileRequest request);

		Task<bool> TryOpenAsync(Uri uri);
	}

	public static partial class Launcher
	{
		static ILauncher? defaultImplementation;

		public static ILauncher Default =>
			defaultImplementation ??= new LauncherImplementation();

		internal static void SetDefault(ILauncher? implementation) =>
			defaultImplementation = implementation;
	}

	partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformCanOpenAsync(uri);
		}

		public Task<bool> OpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformOpenAsync(uri);
		}

		public Task<bool> OpenAsync(OpenFileRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.File == null)
				throw new ArgumentNullException(nameof(request.File));

			return PlatformOpenAsync(request);
		}

		public Task<bool> TryOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformTryOpenAsync(uri);
		}
	}

	public static class LauncherExtensions
	{
		public static Task<bool> CanOpenAsync(this ILauncher launcher, string uri) =>
			launcher.CanOpenAsync(new Uri(uri));

		public static Task<bool> OpenAsync(this ILauncher launcher, string uri) =>
			launcher.OpenAsync(new Uri(uri));

		public static Task<bool> TryOpenAsync(this ILauncher launcher, string uri) =>
			launcher.TryOpenAsync(new Uri(uri));
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OpenFileRequest']/Docs" />
	public class OpenFileRequest
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public OpenFileRequest()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public OpenFileRequest(string title, ReadOnlyFile file)
		{
			Title = title;
			File = file;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public OpenFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ReadOnlyFile(file);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='Title']/Docs" />
		public string? Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='File']/Docs" />
		public ReadOnlyFile? File { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='PresentationSourceBounds']/Docs" />
		public Rect PresentationSourceBounds { get; set; } = Rect.Zero;
	}
}