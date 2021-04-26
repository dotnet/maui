using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, FrameworkElement>
	{
		double _lastScale = 0.0;

		protected Image? RealNativeView { get; set; }

		protected override FrameworkElement CreateNativeView()
		{
			RealNativeView = new Image();
			return new Border { Child = RealNativeView };
		}

		protected override void ConnectHandler(FrameworkElement nativeView)
		{
			base.ConnectHandler(nativeView);

			_lastScale = 0.0;
			nativeView.Loaded += OnNativeViewLoaded;
			nativeView.Unloaded += OnNativeViewUnloaded;
		}

		protected override void DisconnectHandler(FrameworkElement nativeView)
		{
			base.DisconnectHandler(nativeView);

			if (nativeView.XamlRoot != null)
				nativeView.XamlRoot.Changed -= OnXamlRootChanged;

			_lastScale = 0.0;
			nativeView.Loaded -= OnNativeViewLoaded;
			nativeView.Unloaded -= OnNativeViewUnloaded;

			_sourceManager.Reset();
		}

		public static void MapAspect(ImageHandler handler, IImage image)
		{
			handler.RealNativeView?.UpdateAspect(image);
		}

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
		{
			handler.RealNativeView?.UpdateIsAnimationPlaying(image);
		}

		public static async void MapSource(ImageHandler handler, IImage image) =>
			await MapSourceAsync(handler, image);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.RealNativeView == null)
				return;

			var token = handler._sourceManager.BeginLoad();

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var result = await handler.RealNativeView.UpdateSourceAsync(image, provider, token);

			handler._sourceManager.CompleteLoad(result);
		}

		void OnNativeViewLoaded(object sender = null!, RoutedEventArgs e = null!)
		{
			if (NativeView?.XamlRoot != null)
			{
				_lastScale = NativeView.XamlRoot.RasterizationScale;
				NativeView.XamlRoot.Changed += OnXamlRootChanged;
			}
		}

		void OnNativeViewUnloaded(object sender = null!, RoutedEventArgs e = null!)
		{
			if (NativeView?.XamlRoot != null)
				NativeView.XamlRoot.Changed -= OnXamlRootChanged;
		}

		void OnXamlRootChanged(XamlRoot sender, XamlRootChangedEventArgs args)
		{
			if (_lastScale == sender.RasterizationScale)
				return;

			_lastScale = sender.RasterizationScale;

			if (_sourceManager.IsResolutionDependent)
				UpdateValue(nameof(IImage.Source));
		}
	}
}