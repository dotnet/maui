using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

#if UWP_18362
using Windows.UI.WindowManagement;
#endif

using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.UWP;

[assembly: Dependency(typeof(DualScreenService))]
namespace Xamarin.Forms.DualScreen
{
    internal partial class DualScreenService : IDualScreenService
	{
#pragma warning disable CS0067
		public event EventHandler OnScreenChanged;
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

		public DualScreenService()
        {
        }

        public bool IsSpanned
        {
            get
            {
                var visibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;

                if (visibleBounds.Height > 1200 || visibleBounds.Width > 1200)
                    return true;

                return false;
            }
		}
		public DeviceInfo DeviceInfo => Device.info;

		public bool IsLandscape
        {
            get
            {
                if (IsSpanned)
                    return ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait;
                else
                    return ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape;
            }
        }

        public void Dispose()
        {
        }

        public Rectangle GetHinge()
        {
            var screen = DisplayInformation.GetForCurrentView();

            if (IsLandscape)
            {
                if (IsSpanned)
                    return new Rectangle(0, 664 + 24, ScaledPixels(screen.ScreenWidthInRawPixels), 0);
                else
                    return new Rectangle(0, 664, ScaledPixels(screen.ScreenWidthInRawPixels), 0);
            }
            else
                return new Rectangle(720, 0, 0, ScaledPixels(screen.ScreenHeightInRawPixels));
        }

        double ScaledPixels(double n)
            => n / DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

        public Point? GetLocationOnScreen(VisualElement visualElement)
        {
            var view = Platform.UWP.Platform.GetRenderer(visualElement);

            if (view?.ContainerElement == null)
                return null;

            var ttv = view.ContainerElement.TransformToVisual(Window.Current.Content);
            Windows.Foundation.Point screenCoords = ttv.TransformPoint(new Windows.Foundation.Point(0, 0));

            return new Point(screenCoords.X, screenCoords.Y);
        }

		public void WatchForChangesOnLayout(VisualElement visualElement)
		{
			var view = Platform.UWP.Platform.GetRenderer(visualElement);

			if (view?.ContainerElement == null)
				return;

			view.ContainerElement.LayoutUpdated += OnContainerElementLayoutUpdated;
		}

		public void StopWatchingForChangesOnLayout(VisualElement visualElement)
		{
			var view = Platform.UWP.Platform.GetRenderer(visualElement);

			if (view?.ContainerElement == null)
				return;

			view.ContainerElement.LayoutUpdated -= OnContainerElementLayoutUpdated;
		}

		void OnContainerElementLayoutUpdated(object sender, object e)
		{
			OnScreenChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}