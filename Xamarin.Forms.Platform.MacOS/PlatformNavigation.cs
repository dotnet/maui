using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class PlatformNavigation : INavigation, IDisposable
	{
		ModalPageTracker _modalTracker;
		PlatformRenderer _platformRenderer;
		bool _animateModals;
		bool _disposed;

		public PlatformNavigation(PlatformRenderer mainRenderer)
		{
			_platformRenderer = mainRenderer;
			_modalTracker = new ModalPageTracker(_platformRenderer);
			_animateModals = true;
		}

		public ModalPageTracker ModalPageTracker => _modalTracker;

		public IReadOnlyList<Page> ModalStack => _modalTracker.ModalStack;

		public IReadOnlyList<Page> NavigationStack => new List<Page>();

		public bool AnimateModalPages
		{
			get { return _animateModals; }
			set { _animateModals = value; }
		}

		Task<Page> INavigation.PopAsync()
		{
			return ((INavigation)this).PopAsync(true);
		}

		Task<Page> INavigation.PopAsync(bool animated)
		{
			throw new InvalidOperationException("PopAsync is not supported globally on MacOS, please use a NavigationPage.");
		}

		Task INavigation.PopToRootAsync()
		{
			return ((INavigation)this).PopToRootAsync(true);
		}

		Task INavigation.PopToRootAsync(bool animated)
		{
			throw new InvalidOperationException("PopToRootAsync is not supported globally on MacOS, please use a NavigationPage.");
		}

		Task INavigation.PushAsync(Page root)
		{
			return ((INavigation)this).PushAsync(root, true);
		}

		Task INavigation.PushAsync(Page root, bool animated)
		{
			throw new InvalidOperationException("PushAsync is not supported globally on MacOS, please use a NavigationPage.");
		}

		Task INavigation.PushModalAsync(Page modal)
		{
			return ((INavigation)this).PushModalAsync(modal, true);
		}

		Task<Page> INavigation.PopModalAsync()
		{
			return ((INavigation)this).PopModalAsync(true);
		}

		Task INavigation.PushModalAsync(Page modal, bool animated)
		{
			modal.Platform = _platformRenderer.Platform;
			return _modalTracker.PushAsync(modal, _animateModals && animated);
		}

		Task<Page> INavigation.PopModalAsync(bool animated)
		{
			return _modalTracker.PopAsync(animated);
		}

		void INavigation.RemovePage(Page page)
		{
			throw new InvalidOperationException("RemovePage is not supported globally on macOS, please use a NavigationPage.");
		}

		void INavigation.InsertPageBefore(Page page, Page before)
		{
			throw new InvalidOperationException(
				"InsertPageBefore is not supported globally on macOS, please use a NavigationPage.");
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_modalTracker.Dispose();
					_modalTracker = null;
					_platformRenderer = null;
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}