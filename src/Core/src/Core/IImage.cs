#nullable enable

using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that holds an image.
	/// </summary>
	public interface IImage : IView, IImageSourcePart
	{
		/// <summary>
		/// Gets the scaling mode for the image.
		/// </summary>
		Aspect Aspect { get; }

		/// <summary>
		/// Gets or sets a Boolean value that, if true hints to the rendering engine that it may safely omit drawing visual elements behind the image.
		/// </summary>
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