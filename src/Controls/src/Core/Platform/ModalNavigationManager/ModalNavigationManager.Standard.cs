// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

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
