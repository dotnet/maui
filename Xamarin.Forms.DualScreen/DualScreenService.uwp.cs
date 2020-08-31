using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
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
    internal partial class DualScreenService : IDualScreenService, Platform.UWP.DualScreen.IDualScreenService
	{
		readonly WeakEventManager _onScreenChangedEventManager = new WeakEventManager();

		public DualScreenService()
        {
			if (Window.Current != null)
			{
				Window.Current.SizeChanged += OnCurrentSizeChanged;
			}
		}

		public event EventHandler OnScreenChanged
		{
			add { _onScreenChangedEventManager.AddEventHandler(value); }
			remove { _onScreenChangedEventManager.RemoveEventHandler(value); }
		}

		public async Task<int> GetHingeAngleAsync()
		{
			if (!ApiInformation.IsMethodPresent("Windows.Devices.Sensors.HingeAngleSensor", "GetDefaultAsync"))
			{
				return await NoDualScreenServiceImpl.Instance.GetHingeAngleAsync();
			}

#if UWP_18362
			var sensor = await Windows.Devices.Sensors.HingeAngleSensor.GetDefaultAsync();

			if (sensor == null)
				return await NoDualScreenServiceImpl.Instance.GetHingeAngleAsync();

			var currentReading = await sensor.GetCurrentReadingAsync();
			return (int)currentReading.AngleInDegrees;
#else
			return await NoDualScreenServiceImpl.Instance.GetHingeAngleAsync();
#endif
		}

		void OnCurrentSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			_onScreenChangedEventManager.HandleEvent(this, EventArgs.Empty, nameof(OnScreenChanged));
		}

		public bool IsSpanned
        {
            get
            {
				var viewMode = (int)ApplicationView.GetForCurrentView().ViewMode;

				switch (viewMode)
				{
					case 2:
						return true;
					default:
						return false;
				}
            }
		}

		public DeviceInfo DeviceInfo => Device.info;

		public bool IsLandscape
        {
            get
			{
				if (!IsSpanned)
					return ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape;

#if UWP_18362
				var displayRegions = ApplicationView.GetForCurrentView().GetDisplayRegions();
				if (displayRegions.Count == 2)
				{
					// We are split in two panes. Layout accordingly
					if (displayRegions[0].WorkAreaOffset.X != displayRegions[1].WorkAreaOffset.X)
					{
						return false;
					}
					else if (displayRegions[0].WorkAreaOffset.Y != displayRegions[1].WorkAreaOffset.Y)
					{
						return true;
					}
					else
					{
						return ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape;

					}
				}
#endif
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

        public Rectangle GetHinge()
        {
			if (!ApiInformation.IsMethodPresent("Windows.UI.ViewManagement.ApplicationView", "GetSpanningRects"))
				return Rectangle.Zero;

			if (!IsSpanned)
				return Rectangle.Zero;

			var screen = DisplayInformation.GetForCurrentView();


#if UWP_18362
			var applicationView = ApplicationView.GetForCurrentView();
			List<Windows.Foundation.Rect> spanningRects = null;

#if UWP_19000
			spanningRects = applicationView.GetSpanningRects().ToList();
#endif

			if (spanningRects?.Count == 2)
			{
				if(!IsLandscape)
				{
					var x = spanningRects[0].Width;
					var hingeWidth = spanningRects[1].X - x;
					return new Rectangle(x, 0, hingeWidth, ScaledPixels(screen.ScreenHeightInRawPixels));
				}
				else
				{
					var y = spanningRects[0].Height;
					var hingeHeight = spanningRects[1].Y - y;
					return new Rectangle(0, y, ScaledPixels(screen.ScreenWidthInRawPixels), hingeHeight);
				}
			}
#endif

			// fall back to hard coded
			Rectangle returnValue = Rectangle.Zero;

			if (IsLandscape)
			{
				if (IsSpanned)
					returnValue = new Rectangle(0, 664 + 24, ScaledPixels(screen.ScreenWidthInRawPixels), 0);
				else
					returnValue = new Rectangle(0, 664, ScaledPixels(screen.ScreenWidthInRawPixels), 0);
			}
			else
				returnValue = new Rectangle(720, 0, 0, ScaledPixels(screen.ScreenHeightInRawPixels));

			return returnValue;

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