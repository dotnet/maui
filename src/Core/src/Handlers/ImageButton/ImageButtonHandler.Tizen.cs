using System;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	//TODO: Need to impl
	public partial class ImageButtonHandler : ViewHandler<IImageButton, Image>
	{
		protected override Image CreatePlatformView() => throw new NotImplementedException();
		
		void OnSetImageSource(Image? obj)
		{
		}
	}
}
