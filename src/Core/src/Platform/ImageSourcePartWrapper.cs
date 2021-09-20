using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader : IImageSourcePart
	{
		public ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		Func<IImageSource?>? GetSource { get; }

		Func<bool>? GetIsAnimationPlaying { get; }

		Action<bool>? SetIsLoading { get; }

		IElementHandler Handler { get; }

		internal ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSource?> getSource,
			Func<bool>? getIsAnimationPlaying,
			Action<bool>? setIsLoading)
		{
			Handler = handler;
		}

		public void Reset()
		{
			SourceManager.Reset();
		}

		IImageSource? IImageSourcePart.Source => GetSource?.Invoke();

		bool IImageSourcePart.IsAnimationPlaying => GetIsAnimationPlaying?.Invoke() ?? false;

		void IImageSourcePart.UpdateIsLoading(bool isLoading) => SetIsLoading?.Invoke(isLoading);

		public async Task UpdateImageSourceAsync()
		{
#if __IOS__ || __ANDROID__ || WINDOWS
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
