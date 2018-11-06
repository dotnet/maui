using System;

namespace Xamarin.Forms
{
	public interface IImageController : IViewController
	{
		void SetIsLoading(bool isLoading);
		Aspect Aspect { get; }
		ImageSource Source { get; }
		bool IsOpaque { get; }
		void RaiseImageSourcePropertyChanged();
		BindableProperty SourceProperty { get; }
		BindableProperty AspectProperty { get; }
		BindableProperty IsOpaqueProperty { get; }
	}
}