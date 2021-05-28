#nullable enable
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		double _lastScale = 0.0;

		protected override Image CreateNativeView() => new Image();

		protected override void ConnectHandler(Image nativeView)
		{
			base.ConnectHandler(nativeView);

			_lastScale = 0.0;
			nativeView.Loaded += OnNativeViewLoaded;
			nativeView.Unloaded += OnNativeViewUnloaded;
		}

		protected override void DisconnectHandler(Image nativeView)
		{
			base.DisconnectHandler(nativeView);

			if (nativeView.XamlRoot != null)
				nativeView.XamlRoot.Changed -= OnXamlRootChanged;

			_lastScale = 0.0;
			nativeView.Loaded -= OnNativeViewLoaded;
			nativeView.Unloaded -= OnNativeViewUnloaded;

			_sourceManager.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(ImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));

			handler.WrappedNativeView?.UpdateBackground(image);
		}

		public static void MapAspect(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(ImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.NativeView == null)
				return;

			var token = handler._sourceManager.BeginLoad();

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var result = await handler.NativeView.UpdateSourceAsync(image, provider, token);

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