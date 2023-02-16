using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	public class PopupManager : IDisposable
	{
		ITizenPlatform _platform;

		public PopupManager(ITizenPlatform platform)
		{
			_platform = platform;
			MessagingCenter.Subscribe<Page, bool>(this, Page.BusySetSignalName, OnBusySetRequest);
			MessagingCenter.Subscribe<Page, AlertArguments>(this, Page.AlertSignalName, OnAlertRequest);
			MessagingCenter.Subscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName, OnActionSheetRequest);
			MessagingCenter.Subscribe<Page, PromptArguments>(this, Page.PromptSignalName, OnPromptRequested);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingCenter.Unsubscribe<Page, AlertArguments>(this, Page.AlertSignalName);
				MessagingCenter.Unsubscribe<Page, bool>(this, Page.BusySetSignalName);
				MessagingCenter.Unsubscribe<Page, ActionSheetArguments>(this, Page.ActionSheetSignalName);
				MessagingCenter.Unsubscribe<Page, PromptArguments>(this, Page.PromptSignalName);
			}
		}

		void OnBusySetRequest(Page sender, bool enabled)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;
			// TODO. show busy popup
		}

		void OnAlertRequest(Page sender, AlertArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;
			// TODO. Show alert popup
		}

		void OnActionSheetRequest(Page sender, ActionSheetArguments arguments)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;
			// TODO. Show action sheet popup
		}

		void OnPromptRequested(Page sender, PromptArguments args)
		{
			// Verify that the page making the request is child of this platform
			if (!_platform.PageIsChildOfPlatform(sender))
				return;

			// TODO prompt popup
		}

	}
}