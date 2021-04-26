﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreateNativeView() => new MauiImageView();

		protected override void ConnectHandler(UIImageView nativeView)
		{
			base.ConnectHandler(nativeView);

			if (NativeView is MauiImageView imageView)
				imageView.WindowChanged += OnWindowChanged;
		}

		protected override void DisconnectHandler(UIImageView nativeView)
		{
			base.DisconnectHandler(nativeView);

			if (NativeView is MauiImageView imageView)
				imageView.WindowChanged -= OnWindowChanged;
		}

		public static void MapAspect(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateAspect(image);
		}

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateIsAnimationPlaying(image);
		}

		public static async void MapSource(ImageHandler handler, IImage image) =>
			await MapSourceAsync(handler, image);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.NativeView == null)
				return;

			var token = handler._sourceManager.BeginLoad();

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var result = await handler.NativeView.UpdateSourceAsync(image, provider, token);

			handler._sourceManager.CompleteLoad(result);
		}

		void OnWindowChanged(object? sender, EventArgs e)
		{
			if (_sourceManager.IsResolutionDependent)
				UpdateValue(nameof(IImage.Source));
		}
	}
}