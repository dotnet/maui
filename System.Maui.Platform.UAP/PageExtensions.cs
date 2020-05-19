using System;
using System.Collections.Generic;
using System.Linq;
using global::Windows.UI.Xaml;

namespace System.Maui.Platform.UWP
{
	public sealed partial class FormsEmbeddedPageWrapper : global::Windows.UI.Xaml.Controls.Page
	{
		internal static Dictionary<Guid, ContentPage> Pages = new Dictionary<Guid, ContentPage>();

		public FormsEmbeddedPageWrapper()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo(global::Windows.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.Parameter == null)
			{
				throw new InvalidOperationException($"Cannot navigate to {nameof(FormsEmbeddedPageWrapper)} without "
					+ $"providing a {nameof(System.Maui.Page)} identifier.");
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
			if (!System.Maui.Maui.IsInitialized)
			{
				throw new InvalidOperationException("call System.Maui.Maui.Init() before this");
			}

			var root = new global::Windows.UI.Xaml.Controls.Page();

			// Yes, this looks awkward. But the page needs to be Platformed or several things won't work
			new WindowsPlatform(root);

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

		public static bool Navigate(this global::Windows.UI.Xaml.Controls.Frame frame, ContentPage page)
		{
			return Navigate(frame, page, null);
		}

		internal static bool Navigate(this global::Windows.UI.Xaml.Controls.Frame frame, ContentPage page, global::Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo infoOverride)
		{

			if (page == null)
			{
				throw new ArgumentNullException(nameof(page));
			}

			Guid id = Guid.NewGuid();

			FormsEmbeddedPageWrapper.Pages.Add(id, page);
			if (infoOverride != null)
				return frame.Navigate(typeof(FormsEmbeddedPageWrapper), id, infoOverride);

			return frame.Navigate(typeof(FormsEmbeddedPageWrapper), id);
		}

		internal static Page GetCurrentPage(this Page currentPage)
		{
			if (currentPage is MasterDetailPage mdp)
				return GetCurrentPage(mdp.Detail);
			else if (currentPage is IPageContainer<Page> pc)
				return GetCurrentPage(pc.CurrentPage);
			else
				return currentPage;
		}
	}
}