using System.Collections.ObjectModel;
using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class PageRenderer : VisualElementRenderer<Page, FrameworkElement>
	{
		bool _disposed;

		bool _loaded;

		IPageController PageController => Element as IPageController;

		protected override void Dispose(bool disposing)
		{
			if (!disposing || _disposed)
				return;

			_disposed = true;

			if (Element != null)
			{
				ReadOnlyCollection<Element> children = ((IElementController)Element).LogicalChildren;
				for (var i = 0; i < children.Count; i++)
				{
					var visualChild = children[i] as VisualElement;
					visualChild?.Cleanup();
				}
				PageController?.SendDisappearing();
			}

			base.Dispose();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			((IPageController)e.OldElement)?.SendDisappearing();

			if (e.NewElement != null)
			{
				if (e.OldElement == null)
				{
					Loaded += OnLoaded;
					Unloaded += OnUnloaded;

					Tracker = new BackgroundTracker<FrameworkElement>(BackgroundProperty);
				}

				if (_loaded)
					((IPageController)e.NewElement).SendAppearing();
			}
		}

		void OnLoaded(object sender, RoutedEventArgs args)
		{
			var carouselPage = Element?.Parent as CarouselPage;
			if (carouselPage != null && carouselPage.Children[0] != Element)
			{
				return;
			}
			_loaded = true;
			PageController?.SendAppearing();
		}

		void OnUnloaded(object sender, RoutedEventArgs args)
		{
			_loaded = false;
			PageController?.SendDisappearing();
		}
	}
}