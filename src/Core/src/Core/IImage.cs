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

	/// <summary>
	/// Provides images to show while the <see cref="IImageSourcePart.Source"/> of an <see cref="IImageSourcePart"/> is loading or after it failed to load.
	/// </summary>
	/// <remarks>These images are only displayed; they do not affect <see cref="IImageSourcePart.UpdateIsLoading(bool)"/> or <see cref="IImageSourcePartEvents"/>.</remarks>
	public interface IImageSourcePartFallbacks
	{
		/// <summary>
		/// Gets the image to show while <see cref="IImageSourcePart.Source"/> is loading, or <see langword="null"/> to show nothing.
		/// </summary>
		/// <remarks>The placeholder is not shown when the source is available immediately, for example from a memory cache.</remarks>
		IImageSource? PlaceholderSource { get; }

		/// <summary>
		/// Gets the image to show when <see cref="IImageSourcePart.Source"/> fails to load, or <see langword="null"/> to show nothing.
		/// </summary>
		IImageSource? ErrorSource { get; }

		/// <summary>
		/// Gets a value indicating whether the image that is shown stays visible while a new <see cref="IImageSourcePart.Source"/> loads,
		/// instead of the placeholder (or nothing).
		/// </summary>
		/// <remarks>
		/// Only an image that loaded successfully is kept. When the new source fails to load, the error image (or nothing) is shown.
		/// </remarks>
		bool KeepPreviousImageWhileLoading { get; }
	}

	public interface IImageSourcePartEvents
	{
		void LoadingStarted();

		void LoadingCompleted(bool successful);

		void LoadingFailed(Exception exception);
	}
}