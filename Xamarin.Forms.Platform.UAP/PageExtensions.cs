using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	public sealed partial class FormsEmbeddedPageWrapper : Windows.UI.Xaml.Controls.Page
	{
		internal static Dictionary<Guid, ContentPage> Pages = new Dictionary<Guid, ContentPage>();

		public FormsEmbeddedPageWrapper()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.Parameter == null)
			{
				throw new InvalidOperationException($"Cannot navigate to {nameof(FormsEmbeddedPageWrapper)} without " 
					+ $"providing a {nameof(Xamarin.Forms.Page)} identifier.");
			}

			// Find the page instance in the dictionary and then discard it so we don't prevent it from being collected
			var key = (Guid)e.Parameter;
			var page = Pages[key];
			Pages.Remove(key); 

			// Convert that page into a FrameWorkElement we can display in the ContentPresenter
			FrameworkElement frameworkElement = page.CreateFrameworkElement();

			if (frameworkElement == null)
			{
				throw new InvalidOperationException($"Could not find or create a renderer for the Page {page}");
			}

			Root.Content = frameworkElement;
		}
	}

	public static class PageExtensions
	{
		public static FrameworkElement CreateFrameworkElement(this ContentPage contentPage)
		{
			return contentPage.ToFrameworkElement();
		}

		internal static FrameworkElement ToFrameworkElement(this VisualElement visualElement)
		{
			if (!Forms.IsInitialized)
			{
				throw new InvalidOperationException("call Forms.Init() before this");
			}

			var root = new Windows.UI.Xaml.Controls.Page();

			new WindowsPlatform(root).SetPlatformDisconnected(visualElement);

			var renderer = visualElement.GetOrCreateRenderer();

			if (renderer == null)
			{
				throw new InvalidOperationException($"Could not find or create a renderer for {visualElement}");
			}

			var frameworkElement = renderer.ContainerElement;

			frameworkElement.Loaded += (sender, args) =>
			{
				visualElement.Layout(new Rectangle(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
			};

			return frameworkElement;
		}

		public static bool Navigate(this Windows.UI.Xaml.Controls.Frame frame, ContentPage page)
		{
			if (page == null)
			{
				throw new ArgumentNullException(nameof(page));
			}

			Guid id = Guid.NewGuid();
			FormsEmbeddedPageWrapper.Pages.Add(id, page);
			return frame.Navigate(typeof(FormsEmbeddedPageWrapper), id);
		}
	}
}