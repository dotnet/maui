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
}