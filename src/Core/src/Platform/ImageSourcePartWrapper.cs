using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	internal partial class ImageSourcePartWrapper<T> : IImageSourcePart
			where T : ElementHandler
	{
		public ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		Func<T, IImageSource?>? GetSource { get; }

		Func<T, bool>? GetIsAnimationPlaying { get; }

		Action<bool>? SetIsLoading { get; }

		T Handler { get; }

		private ImageSourcePartWrapper(
			T handler,
			Func<T, IImageSource?> getSource,
			Func<T, bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading)
		{
			GetSource = getSource;
			GetIsAnimationPlaying = getIsAnimationPlaying;
			SetIsLoading = setIsLoading;
			Handler = handler;
		}

		public IImageSource? Source => GetSource?.Invoke(Handler);

		public bool IsAnimationPlaying => GetIsAnimationPlaying?.Invoke(Handler) ?? false;

		public void UpdateIsLoading(bool isLoading) => SetIsLoading?.Invoke(isLoading);

		public async Task UpdateImageSource()
		{
#if __IOS__ || __ANDROID__
			if (NativeView != null)
			{
				var token = this.SourceManager.BeginLoad();
				var provider = Handler.GetRequiredService<IImageSourceServiceProvider>();
				var result = await this.UpdateSourceAsync(NativeView, provider, SetImage!, token);
				SourceManager.CompleteLoad(result);
			}
#else
			await Task.CompletedTask;
#endif
		}
	}
}
