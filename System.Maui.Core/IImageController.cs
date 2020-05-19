using System;

namespace System.Maui
{
	public interface IImageController : IViewController
	{
		void SetIsLoading(bool isLoading);
		bool GetLoadAsAnimation();
	}
}