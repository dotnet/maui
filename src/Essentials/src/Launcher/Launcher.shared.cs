using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Essentials
{
	public interface ILauncher
	{
		Task<bool> CanOpenAsync(string uri);

		Task<bool> CanOpenAsync(Uri uri);

		Task<bool> OpenAsync(string uri);

		Task<bool> OpenAsync(Uri uri);

		Task<bool> OpenAsync(OpenFileRequest request);
		
		Task<bool> TryOpenAsync(string uri);

		Task<bool> TryOpenAsync(Uri uri);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Launcher']/Docs" />
	public static partial class Launcher
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='CanOpenAsync'][0]/Docs" />
		public static Task<bool> CanOpenAsync(string uri)
			=> Current.CanOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='CanOpenAsync'][1]/Docs" />
		public static Task<bool> CanOpenAsync(Uri uri)
			=> Current.CanOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][0]/Docs" />
		public static Task<bool> OpenAsync(string uri)
			=> Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task<bool> OpenAsync(Uri uri)
			=> Current.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task<bool> OpenAsync(OpenFileRequest request)
			=> Current.OpenAsync(request);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='TryOpenAsync'][0]/Docs" />
		public static Task<bool> TryOpenAsync(string uri)
			=> Current.TryOpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Launcher.xml" path="//Member[@MemberName='TryOpenAsync'][1]/Docs" />
		public static Task<bool> TryOpenAsync(Uri uri)
			=> Current.TryOpenAsync(uri);

#nullable enable
		static ILauncher? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static ILauncher Current =>
			currentImplementation ??= new Implementations.LauncherImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(ILauncher? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OpenFileRequest']/Docs" />
	public class OpenFileRequest
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public OpenFileRequest()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public OpenFileRequest(string title, ReadOnlyFile file)
		{
			Title = title;
			File = file;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public OpenFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ReadOnlyFile(file);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='File']/Docs" />
		public ReadOnlyFile File { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OpenFileRequest.xml" path="//Member[@MemberName='PresentationSourceBounds']/Docs" />
		public Rect PresentationSourceBounds { get; set; } = Rect.Zero;
	}
}

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException(nameof(uri));

			return CanOpenAsync(new Uri(uri));
		}

		public Task<bool> CanOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformCanOpenAsync(uri);
		}

		public Task<bool> OpenAsync(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException(nameof(uri));

			return OpenAsync(new Uri(uri));
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

		public Task<bool> TryOpenAsync(string uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException(nameof(uri));

			return TryOpenAsync(new Uri(uri));
		}

		public Task<bool> TryOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformTryOpenAsync(uri);
		}
	}
}