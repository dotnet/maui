using System;

namespace Xamarin.Forms
{
	public interface IImageController : IViewController
	{
		void SetIsLoading(bool isLoading);
		bool GetLoadAsAnimation();
	}
}