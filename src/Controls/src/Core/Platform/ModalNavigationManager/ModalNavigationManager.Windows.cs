#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		Panel Container
		{
			get
			{
				if (_window.NativeWindow.Content is Panel p)
					return p;

				throw new InvalidOperationException("Root container Panel not found");
			}
		}

		public  Task<Page> PopModalAsync(bool animated)
		{
			var tcs = new TaskCompletionSource<Page>();
			var currentPage = _navModel.CurrentPage;
			Page result = _navModel.PopModal();
			SetCurrent(_navModel.CurrentPage, currentPage, true, () => tcs.SetResult(result));
			return tcs.Task;
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			if (modal == null)
				throw new ArgumentNullException(nameof(modal));

			var tcs = new TaskCompletionSource<bool>();
			var currentPage = _navModel.CurrentPage;
			_navModel.PushModal(modal);
			SetCurrent(modal, currentPage, false, () => tcs.SetResult(true));
			return tcs.Task;
		}

		void RemovePage(Page page)
		{
			if (Container == null || page == null)
				return;

			var mauiContext = page.FindMauiContext() ??
				throw new InvalidOperationException("Maui Context removed from outgoing page too early");

			Container.Children.Remove(mauiContext.GetNavigationRootManager().RootView);
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

				if (!popping)
				{
					var modalContext =
						MauiContext
							.MakeScoped(registerNewNavigationRoot: true);

					newPage.Toolbar ??= new Toolbar(newPage);
					_ = newPage.Toolbar.ToPlatform(modalContext);

					var windowManager = modalContext.GetNavigationRootManager();
					windowManager.Connect(newPage);
					Container.Children.Add(windowManager.RootView);

					previousPage
						.FindMauiContext()
						?.GetNavigationRootManager()
						?.UpdateAppTitleBar(false);
				}
				else
				{
					var windowManager = newPage.FindMauiContext()?.GetNavigationRootManager() ??
						throw new InvalidOperationException("Previous Page Has Lost its MauiContext");

					if(!Container.Children.Contains(windowManager.RootView))
						Container.Children.Add(windowManager.RootView);

					windowManager.UpdateAppTitleBar(true);
				}

				completedCallback?.Invoke();
			}
			catch (Exception error)
			{
				//This exception prevents the Main Page from being changed in a child 
				//window or a different thread, except on the Main thread. 
				//HEX 0x8001010E 
				if (error.HResult == -2147417842)
					throw new InvalidOperationException("Changing the current page is only allowed if it's being called from the same UI thread." +
						"Please ensure that the new page is in the same UI thread as the current page.");
				throw;
			}
		}
	}
}
