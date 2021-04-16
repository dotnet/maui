namespace Microsoft.Maui
{
	public interface IImage : IView, IImageSourcePart
	{
		Aspect Aspect { get; }

		bool IsOpaque { get; }
	}

	public interface IImageSourcePart : IView
	{
		IImageSource? Source { get; }

		bool IsAnimationPlaying { get; }

		void UpdateIsLoading(bool isLoading);
	}
}