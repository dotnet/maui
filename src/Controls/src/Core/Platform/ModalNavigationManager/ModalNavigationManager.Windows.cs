#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		WindowRootViewContainer Container =>
			_window.NativeWindow.Content as WindowRootViewContainer ??
			throw new InvalidOperationException("Root container Panel not found");

		public Task<Page> PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			var currentPage = _navModel.CurrentPage;
			Page result = _navModel.PopModal();
			SetCurrent(_navModel.CurrentPage, currentPage, true, () => tcs.SetResult(result));
			return tcs.Task;
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			_ = modal ?? throw new ArgumentNullException(nameof(modal));

			var tcs = new TaskCompletionSource<bool>();
			var currentPage = _navModel.CurrentPage;
			_navModel.PushModal(modal);
			SetCurrent(modal, currentPage, false, () => tcs.SetResult(true));
			return tcs.Task;
		}

		void RemovePage(Page page)
		{
			if (page == null)
				return;

			var mauiContext = page.FindMauiContext() ??
				throw new InvalidOperationException("Maui Context removed from outgoing page too early");

			var windowManager = mauiContext.GetNavigationRootManager();
			Container.RemovePage(windowManager.RootView);
		}

		void SetCurrent(Page newPage, Page previousPage, bool popping, Action? completedCallback = null)
		{
			try
			{
				if (popping)
				{
					RemovePage(previousPage);
				}
				else if (newPage.BackgroundColor.IsDefault() && newPage.Background.IsEmpty)
				{
					RemovePage(previousPage);
				}

				if (popping)
				{
					previousPage
						.FindMauiContext()
						?.GetNavigationRootManager()
						?.Disconnect();

					previousPage.Handler = null;

					// Un-parent the page; otherwise the Resources Changed Listeners won't be unhooked and the
					// page will leak
					previousPage.Parent = null;
				}

				if (Container == null || newPage == null)
					return;

				// pushing modal
				if (!popping)
				{
					var modalContext =
						WindowMauiContext
							.MakeScoped(registerNewNavigationRoot: true);

					newPage.Toolbar ??= new Toolbar(newPage);
					_ = newPage.Toolbar.ToPlatform(modalContext);

					var windowManager = modalContext.GetNavigationRootManager();
					windowManager.Connect(newPage.ToPlatform(modalContext));
					Container.AddPage(windowManager.RootView);

					previousPage
						.FindMauiContext()
						?.GetNavigationRootManager()
						?.UpdateAppTitleBar(false);
				}
				// popping modal
				else
				{
					var windowManager = newPage.FindMauiContext()?.GetNavigationRootManager() ??
						throw new InvalidOperationException("Previous Page Has Lost its MauiContext");

					Container.AddPage(windowManager.RootView);

					windowManager.UpdateAppTitleBar(true);
				}

				completedCallback?.Invoke();
			}
			catch (Exception error) when (error.HResult == -2147417842)
			{
				//This exception prevents the Main Page from being changed in a child
				//window or a different thread, except on the Main thread.
				//HEX 0x8001010E 
				throw new InvalidOperationException(
					"Changing the current page is only allowed if it's being called from the same UI thread." +
					"Please ensure that the new page is in the same UI thread as the current page.", error);
			}
		}
	}
}
