using System;

namespace Microsoft.Maui.Controls
{
	public interface IImageController : IViewController
	{
		void SetIsLoading(bool isLoading);
		bool GetLoadAsAnimation();
	}
}