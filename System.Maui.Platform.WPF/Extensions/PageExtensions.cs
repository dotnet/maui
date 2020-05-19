using System;
using System.Windows;

namespace Xamarin.Forms.Platform.WPF.Extensions
{
	public static class PageExtensions
	{
		public static FrameworkElement ToFrameworkElement(this Page view)
		{
			if (!Forms.IsInitialized)
				throw new InvalidOperationException("Call Forms.Init() before this");

			if (!(view.RealParent is Application))
			{
				Application app = new DefaultApplication
				{
					MainPage = view
				};

				var formsApplicationPage = new FormsApplicationPage();
				formsApplicationPage.LoadApplication(app);
				var platform = new Platform(formsApplicationPage);
				platform.SetPage(view);
			}

			IVisualElementRenderer renderer = Platform.GetOrCreateRenderer(view);

			if (renderer == null)
			{
				throw new InvalidOperationException($"Could not find or create a renderer for {view}");
			}

			var frameworkElement = renderer.GetNativeElement();

			frameworkElement.Loaded += (sender, args) =>
			{
				view.Layout(new Rectangle(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
			};

			return frameworkElement;
		}
	}

	class DefaultApplication : Application
	{
	}
}
