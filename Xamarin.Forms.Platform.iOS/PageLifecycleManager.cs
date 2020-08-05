using System;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class PageLifecycleManager : IDisposable
	{
		NSObject _activateObserver;
		NSObject _resignObserver;
		bool _disposed;
		bool _appeared;
		IPageController _pageController;

		public PageLifecycleManager(IPageController pageController)
		{
			_pageController = pageController ?? throw new ArgumentNullException("You need to provide a Page Element");

			_activateObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, n =>
			{
				if (CheckIfWeAreTheCurrentPage())
					HandlePageAppearing();
			});

			_resignObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillResignActiveNotification, n =>
			{
				if (CheckIfWeAreTheCurrentPage())
					HandlePageDisappearing();
			});
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_activateObserver != null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_activateObserver);
					_activateObserver = null;
				}

				if (_resignObserver != null)
				{
					NSNotificationCenter.DefaultCenter.RemoveObserver(_resignObserver);
					_resignObserver = null;
				}

				HandlePageDisappearing();

				_pageController = null;
			}

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void HandlePageAppearing()
		{
			if (_appeared)
				return;

			_appeared = true;
			_pageController?.SendAppearing();

		}

		public void HandlePageDisappearing()
		{
			if (!_appeared || _pageController == null)
				return;

			_appeared = false;
			_pageController.SendDisappearing();
		}

		public bool Appeared => _appeared;

		bool CheckIfWeAreTheCurrentPage()
		{
			if (_pageController.RealParent is IPageContainer<Page> multipage)
				return multipage.CurrentPage == _pageController;
			return true;
		}
	}
}