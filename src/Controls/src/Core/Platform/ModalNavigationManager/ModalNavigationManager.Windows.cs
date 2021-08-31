#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		Page? _modalBackgroundPage;

		Panel Container
		{
			get
			{
				if (_window.NativeWindow.Content is Panel p)
					return p;

				throw new InvalidOperationException("Root container Panel not found");
			}
		}

		Rectangle ContainerBounds
		{
			get { return new Rectangle(0, 0, Container.ActualWidth, Container.ActualHeight); }
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

		void AddPage(Page page)
		{
			if (Container == null || page == null)
				return;

			if (_modalBackgroundPage != null)
				_modalBackgroundPage.GetCurrentPage()?.SendDisappearing();

			page.ToNative(MauiContext);

			var pageHandler = (INativeViewHandler)page.Handler;


			if (pageHandler.ContainerView != null && !Container.Children.Contains(pageHandler.ContainerView))
				Container.Children.Add(pageHandler.ContainerView);
			else if (!Container.Children.Contains(pageHandler.NativeView))
				Container.Children.Add(pageHandler.NativeView);

			(page as IView).Measure(Container.ActualWidth, Container.ActualHeight);
			(page as IView).Arrange(ContainerBounds);

			page.Layout(ContainerBounds);
		}

		void RemovePage(Page page)
		{
			if (Container == null || page == null)
				return;

			if (_modalBackgroundPage != null)
				_modalBackgroundPage.GetCurrentPage()?.SendAppearing();

			var pageHandler = (INativeViewHandler)page.Handler;

			if (Container.Children.Contains(pageHandler.NativeView))
				Container.Children.Remove(pageHandler.NativeView);

			if (Container.Children.Contains(pageHandler.ContainerView))
				Container.Children.Remove(pageHandler.ContainerView);
		}

		partial void OnPageAttachedHandler()
		{
			if (_modalBackgroundPage != null)
			{
				RemovePage(_modalBackgroundPage);
				_modalBackgroundPage.Cleanup();
				_modalBackgroundPage.Parent = null;
			}
		}

		void SetCurrent(Page newPage, Page previousPage, bool popping, Action? completedCallback = null)
		{
			bool modal = true;
			try
			{
				if (modal && !popping && !newPage.BackgroundColor.IsDefault())
					_modalBackgroundPage = previousPage;
				else
				{
					RemovePage(previousPage);

					if (!modal && _modalBackgroundPage != null)
					{
						RemovePage(_modalBackgroundPage);
						_modalBackgroundPage.Cleanup();
						_modalBackgroundPage.Parent = null;
					}

					_modalBackgroundPage = null;
				}

				if (popping)
				{
					//// Un-parent the page; otherwise the Resources Changed Listeners won't be unhooked and the 
					//// page will leak 

					previousPage.Cleanup();
					previousPage.Parent = null;
				}

				newPage.Layout(ContainerBounds);

				AddPage(newPage);

				completedCallback?.Invoke();

				// TODO MAUI WINUI STill needs a Toolbar
				//UpdateToolbarTracker();
				//await UpdateToolbarItems();
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
