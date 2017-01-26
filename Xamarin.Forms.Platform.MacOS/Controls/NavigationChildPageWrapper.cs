using System;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class NavigationChildPageWrapper : NSObject
	{
		bool _disposed;

		public NavigationChildPageWrapper(Page page)
		{
			Page = page;
			Page.PropertyChanged += PagePropertyChanged;
			Identifier = Guid.NewGuid().ToString();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Page != null)
					Page.PropertyChanged -= PagePropertyChanged;
				Page = null;
			}
			base.Dispose(disposing);
		}

		void PagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName
				|| e.PropertyName == Page.TitleProperty.PropertyName
				|| e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
				Platform.NativeToolbarTracker.UpdateToolBar();
		}

		public string Identifier { get; set; }

		public Page Page { get; private set; }
	}
}