using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		Task<Page> PopModalPlatformCoreAsync(bool animated)
		{
			var currentPage = CurrentPlatformPage!;
			_platformModalPages.Remove(currentPage);
			return Task.FromResult(currentPage);
		}

		Task PushModalPlatformCoreAsync(Page modal, bool animated)
		{
			_platformModalPages.Add(modal);
			return Task.CompletedTask;
		}

		Task SyncModalStackWhenPlatformIsReadyCoreAsync() =>
			SyncPlatformModalStackAsync();

		bool IsModalPlatformReadyCore => true;
	}
}
