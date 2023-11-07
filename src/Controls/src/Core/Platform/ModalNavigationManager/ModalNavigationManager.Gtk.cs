#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		Task<Page> PopModalPlatformAsync(bool animated)
		{
			var currentPage = CurrentPlatformPage!;
			_platformModalPages.Remove(currentPage);
			return Task.FromResult(currentPage);
		}

		Task PushModalPlatformAsync(Page modal, bool animated)
		{
			_platformModalPages.Add(modal);
			return Task.CompletedTask;
		}

		Task SyncModalStackWhenPlatformIsReadyAsync() =>
			SyncPlatformModalStackAsync();

		bool IsModalPlatformReady => true;
	}
}
