using System;
using System.Collections.Generic;
using System.Text;
using ElmSharp;
using TImage = Tizen.UIExtensions.ElmSharp.Image;

namespace Microsoft.Maui.Handlers
{
	//TODO: Need to impl
	public partial class ImageButtonHandler : ViewHandler<IImageButton, TImage>
	{
		protected override TImage CreateNativeView() => throw new NotImplementedException();
		
		void OnSetImageSource(TImage? obj)
		{
			throw new NotImplementedException();
		}
	}
}
