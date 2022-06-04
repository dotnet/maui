#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WRect = Windows.Foundation.Rect;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		ContentPanel? _contentPanel;
		ContentPanel ContentPanel => _contentPanel ??= new ContentPanel()
		{
			CrossPlatformArrange = ArrangeContentPanel,
			CrossPlatformMeasure = MeasureContentPanel
		};

		bool UsingContentPanel => _window.NativeWindow.Content == _contentPanel;

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
			if (page == null)
				return;

			var mauiContext = page.FindMauiContext() ??
				throw new InvalidOperationException("Maui Context removed from outgoing page too early");

			if (UsingContentPanel)
			{
				ContentPanel.Children.Remove(mauiContext.GetNavigationRootManager().RootView);
			}
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
				else if (_window.NativeWindow.Content != ContentPanel)
				{
					var rootContent = _window.NativeWindow.Content;
					_window.NativeWindow.Content = ContentPanel;

					if (!ContentPanel.Children.Contains(rootContent))
						ContentPanel.Children.Add(rootContent);
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

				if (newPage == null)
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

					if (UsingContentPanel)
					{
						if (!ContentPanel.Children.Contains(windowManager.RootView))
							ContentPanel.Children.Add(windowManager.RootView);
					}
					else
						_window.NativeWindow.Content = windowManager.RootView;

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

					if (UsingContentPanel)
					{
						// This means we no longer need to place the modal ontop of the content under it
						// so just remove the panel and set the window content
						if (_navModel.Modals.Count == 0)
						{
							if (ContentPanel.Children.Contains(windowManager.RootView))
								ContentPanel.Children.Remove(windowManager.RootView);

							_window.NativeWindow.Content = windowManager.RootView;
						}
						else if (!ContentPanel.Children.Contains(windowManager.RootView))
							ContentPanel.Children.Add(windowManager.RootView);
					}
					else
					{
						_window.NativeWindow.Content = windowManager.RootView;
					}

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

		Size MeasureContentPanel(double width, double height)
		{
			var size = new Size(width, height);
			var platformSize = size.ToPlatform();

			for (int i = 0; i < ContentPanel.Children.Count; i++)
			{
				ContentPanel.Children[i].Measure(platformSize);
			}

			return size;
		}

		Size ArrangeContentPanel(Rect arg)
		{
			var platformRect = new WRect(arg.Location.ToPlatform(), arg.Size.ToPlatform());
			for (int i = 0; i < ContentPanel.Children.Count; i++)
			{
				ContentPanel.Children[i].Arrange(platformRect);
			}

			return arg.Size;
		}
	}
}
