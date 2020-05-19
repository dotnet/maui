using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Windows.Foundation.Metadata;
using global::Windows.UI.ViewManagement;

#if UWP_18362
using global::Windows.UI.WindowManagement;
#endif

using global::Windows.UI.Xaml.Hosting;
using System.Maui.Platform.UWP;

namespace System.Maui.DualScreen
{
	public static class DualScreenHelper
	{

#if UWP_18362

        public static bool HasCompactModeSupport()
		{
			if (!ApiInformation.IsTypePresent("global::Windows.UI.WindowManagement.AppWindow"))
			{
				return false;
			}

			return ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay);
        }

        public static async Task<CompactModeArgs> OpenCompactMode(ContentPage contentPage)
        {
			if (!ApiInformation.IsTypePresent("global::Windows.UI.WindowManagement.AppWindow"))
			{
				return new CompactModeArgs(null, false);
			}

			// 1. Create a new Window
			AppWindow appWindow = await AppWindow.TryCreateAsync();

            var frameworkElement = contentPage.CreateFrameworkElement();

            global::Windows.UI.Xaml.Controls.Frame frame = new global::Windows.UI.Xaml.Controls.Frame()
            {
                Content = frameworkElement
            };

            // 2. Create the pageand set the new window's content
            ElementCompositionPreview.SetAppWindowContent(appWindow, frame);
            CompactModeArgs args = null;

            // 3. Check if you can leverage the compact overlay APIs
            if (appWindow.Presenter.IsPresentationSupported(AppWindowPresentationKind.CompactOverlay))
            {
                // 4. Show the window
                bool result = await appWindow.TryShowAsync();

                if (result)
                {
					bool windowClosed = false;
                    appWindow.Presenter.RequestPresentation(AppWindowPresentationKind.CompactOverlay);
                    frame.SizeChanged += OnFrameSizeChanged;
					appWindow.Closed += AppWindow_Closed;


					args = new CompactModeArgs(async () =>
					{
						if (windowClosed)
							return;

						frame.SizeChanged -= OnFrameSizeChanged;
						await appWindow.CloseAsync();
					}, 
					true);


					void OnFrameSizeChanged(object sender, global::Windows.UI.Xaml.SizeChangedEventArgs e)
                    {
						if (windowClosed)
							return;

                        contentPage.HeightRequest = frame.ActualWidth;
                        contentPage.WidthRequest = frame.ActualHeight;
						Layout.LayoutChildIntoBoundingRegion(contentPage, new Rectangle(0, 0, frame.ActualWidth, frame.ActualHeight));
					}

					void AppWindow_Closed(AppWindow sender, AppWindowClosedEventArgs a)
					{
						frame.SizeChanged -= OnFrameSizeChanged;
						windowClosed = true;
					}
                }
            }


            if (args == null)
            {
                args = new CompactModeArgs(null, false);
            }

            return args;
        }

#else
		public static bool HasCompactModeSupport()
        {
            return false;
        }

        public static Task<CompactModeArgs> OpenCompactMode(ContentPage contentPage)
        {
			return Task.FromResult(new CompactModeArgs(null, false));
		}

#endif
	}
}
