using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Lets a backend built over the platform-less (<c>net10.0</c>) assemblies show modal pages (the GTK backend opens a modal window).
	/// With the default <see langword="null"/> <c>ModalNavigationBackend.Current</c> modal pages are only tracked, as before.
	/// </summary>
	internal interface IModalNavigationBackend
	{
		/// <summary>Shows <paramref name="modal"/> above <paramref name="previous"/>; completes when it is on screen.</summary>
		Task PushAsync(Window window, Page modal, Page previous, bool animated);

		/// <summary>Removes <paramref name="popped"/>, revealing <paramref name="next"/>; completes when it is gone.</summary>
		Task PopAsync(Window window, Page popped, Page next, bool animated);
	}

	internal static class ModalNavigationBackend
	{
		/// <summary>The backend, or <see langword="null"/> when modal pages are not presented natively.</summary>
		public static IModalNavigationBackend? Current { get; set; }
	}

	internal partial class ModalNavigationManager
	{
		async Task<Page> PopModalPlatformAsync(bool animated)
		{
			var currentPage = CurrentPlatformPage!;
			_platformModalPages.Remove(currentPage);

			if (ModalNavigationBackend.Current is { } backend)
				await backend.PopAsync(_window, currentPage, CurrentPlatformPage, animated);

			return currentPage;
		}

		async Task PushModalPlatformAsync(Page modal, bool animated)
		{
			var previousPage = CurrentPlatformPage;
			_platformModalPages.Add(modal);

			if (ModalNavigationBackend.Current is { } backend)
				await backend.PushAsync(_window, modal, previousPage, animated);
		}

		Task SyncModalStackWhenPlatformIsReadyAsync() =>
			SyncPlatformModalStackAsync();

		bool IsModalPlatformReady => true;
	}
}
