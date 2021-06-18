#nullable enable

using System;

namespace Microsoft.Maui
{
	public interface IImage : IView, IImageSourcePart
	{
		Aspect Aspect { get; }

		bool IsOpaque { get; }
	}

	public interface IImageSourcePart
	{
		IImageSource? Source { get; }

		bool IsAnimationPlaying { get; }

		void UpdateIsLoading(bool isLoading);
	}

	public interface IImageSourcePartEvents
	{
		void LoadingStarted();

		void LoadingCompleted(bool successful);

		void LoadingFailed(Exception exception);
	}
}