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
		public event EventHandler OnScreenChanged;

		public DualScreenService()
        {
			if(Window.Current != null)
				Window.Current.SizeChanged += OnCurrentSizeChanged;
		}

		public Task<int> GetHingeAngleAsync() => Task.FromResult(0);

		void OnCurrentSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			OnScreenChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool IsSpanned
        {
            get
            {
                var visibleBounds = Window.Current.Bounds;

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

		public Size ScaledScreenSize
		{
			get
			{
				Windows.Foundation.Rect windowSize = Window.Current.Bounds;
				return new Size(windowSize.Width, windowSize.Height);
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

		public object WatchForChangesOnLayout(VisualElement visualElement, Action action)
		{
			var view = Platform.UWP.Platform.GetRenderer(visualElement);

			if (view?.ContainerElement == null)
				return null;

			EventHandler<object> layoutUpdated = (_, __) =>
			{
				action();
			};

			view.ContainerElement.LayoutUpdated += layoutUpdated;
			return layoutUpdated;
		}

		public void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle)
		{
			if (handle == null)
				return;

			var view = Platform.UWP.Platform.GetRenderer(visualElement);

			if (view?.ContainerElement == null)
				return;

			if(handle is EventHandler<object> handler)
				view.ContainerElement.LayoutUpdated -= handler;
		}
	}
}